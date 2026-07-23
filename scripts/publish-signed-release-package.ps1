param(
    [Parameter(Mandatory = $true)]
    [string]$SourcePackageDirectory,
    [string]$OutputDirectory,
    [Parameter(Mandatory = $true)]
    [string]$SignToolPath,
    [Parameter(Mandatory = $true)]
    [string]$CertificateThumbprint,
    [Parameter(Mandatory = $true)]
    [string]$TimestampUrl
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
$artifactRoot = [IO.Path]::GetFullPath((Join-Path $repoRoot ".artifacts"))
$artifactPrefix = $artifactRoot.TrimEnd([IO.Path]::DirectorySeparatorChar) +
    [IO.Path]::DirectorySeparatorChar

function Resolve-ArtifactDirectory {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Label
    )

    $resolved = if ([IO.Path]::IsPathRooted($Path)) {
        [IO.Path]::GetFullPath($Path)
    } else {
        [IO.Path]::GetFullPath((Join-Path $repoRoot $Path))
    }
    if (-not $resolved.StartsWith(
        $artifactPrefix,
        [StringComparison]::OrdinalIgnoreCase)) {
        throw "$Label must be under: $artifactRoot"
    }

    return $resolved
}

function Get-PackageRelativePath {
    param(
        [Parameter(Mandatory = $true)][string]$BaseDirectory,
        [Parameter(Mandatory = $true)][string]$FilePath
    )

    $baseUri = [Uri]($BaseDirectory.TrimEnd(
        [IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar)
    $fileUri = [Uri]$FilePath
    return [Uri]::UnescapeDataString(
        $baseUri.MakeRelativeUri($fileUri).ToString()).Replace('/', '\')
}

function Assert-NoReparsePath {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Label
    )

    $item = Get-Item -LiteralPath $Path
    if (($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
        throw "$Label cannot be a reparse point."
    }
    $directory = if ($item.PSIsContainer) {
        [IO.DirectoryInfo]$item
    } else {
        $item.Directory
    }
    while ($null -ne $directory) {
        if (($directory.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
            throw "$Label cannot use a reparse-point ancestor."
        }
        $directory = $directory.Parent
    }
}

function Test-ByteSequence {
    param(
        [Parameter(Mandatory = $true)][byte[]]$Buffer,
        [Parameter(Mandatory = $true)][byte[]]$Sequence
    )

    if ($Sequence.Length -eq 0 -or $Buffer.Length -lt $Sequence.Length) {
        return $false
    }
    for ($offset = 0; $offset -le $Buffer.Length - $Sequence.Length; $offset++) {
        $matched = $true
        for ($index = 0; $index -lt $Sequence.Length; $index++) {
            if ($Buffer[$offset + $index] -ne $Sequence[$index]) {
                $matched = $false
                break
            }
        }
        if ($matched) {
            return $true
        }
    }

    return $false
}

function Test-CodeSigningEku {
    param(
        [Parameter(Mandatory = $true)]
        [Security.Cryptography.X509Certificates.X509Certificate2]$Certificate
    )

    $codeSigningOid = "1.3.6.1.5.5.7.3.3"
    foreach ($extension in $Certificate.Extensions) {
        if ($null -eq $extension.Oid -or
            $extension.Oid.Value -ne "2.5.29.37") {
            continue
        }

        $enhancedKeyUsage =
            New-Object Security.Cryptography.X509Certificates.X509EnhancedKeyUsageExtension(
                $extension,
                $extension.Critical)
        foreach ($usage in $enhancedKeyUsage.EnhancedKeyUsages) {
            if ($usage.Value -eq $codeSigningOid) {
                return $true
            }
        }
    }

    return $false
}

function Test-RsaPublicKey {
    param(
        [Parameter(Mandatory = $true)]
        [Security.Cryptography.X509Certificates.X509Certificate2]$Certificate
    )

    $rsaPublicKeyOid = "1.2.840.113549.1.1.1"
    return $null -ne $Certificate.PublicKey -and
        $null -ne $Certificate.PublicKey.Oid -and
        $Certificate.PublicKey.Oid.Value -eq $rsaPublicKeyOid
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

function Assert-SourceManifestEntry {
    param(
        [Parameter(Mandatory = $true)]$Entry,
        [Parameter(Mandatory = $true)][string]$SourceRoot
    )

    $relativePath = [string]$Entry.Path
    if ([string]::IsNullOrWhiteSpace($relativePath) -or
        [IO.Path]::IsPathRooted($relativePath) -or
        $relativePath.Split([IO.Path]::DirectorySeparatorChar) -contains "..") {
        throw "Source manifest contains an unsafe path."
    }
    $fullPath = [IO.Path]::GetFullPath((Join-Path $SourceRoot $relativePath))
    if (-not $fullPath.StartsWith(
        $SourceRoot.TrimEnd([IO.Path]::DirectorySeparatorChar) +
            [IO.Path]::DirectorySeparatorChar,
        [StringComparison]::OrdinalIgnoreCase)) {
        throw "Source manifest path escapes the package root."
    }
    if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) {
        throw "Source package file is missing: $relativePath"
    }
    $file = Get-Item -LiteralPath $fullPath
    if ($file.Length -ne [long]$Entry.Length) {
        throw "Source package length verification failed: $relativePath"
    }
    $hash = Get-FileHash -LiteralPath $fullPath -Algorithm SHA256
    if (-not [string]::Equals(
        $hash.Hash,
        [string]$Entry.SHA256,
        [StringComparison]::OrdinalIgnoreCase)) {
        throw "Source package hash verification failed: $relativePath"
    }
}

$SourcePackageDirectory = Resolve-ArtifactDirectory `
    -Path $SourcePackageDirectory `
    -Label "Source package"
if (-not (Test-Path -LiteralPath $SourcePackageDirectory -PathType Container)) {
    throw "Source package directory does not exist: $SourcePackageDirectory"
}
Assert-NoReparsePath `
    -Path $SourcePackageDirectory `
    -Label "Source package"
Assert-NoReparsePath `
    -Path $artifactRoot `
    -Label "Artifact root"

if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $stamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $OutputDirectory = Join-Path $artifactRoot "OMNIX-Entropy-release-$stamp"
}
$OutputDirectory = Resolve-ArtifactDirectory `
    -Path $OutputDirectory `
    -Label "Output"
if (-not [string]::Equals(
    [IO.Path]::GetDirectoryName($OutputDirectory),
    $artifactRoot,
    [StringComparison]::OrdinalIgnoreCase)) {
    throw "Output must be a new direct child directory under: $artifactRoot"
}
if (Test-Path -LiteralPath $OutputDirectory) {
    throw "Output already exists: $OutputDirectory"
}
if (Test-Path -LiteralPath "$OutputDirectory.zip") {
    throw "Output already exists: $OutputDirectory.zip"
}

$sourceManifestPath = Join-Path $SourcePackageDirectory "package-manifest.json"
if (-not (Test-Path -LiteralPath $sourceManifestPath -PathType Leaf)) {
    throw "Source package manifest is missing."
}
$sourceManifest = Get-Content -LiteralPath $sourceManifestPath -Raw |
    ConvertFrom-Json
if ($sourceManifest.PackageKind -ne "PortableTestPackage" -or
    $sourceManifest.ReleaseCommandSurface -ne "ProductionOnly") {
    throw "Source package is not a verified ProductionOnly PortableTestPackage."
}
$requiredFiles = @(
    "Css.App.exe",
    "Css.Elevated.exe",
    "Css.Elevated.dll",
    "rules.scan.json"
)
$manifestPaths = @{}
foreach ($entry in @($sourceManifest.Files)) {
    $manifestPathKey = ([string]$entry.Path).ToUpperInvariant()
    if ($manifestPaths.ContainsKey($manifestPathKey)) {
        throw "Source manifest contains a duplicate package path."
    }
    $manifestPaths[$manifestPathKey] = $true
    Assert-SourceManifestEntry `
        -Entry $entry `
        -SourceRoot $SourcePackageDirectory
}

foreach ($requiredFile in $requiredFiles) {
    if (-not $manifestPaths.ContainsKey($requiredFile.ToUpperInvariant())) {
        throw "Source manifest does not cover required package file: $requiredFile"
    }
    $requiredPath = Join-Path $SourcePackageDirectory $requiredFile
    if (-not (Test-Path -LiteralPath $requiredPath -PathType Leaf)) {
        throw "Required source package file is missing: $requiredFile"
    }
}

$workerAssemblyPath = Join-Path $SourcePackageDirectory "Css.Elevated.dll"
$debugCommandToken = "official-uninstall-fake-worker"
$workerAssemblyBytes = [IO.File]::ReadAllBytes($workerAssemblyPath)
if ((Test-ByteSequence `
        -Buffer $workerAssemblyBytes `
        -Sequence ([Text.Encoding]::UTF8.GetBytes($debugCommandToken))) -or
    (Test-ByteSequence `
        -Buffer $workerAssemblyBytes `
        -Sequence ([Text.Encoding]::Unicode.GetBytes($debugCommandToken)))) {
    throw "Release worker includes debug-only command surface: $debugCommandToken"
}

$resolvedSignTool = [IO.Path]::GetFullPath($SignToolPath)
if (-not (Test-Path -LiteralPath $resolvedSignTool -PathType Leaf) -or
    -not [string]::Equals(
        [IO.Path]::GetFileName($resolvedSignTool),
        "signtool.exe",
        [StringComparison]::OrdinalIgnoreCase)) {
    throw "SignToolPath must point to an existing signtool.exe."
}

$normalizedThumbprint = ($CertificateThumbprint -replace "\s", "").ToUpperInvariant()
if ($normalizedThumbprint -notmatch "^[0-9A-F]{40}$") {
    throw "Certificate thumbprint must contain exactly 40 hexadecimal characters."
}
$certificateStoreRoot = "Cert:\CurrentUser\My"
$certificatePath = Join-Path $certificateStoreRoot $normalizedThumbprint
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
if (-not (Test-CodeSigningEku -Certificate $certificate)) {
    throw "Code-signing certificate does not contain the code-signing EKU."
}
if (-not (Test-RsaPublicKey -Certificate $certificate)) {
    throw "Code-signing certificate must use an RSA public key."
}

$timestampUri = $null
if (-not [Uri]::TryCreate(
        $TimestampUrl,
        [UriKind]::Absolute,
        [ref]$timestampUri) -or
    -not (Test-ApprovedTimestampUri -Uri $timestampUri)) {
    throw "TimestampUrl must be an absolute HTTPS endpoint or the approved official HTTP RFC3161 endpoint."
}

New-Item -ItemType Directory -Path $OutputDirectory | Out-Null
foreach ($sourceItem in Get-ChildItem -LiteralPath $SourcePackageDirectory -Recurse) {
    if (($sourceItem.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
        throw "Source package content cannot contain a reparse point."
    }
    $relativePath = Get-PackageRelativePath `
        -BaseDirectory $SourcePackageDirectory `
        -FilePath $sourceItem.FullName
    if ($relativePath -in @("package-manifest.json", "README-TEST.txt")) {
        continue
    }
    $destination = Join-Path $OutputDirectory $relativePath
    if ($sourceItem.PSIsContainer) {
        if (-not (Test-Path -LiteralPath $destination)) {
            New-Item -ItemType Directory -Path $destination | Out-Null
        }
        continue
    }
    $destinationParent = [IO.Path]::GetDirectoryName($destination)
    if (-not (Test-Path -LiteralPath $destinationParent)) {
        New-Item -ItemType Directory -Path $destinationParent | Out-Null
    }
    Copy-Item -LiteralPath $sourceItem.FullName -Destination $destination
}

$appPath = Join-Path $OutputDirectory "Css.App.exe"
$workerPath = Join-Path $OutputDirectory "Css.Elevated.exe"
& $resolvedSignTool sign /s My /sha1 $normalizedThumbprint /fd SHA256 `
    /tr $TimestampUrl /td SHA256 $appPath
if ($LASTEXITCODE -ne 0) {
    throw "Signing Css.App.exe failed with exit code $LASTEXITCODE."
}
& $resolvedSignTool sign /s My /sha1 $normalizedThumbprint /fd SHA256 `
    /tr $TimestampUrl /td SHA256 $workerPath
if ($LASTEXITCODE -ne 0) {
    throw "Signing Css.Elevated.exe failed with exit code $LASTEXITCODE."
}

$appSignature = Get-AuthenticodeSignature -LiteralPath $appPath
$workerSignature = Get-AuthenticodeSignature -LiteralPath $workerPath
foreach ($signature in @($appSignature, $workerSignature)) {
    if ($signature.Status.ToString() -ne "Valid" -or
        $null -eq $signature.SignerCertificate) {
        throw "Signature verification failed after signing."
    }
    if (-not [string]::Equals(
        $signature.SignerCertificate.Thumbprint,
        $normalizedThumbprint,
        [StringComparison]::OrdinalIgnoreCase)) {
        throw "Signer thumbprint does not match the requested certificate."
    }
    if ($null -eq $signature.TimeStamperCertificate) {
        throw "Timestamp verification failed after signing."
    }
}

$readmeTemplatePath = Join-Path $repoRoot "scripts\README-SIGNED-RELEASE.zh-CN.txt"
if (-not (Test-Path -LiteralPath $readmeTemplatePath -PathType Leaf)) {
    throw "Signed release readme template is missing."
}
$readmePath = Join-Path $OutputDirectory "README-RELEASE-CANDIDATE.txt"
Copy-Item -LiteralPath $readmeTemplatePath -Destination $readmePath

$packageFiles = @(
    Get-ChildItem -LiteralPath $OutputDirectory -File -Recurse |
        Sort-Object FullName |
        ForEach-Object {
            $file = $_
            $hash = Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256
            [ordered]@{
                Path = Get-PackageRelativePath `
                    -BaseDirectory $OutputDirectory `
                    -FilePath $file.FullName
                Length = $file.Length
                SHA256 = $hash.Hash
            }
        }
)

$sourceManifestHash = Get-FileHash `
    -LiteralPath $sourceManifestPath `
    -Algorithm SHA256
$manifest = [ordered]@{
    Product = "OMNIX-Entropy"
    PackageKind = "SignedReleaseCandidate"
    GeneratedAtUtc = [DateTimeOffset]::UtcNow.ToString("O")
    Configuration = "Release"
    RuntimeMode = "FrameworkDependent"
    RequiredRuntime = ".NET 8 Desktop Runtime"
    ReleaseCommandSurface = "ProductionOnly"
    SourcePackageManifestSHA256 = $sourceManifestHash.Hash
    ValidSameSigner = $true
    MutationReadiness = "EligibleForDisposableMachineAcceptance"
    DisposableMachineAcceptance = $false
    AcceptanceStatus = "AwaitingDisposableMachineAcceptance"
    TimestampUrl = $TimestampUrl
    CertificatePublicKeyAlgorithm = "RSA"
    App = [ordered]@{
        File = "Css.App.exe"
        SignatureStatus = $appSignature.Status.ToString()
        SignerSubject = $appSignature.SignerCertificate.Subject
        SignerThumbprint = $appSignature.SignerCertificate.Thumbprint
    }
    Worker = [ordered]@{
        File = "Css.Elevated.exe"
        SignatureStatus = $workerSignature.Status.ToString()
        SignerSubject = $workerSignature.SignerCertificate.Subject
        SignerThumbprint = $workerSignature.SignerCertificate.Thumbprint
    }
    Files = $packageFiles
}

$utf8WithoutBom = [Text.UTF8Encoding]::new($false)
$manifestPath = Join-Path $OutputDirectory "package-manifest.json"
$manifestJson = $manifest | ConvertTo-Json -Depth 8
[IO.File]::WriteAllText($manifestPath, $manifestJson, $utf8WithoutBom)

$zipPath = "$OutputDirectory.zip"
Compress-Archive `
    -Path (Join-Path $OutputDirectory "*") `
    -DestinationPath $zipPath

[pscustomobject]@{
    PackageDirectory = $OutputDirectory
    ZipPath = $zipPath
    AppSignatureStatus = $appSignature.Status.ToString()
    WorkerSignatureStatus = $workerSignature.Status.ToString()
    ValidSameSigner = $true
    MutationReadiness = "EligibleForDisposableMachineAcceptance"
    DisposableMachineAcceptance = $false
}
