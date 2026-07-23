param(
    [Parameter(Mandatory = $true)]
    [string]$InstallerDirectory
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Assert-NoReparsePath {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Label
    )

    $item = Get-Item -LiteralPath $Path
    if (($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
        throw "$Label cannot be a ReparsePoint."
    }
    $directory = if ($item.PSIsContainer) { [IO.DirectoryInfo]$item } else { $item.Directory }
    while ($null -ne $directory) {
        if (($directory.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
            throw "$Label cannot use a ReparsePoint ancestor."
        }
        $directory = $directory.Parent
    }
}

if (-not [IO.Path]::IsPathRooted($InstallerDirectory)) {
    throw "InstallerDirectory must be a fully qualified local path."
}
$InstallerDirectory = [IO.Path]::GetFullPath($InstallerDirectory)
if (-not (Test-Path -LiteralPath $InstallerDirectory -PathType Container)) {
    throw "InstallerDirectory does not exist."
}
$drive = [IO.DriveInfo]::new([IO.Path]::GetPathRoot($InstallerDirectory))
if ($drive.DriveType -ne [IO.DriveType]::Fixed) {
    throw "InstallerDirectory must be on a Fixed local drive."
}
Assert-NoReparsePath -Path $InstallerDirectory -Label "InstallerDirectory"

$items = @(Get-ChildItem -LiteralPath $InstallerDirectory -Force)
foreach ($item in $items) {
    if ($item.PSIsContainer -or
        ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
        throw "InstallerDirectory may contain only the installer and its manifest."
    }
}

$manifestPath = Join-Path $InstallerDirectory "installer-manifest.json"
if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
    throw "Installer manifest is missing."
}
$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
if ($manifest.SchemaVersion -ne 1 -or
    $manifest.Product -ne "OMNIX-Entropy" -or
    $manifest.PackageKind -ne "PersonalWindowsInstaller" -or
    [string]$manifest.Version -notmatch "^[0-9]+\.[0-9]+\.[0-9]+$" -or
    $manifest.DefaultInstallDirectory -ne "D:\Software\OMNIX-Entropy\Install" -or
    $manifest.DirectorySelectionVisible -ne $true -or
    $manifest.SilentInstallAllowed -ne $false -or
    [string]$manifest.SourcePackageManifestSHA256 -notmatch "^[0-9A-Fa-f]{64}$" -or
    [string]$manifest.SourcePackageSignerThumbprint -notmatch "^[0-9A-Fa-f]{40}$") {
    throw "Installer manifest identity or safety policy is invalid."
}

$expectedName = "OMNIX-Entropy-$($manifest.Version)-win-x64-setup.exe"
if ($manifest.Installer.File -ne $expectedName -or
    [string]$manifest.Installer.SHA256 -notmatch "^[0-9A-Fa-f]{64}$" -or
    [long]$manifest.Installer.Length -le 0 -or
    $manifest.Installer.SignatureStatus -ne "Valid" -or
    $manifest.Installer.TimestampPresent -ne $true) {
    throw "Installer manifest payload evidence is invalid."
}

$installerPath = Join-Path $InstallerDirectory $expectedName
if (-not (Test-Path -LiteralPath $installerPath -PathType Leaf)) {
    throw "Installer file is missing."
}
if ($items.Count -ne 2) {
    throw "InstallerDirectory contains an unlisted file."
}
$installerFile = Get-Item -LiteralPath $installerPath
if ($installerFile.Length -ne [long]$manifest.Installer.Length) {
    throw "Installer file length verification failed."
}
$installerHash = Get-FileHash -LiteralPath $installerPath -Algorithm SHA256
if (-not [string]::Equals(
    $installerHash.Hash,
    [string]$manifest.Installer.SHA256,
    [StringComparison]::OrdinalIgnoreCase)) {
    throw "Installer file hash verification failed."
}

$signature = Get-AuthenticodeSignature -LiteralPath $installerPath
if ($signature.Status.ToString() -ne "Valid" -or
    $null -eq $signature.SignerCertificate -or
    $null -eq $signature.TimeStamperCertificate) {
    throw "Installer Authenticode signature or timestamp is invalid."
}
$signerThumbprint = $signature.SignerCertificate.Thumbprint
if (-not [string]::Equals(
    $signerThumbprint,
    [string]$manifest.Installer.SignerThumbprint,
    [StringComparison]::OrdinalIgnoreCase) -or
    -not [string]::Equals(
        $signerThumbprint,
        [string]$manifest.SourcePackageSignerThumbprint,
        [StringComparison]::OrdinalIgnoreCase)) {
    throw "Installer signer does not match the manifest or source package signer."
}

[pscustomobject]@{
    InstallerDirectory = $InstallerDirectory
    InstallerPath = $installerPath
    Version = [string]$manifest.Version
    SignerThumbprint = $signerThumbprint
    DefaultInstallDirectory = [string]$manifest.DefaultInstallDirectory
    DirectorySelectionVisible = $true
    SilentInstallAllowed = $false
    CanStageGitHubRelease = $true
}
