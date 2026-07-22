param(
    [Parameter(Mandatory = $true)]
    [string]$PackageDirectory
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

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
        throw "$Label cannot be a ReparsePoint."
    }
    $directory = if ($item.PSIsContainer) {
        [IO.DirectoryInfo]$item
    } else {
        $item.Directory
    }
    while ($null -ne $directory) {
        if (($directory.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
            throw "$Label cannot use a ReparsePoint ancestor."
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

if (-not [IO.Path]::IsPathRooted($PackageDirectory)) {
    throw "PackageDirectory must be a fully qualified local path."
}
$PackageDirectory = [IO.Path]::GetFullPath($PackageDirectory)
if (-not (Test-Path -LiteralPath $PackageDirectory -PathType Container)) {
    throw "PackageDirectory does not exist."
}
$driveRoot = [IO.Path]::GetPathRoot($PackageDirectory)
$drive = [IO.DriveInfo]::new($driveRoot)
if ($drive.DriveType -ne [IO.DriveType]::Fixed) {
    throw "PackageDirectory must be on a Fixed local drive."
}
Assert-NoReparsePath -Path $PackageDirectory -Label "PackageDirectory"

$packageItems = @(Get-ChildItem -LiteralPath $PackageDirectory -Recurse -Force)
foreach ($packageItem in $packageItems) {
    if (($packageItem.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
        throw "Package content cannot contain a ReparsePoint."
    }
}

$manifestPath = Join-Path $PackageDirectory "package-manifest.json"
if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
    throw "Signed release candidate manifest is missing."
}
$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
if ($manifest.PackageKind -ne "SignedReleaseCandidate" -or
    $manifest.ReleaseCommandSurface -ne "ProductionOnly" -or
    $manifest.ValidSameSigner -ne $true -or
    $manifest.MutationReadiness -ne "EligibleForDisposableMachineAcceptance" -or
    $manifest.DisposableMachineAcceptance -ne $false -or
    $manifest.AcceptanceStatus -ne "AwaitingDisposableMachineAcceptance") {
    throw "Manifest is not an awaiting signed release candidate."
}

$packagePrefix = $PackageDirectory.TrimEnd(
    [IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
$manifestPaths = @{}
foreach ($entry in @($manifest.Files)) {
    $relativePath = [string]$entry.Path
    $segments = $relativePath -split '[\\/]'
    if ([string]::IsNullOrWhiteSpace($relativePath) -or
        [IO.Path]::IsPathRooted($relativePath) -or
        $segments -contains "..") {
        throw "Manifest contains an unsafe package path."
    }
    $pathKey = $relativePath.Replace('/', '\').ToUpperInvariant()
    if ($manifestPaths.ContainsKey($pathKey)) {
        throw "Manifest contains a duplicate package path."
    }
    $manifestPaths[$pathKey] = $true

    $fullPath = [IO.Path]::GetFullPath((Join-Path $PackageDirectory $relativePath))
    if (-not $fullPath.StartsWith(
        $packagePrefix,
        [StringComparison]::OrdinalIgnoreCase)) {
        throw "Manifest package path escapes PackageDirectory."
    }
    if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) {
        throw "Manifest package file is missing: $relativePath"
    }
    $file = Get-Item -LiteralPath $fullPath
    if ($file.Length -ne [long]$entry.Length) {
        throw "Package file length verification failed: $relativePath"
    }
    $hash = Get-FileHash -LiteralPath $fullPath -Algorithm SHA256
    if (-not [string]::Equals(
        $hash.Hash,
        [string]$entry.SHA256,
        [StringComparison]::OrdinalIgnoreCase)) {
        throw "Package file hash verification failed: $relativePath"
    }
}

$requiredFiles = @(
    "Css.App.exe",
    "Css.Elevated.exe",
    "Css.Elevated.dll",
    "rules.scan.json"
)
foreach ($requiredFile in $requiredFiles) {
    if (-not $manifestPaths.ContainsKey($requiredFile.ToUpperInvariant())) {
        throw "Manifest does not cover required package file: $requiredFile"
    }
}

foreach ($actualFile in @($packageItems | Where-Object { -not $_.PSIsContainer })) {
    $relativePath = Get-PackageRelativePath `
        -BaseDirectory $PackageDirectory `
        -FilePath $actualFile.FullName
    if ([string]::Equals(
        $relativePath,
        "package-manifest.json",
        [StringComparison]::OrdinalIgnoreCase)) {
        continue
    }
    if (-not $manifestPaths.ContainsKey($relativePath.ToUpperInvariant())) {
        throw "Unlisted package payload file: $relativePath"
    }
}

$appPath = Join-Path $PackageDirectory "Css.App.exe"
$workerPath = Join-Path $PackageDirectory "Css.Elevated.exe"
$appSignature = Get-AuthenticodeSignature -LiteralPath $appPath
$workerSignature = Get-AuthenticodeSignature -LiteralPath $workerPath
$rsaPublicKeyOid = "1.2.840.113549.1.1.1"
foreach ($signature in @($appSignature, $workerSignature)) {
    if ($signature.Status.ToString() -ne "Valid" -or
        $null -eq $signature.SignerCertificate) {
        throw "Package signature is not valid."
    }
    if ($null -eq $signature.TimeStamperCertificate) {
        throw "Package signature does not contain a verified timestamp."
    }
    if ($null -eq $signature.SignerCertificate.PublicKey -or
        $null -eq $signature.SignerCertificate.PublicKey.Oid -or
        $signature.SignerCertificate.PublicKey.Oid.Value -ne $rsaPublicKeyOid) {
        throw "Signer certificate must use an RSA public key."
    }
}
if ([string]$manifest.CertificatePublicKeyAlgorithm -ne "RSA") {
    throw "Candidate manifest does not declare the required RSA signer algorithm."
}

$appThumbprint = $appSignature.SignerCertificate.Thumbprint
$workerThumbprint = $workerSignature.SignerCertificate.Thumbprint
if (-not [string]::Equals(
    $appThumbprint,
    $workerThumbprint,
    [StringComparison]::OrdinalIgnoreCase)) {
    throw "Signer thumbprints do not match."
}
if (-not [string]::Equals(
        $appThumbprint,
        [string]$manifest.App.SignerThumbprint,
        [StringComparison]::OrdinalIgnoreCase) -or
    -not [string]::Equals(
        $workerThumbprint,
        [string]$manifest.Worker.SignerThumbprint,
        [StringComparison]::OrdinalIgnoreCase)) {
    throw "Signer thumbprint does not match the manifest."
}

$workerAssemblyPath = Join-Path $PackageDirectory "Css.Elevated.dll"
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

[pscustomobject]@{
    PackageDirectory = $PackageDirectory
    CanBeginDisposableAcceptance = $true
    DisposableMachineAcceptance = $false
    AcceptanceStatus = "AwaitingBehavioralAcceptance"
    SignerSubject = $appSignature.SignerCertificate.Subject
    SignerThumbprint = $appThumbprint
    VerifiedPayloadFiles = $manifestPaths.Count
    NextStep = "Run the explicit disposable-machine behavioral acceptance checklist."
}
