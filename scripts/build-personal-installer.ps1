param(
    [Parameter(Mandatory = $true)]
    [string]$PackageDirectory,
    [Parameter(Mandatory = $true)]
    [ValidatePattern("^[0-9]+\.[0-9]+\.[0-9]+$")]
    [string]$Version,
    [Parameter(Mandatory = $true)]
    [string]$InnoCompilerPath,
    [Parameter(Mandatory = $true)]
    [string]$SignToolPath,
    [Parameter(Mandatory = $true)]
    [string]$CertificateThumbprint,
    [Parameter(Mandatory = $true)]
    [string]$TimestampUrl,
    [string]$OutputDirectory
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
$artifactRoot = [IO.Path]::GetFullPath((Join-Path $repoRoot ".artifacts"))
$artifactPrefix = $artifactRoot.TrimEnd([IO.Path]::DirectorySeparatorChar) +
    [IO.Path]::DirectorySeparatorChar
$utf8WithoutBom = [Text.UTF8Encoding]::new($false)

function Resolve-ArtifactDirectory {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Label,
        [switch]$MustExist
    )

    $resolved = if ([IO.Path]::IsPathRooted($Path)) {
        [IO.Path]::GetFullPath($Path)
    } else {
        [IO.Path]::GetFullPath((Join-Path $repoRoot $Path))
    }
    if (-not $resolved.StartsWith($artifactPrefix, [StringComparison]::OrdinalIgnoreCase)) {
        throw "$Label must be under: $artifactRoot"
    }
    if ($MustExist -and -not (Test-Path -LiteralPath $resolved -PathType Container)) {
        throw "$Label does not exist: $resolved"
    }
    return $resolved
}

function Resolve-RequiredTool {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$ExpectedName,
        [Parameter(Mandatory = $true)][string]$Label
    )

    if (-not [IO.Path]::IsPathRooted($Path)) {
        throw "$Label path must be fully qualified."
    }
    $resolved = [IO.Path]::GetFullPath($Path)
    if (-not (Test-Path -LiteralPath $resolved -PathType Leaf) -or
        -not [string]::Equals(
            [IO.Path]::GetFileName($resolved),
            $ExpectedName,
            [StringComparison]::OrdinalIgnoreCase)) {
        throw "$Label must point to an existing $ExpectedName."
    }
    $tool = Get-Item -LiteralPath $resolved
    if (($tool.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
        throw "$Label cannot be a reparse point."
    }
    return $resolved
}

function Test-ApprovedTimestampUri {
    param([Parameter(Mandatory = $true)][Uri]$Uri)

    if (-not [string]::IsNullOrEmpty($Uri.UserInfo) -or
        -not [string]::IsNullOrEmpty($Uri.Fragment)) {
        return $false
    }
    if ($Uri.Scheme -eq [Uri]::UriSchemeHttps) {
        return $true
    }

    return $Uri.Scheme -eq [Uri]::UriSchemeHttp -and
        $Uri.IsDefaultPort -and
        [string]::Equals(
            $Uri.Host,
            "timestamp.digicert.com",
            [StringComparison]::OrdinalIgnoreCase) -and
        $Uri.AbsolutePath -eq "/" -and
        [string]::IsNullOrEmpty($Uri.Query)
}

$PackageDirectory = Resolve-ArtifactDirectory `
    -Path $PackageDirectory `
    -Label "PackageDirectory" `
    -MustExist

$candidateVerifier = Join-Path $PSScriptRoot "verify-signed-release-candidate.ps1"
& powershell -NoProfile -ExecutionPolicy Bypass -File $candidateVerifier `
    -PackageDirectory $PackageDirectory
if ($LASTEXITCODE -ne 0) {
    throw "Signed release candidate verification failed."
}

$packageManifestPath = Join-Path $PackageDirectory "package-manifest.json"
$packageManifest = Get-Content -LiteralPath $packageManifestPath -Raw | ConvertFrom-Json
$packageSigner = ([string]$packageManifest.App.SignerThumbprint).ToUpperInvariant()
if ($packageManifest.PackageKind -ne "SignedReleaseCandidate" -or
    $packageManifest.ValidSameSigner -ne $true -or
    -not [string]::Equals(
        $packageSigner,
        [string]$packageManifest.Worker.SignerThumbprint,
        [StringComparison]::OrdinalIgnoreCase)) {
    throw "Package manifest does not contain valid same-signer evidence."
}

$normalizedThumbprint = ($CertificateThumbprint -replace "\s", "").ToUpperInvariant()
if ($normalizedThumbprint -notmatch "^[0-9A-F]{40}$") {
    throw "Certificate thumbprint must contain exactly 40 hexadecimal characters."
}
if (-not [string]::Equals(
    $normalizedThumbprint,
    $packageSigner,
    [StringComparison]::OrdinalIgnoreCase)) {
    throw "Installer signer must match the signed App and worker."
}

$certificatePath = Join-Path "Cert:\CurrentUser\My" $normalizedThumbprint
if (-not (Test-Path -LiteralPath $certificatePath -PathType Leaf)) {
    throw "Code-signing certificate was not found in Cert:\CurrentUser\My."
}
$certificate = Get-Item -LiteralPath $certificatePath
if (-not $certificate.HasPrivateKey) {
    throw "Code-signing certificate does not have an accessible private key."
}
$now = Get-Date
if ($certificate.NotBefore -gt $now -or $certificate.NotAfter -le $now) {
    throw "Code-signing certificate is not currently valid."
}

$timestampUri = $null
if (-not [Uri]::TryCreate($TimestampUrl, [UriKind]::Absolute, [ref]$timestampUri) -or
    -not (Test-ApprovedTimestampUri -Uri $timestampUri)) {
    throw "TimestampUrl must be an absolute HTTPS endpoint or the approved official HTTP RFC3161 endpoint."
}

$resolvedCompiler = Resolve-RequiredTool `
    -Path $InnoCompilerPath `
    -ExpectedName "ISCC.exe" `
    -Label "InnoCompilerPath"
$resolvedSignTool = Resolve-RequiredTool `
    -Path $SignToolPath `
    -ExpectedName "signtool.exe" `
    -Label "SignToolPath"

if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path $artifactRoot "OMNIX-Entropy-installer-v$Version"
}
$OutputDirectory = Resolve-ArtifactDirectory `
    -Path $OutputDirectory `
    -Label "OutputDirectory"
if (-not [string]::Equals(
    [IO.Path]::GetDirectoryName($OutputDirectory),
    $artifactRoot,
    [StringComparison]::OrdinalIgnoreCase)) {
    throw "OutputDirectory must be a new direct child of: $artifactRoot"
}
if (Test-Path -LiteralPath $OutputDirectory) {
    throw "OutputDirectory already exists: $OutputDirectory"
}

$installerDefinition = Join-Path $repoRoot "installer\OMNIX-Entropy.iss"
if (-not (Test-Path -LiteralPath $installerDefinition -PathType Leaf)) {
    throw "Installer definition is missing."
}

$signCommand = '$q' + $resolvedSignTool + '$q sign /s My /sha1 ' +
    $normalizedThumbprint + ' /fd SHA256 /tr $q' + $TimestampUrl +
    '$q /td SHA256 $f'
$compilerArguments = @(
    "/DMyAppVersion=$Version",
    "/DSourcePackage=$PackageDirectory",
    "/DOutputDirectory=$OutputDirectory",
    "/Somnix=$signCommand",
    $installerDefinition
)
& $resolvedCompiler @compilerArguments
if ($LASTEXITCODE -ne 0) {
    throw "Inno Setup compilation failed with exit code $LASTEXITCODE."
}

$installerName = "OMNIX-Entropy-$Version-win-x64-setup.exe"
$installerPath = Join-Path $OutputDirectory $installerName
if (-not (Test-Path -LiteralPath $installerPath -PathType Leaf)) {
    throw "Expected installer output is missing: $installerName"
}

$signature = Get-AuthenticodeSignature -LiteralPath $installerPath
if ($signature.Status.ToString() -ne "Valid" -or
    $null -eq $signature.SignerCertificate -or
    $null -eq $signature.TimeStamperCertificate) {
    throw "Installer signature or timestamp verification failed."
}
if (-not [string]::Equals(
    $signature.SignerCertificate.Thumbprint,
    $packageSigner,
    [StringComparison]::OrdinalIgnoreCase)) {
    throw "Installer signer does not match the signed App and worker."
}

$installerFile = Get-Item -LiteralPath $installerPath
$installerHash = Get-FileHash -LiteralPath $installerPath -Algorithm SHA256
$sourceManifestHash = Get-FileHash `
    -LiteralPath $packageManifestPath `
    -Algorithm SHA256
$manifest = [ordered]@{
    SchemaVersion = 1
    Product = "OMNIX-Entropy"
    PackageKind = "PersonalWindowsInstaller"
    Version = $Version
    GeneratedAtUtc = [DateTimeOffset]::UtcNow.ToString("O")
    DefaultInstallDirectory = "D:\Software\OMNIX-Entropy\Install"
    DirectorySelectionVisible = $true
    SilentInstallAllowed = $false
    SourcePackageManifestSHA256 = $sourceManifestHash.Hash
    SourcePackageSignerThumbprint = $packageSigner
    Installer = [ordered]@{
        File = $installerName
        Length = $installerFile.Length
        SHA256 = $installerHash.Hash
        SignatureStatus = $signature.Status.ToString()
        SignerSubject = $signature.SignerCertificate.Subject
        SignerThumbprint = $signature.SignerCertificate.Thumbprint
        TimestampPresent = $true
    }
}
$manifestPath = Join-Path $OutputDirectory "installer-manifest.json"
[IO.File]::WriteAllText(
    $manifestPath,
    ($manifest | ConvertTo-Json -Depth 6),
    $utf8WithoutBom)

[pscustomobject]@{
    InstallerDirectory = $OutputDirectory
    InstallerPath = $installerPath
    InstallerManifest = $manifestPath
    DefaultInstallDirectory = $manifest.DefaultInstallDirectory
    DirectorySelectionVisible = $true
    SilentInstallAllowed = $false
    ValidSameSigner = $true
}
