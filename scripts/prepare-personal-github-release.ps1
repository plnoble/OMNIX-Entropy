param(
    [Parameter(Mandatory = $true)]
    [string]$InstallerDirectory,
    [Parameter(Mandatory = $true)]
    [ValidatePattern("^[0-9]+\.[0-9]+\.[0-9]+$")]
    [string]$Version,
    [ValidateSet("plnoble/OMNIX-Entropy")]
    [string]$Repository = "plnoble/OMNIX-Entropy",
    [string]$ReleaseNotesPath,
    [switch]$PublishDraft
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
$artifactRoot = [IO.Path]::GetFullPath((Join-Path $repoRoot ".artifacts"))
$artifactPrefix = $artifactRoot.TrimEnd([IO.Path]::DirectorySeparatorChar) +
    [IO.Path]::DirectorySeparatorChar
$utf8WithoutBom = [Text.UTF8Encoding]::new($false)

function Resolve-ArtifactDirectory {
    param([Parameter(Mandatory = $true)][string]$Path)

    $resolved = if ([IO.Path]::IsPathRooted($Path)) {
        [IO.Path]::GetFullPath($Path)
    } else {
        [IO.Path]::GetFullPath((Join-Path $repoRoot $Path))
    }
    if (-not $resolved.StartsWith(
        $artifactPrefix,
        [StringComparison]::OrdinalIgnoreCase)) {
        throw "InstallerDirectory must be under: $artifactRoot"
    }
    if (-not (Test-Path -LiteralPath $resolved -PathType Container)) {
        throw "InstallerDirectory does not exist: $resolved"
    }
    return $resolved
}

function Assert-SafeReleaseNotesPath {
    param([Parameter(Mandatory = $true)][string]$Path)

    $resolved = [IO.Path]::GetFullPath($Path)
    if (-not (Test-Path -LiteralPath $resolved -PathType Leaf)) {
        throw "Release notes file does not exist: $resolved"
    }
    if ([IO.Path]::GetExtension($resolved) -notin @(".md", ".txt")) {
        throw "Release notes must be a Markdown or text file."
    }
    return $resolved
}

$InstallerDirectory = Resolve-ArtifactDirectory -Path $InstallerDirectory
$installerManifestPath = Join-Path $InstallerDirectory "installer-manifest.json"
if (-not (Test-Path -LiteralPath $installerManifestPath -PathType Leaf)) {
    throw "Installer manifest is missing."
}

$verifier = Join-Path $PSScriptRoot "verify-personal-installer.ps1"
& powershell -NoProfile -ExecutionPolicy Bypass -File $verifier `
    -InstallerDirectory $InstallerDirectory
if ($LASTEXITCODE -ne 0) {
    throw "Personal installer verification failed."
}

$installerManifest = Get-Content -LiteralPath $installerManifestPath -Raw |
    ConvertFrom-Json
if ($installerManifest.PackageKind -ne "PersonalWindowsInstaller" -or
    [string]$installerManifest.Version -ne $Version -or
    $installerManifest.DirectorySelectionVisible -ne $true -or
    $installerManifest.SilentInstallAllowed -ne $false -or
    $installerManifest.Installer.SignatureStatus -ne "Valid" -or
    -not [string]::Equals(
        [string]$installerManifest.Installer.SignerThumbprint,
        [string]$installerManifest.SourcePackageSignerThumbprint,
        [StringComparison]::OrdinalIgnoreCase)) {
    throw "Installer is not a valid same-signer D-first release candidate."
}

$sourceInstallerName = [string]$installerManifest.Installer.File
$sourceInstallerPath = Join-Path $InstallerDirectory $sourceInstallerName
if (-not (Test-Path -LiteralPath $sourceInstallerPath -PathType Leaf)) {
    throw "Verified installer is missing: $sourceInstallerPath"
}

$tag = "v$Version"
$releaseDirectory = Join-Path $artifactRoot "GitHub-$tag"
if (Test-Path -LiteralPath $releaseDirectory) {
    throw "Release staging directory already exists: $releaseDirectory"
}
New-Item -ItemType Directory -Path $releaseDirectory | Out-Null

$packageAssetName = "OMNIX-Entropy-$Version-win-x64-setup.exe"
$packageAssetPath = Join-Path $releaseDirectory $packageAssetName
Copy-Item -LiteralPath $sourceInstallerPath -Destination $packageAssetPath
$releaseInstallerManifestPath = Join-Path $releaseDirectory "installer-manifest.json"
Copy-Item -LiteralPath $installerManifestPath -Destination $releaseInstallerManifestPath
$packageHash = Get-FileHash -LiteralPath $packageAssetPath -Algorithm SHA256
$packageFile = Get-Item -LiteralPath $packageAssetPath
if ($packageFile.Length -ne [long]$installerManifest.Installer.Length -or
    -not [string]::Equals(
        $packageHash.Hash,
        [string]$installerManifest.Installer.SHA256,
        [StringComparison]::OrdinalIgnoreCase)) {
    throw "Installer copy hash verification failed."
}
$installerManifestHash = Get-FileHash `
    -LiteralPath $releaseInstallerManifestPath `
    -Algorithm SHA256
$commitSha = (& git -C $repoRoot rev-parse HEAD 2>$null)
if ($LASTEXITCODE -ne 0 -or $commitSha -notmatch "^[0-9a-fA-F]{40}$") {
    throw "A committed source revision is required before release staging."
}

$channel = [ordered]@{
    SchemaVersion = 1
    Product = "OMNIX-Entropy"
    Repository = $Repository
    Version = $Version
    Tag = $tag
    CommitSHA = $commitSha.ToLowerInvariant()
    GeneratedAtUtc = [DateTimeOffset]::UtcNow.ToString("O")
    Package = [ordered]@{
        AssetName = $packageAssetName
        DownloadUrl = "https://github.com/$Repository/releases/download/$tag/$packageAssetName"
        Length = $packageFile.Length
        SHA256 = $packageHash.Hash
        InstallerManifestSHA256 = $installerManifestHash.Hash
        SignerThumbprint = ([string]$installerManifest.Installer.SignerThumbprint).ToUpperInvariant()
        ValidSameSigner = $true
    }
}
$channelPath = Join-Path $releaseDirectory "omnix-release.json"
[IO.File]::WriteAllText(
    $channelPath,
    ($channel | ConvertTo-Json -Depth 6),
    $utf8WithoutBom)
$sumsPath = Join-Path $releaseDirectory "SHA256SUMS.txt"
$sums = @(
    "$($packageHash.Hash.ToLowerInvariant())  $packageAssetName",
    "$($installerManifestHash.Hash.ToLowerInvariant())  installer-manifest.json",
    "$((Get-FileHash -LiteralPath $channelPath -Algorithm SHA256).Hash.ToLowerInvariant())  omnix-release.json"
) -join [Environment]::NewLine
[IO.File]::WriteAllText($sumsPath, $sums + [Environment]::NewLine, $utf8WithoutBom)

if ($PublishDraft) {
    $remote = (& git -C $repoRoot remote get-url origin 2>$null)
    if ($LASTEXITCODE -ne 0 -or
        $remote -notmatch "^(https://github\.com/|git@github\.com:)plnoble/OMNIX-Entropy(?:\.git)?$") {
        throw "The origin remote must be the fixed OMNIX-Entropy repository."
    }
    & gh auth status | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "GitHub CLI authentication is required to create a draft release."
    }
    & gh release view $tag --repo $Repository *> $null
    if ($LASTEXITCODE -eq 0) {
        throw "Release tag already exists on GitHub: $tag"
    }

    $notes = if ([string]::IsNullOrWhiteSpace($ReleaseNotesPath)) {
        Join-Path $releaseDirectory "release-notes.md"
    } else {
        Assert-SafeReleaseNotesPath -Path $ReleaseNotesPath
    }
    if ([string]::IsNullOrWhiteSpace($ReleaseNotesPath)) {
        [IO.File]::WriteAllText(
            $notes,
            "# OMNIX-Entropy $Version`r`n`r`nPersonal Windows release. Review the attached hash and manifest before publishing.`r`n",
            $utf8WithoutBom)
    }

    $arguments = @(
        "release", "create", $tag,
        "--repo", $Repository,
        "--target", $commitSha,
        "--title", "OMNIX-Entropy $Version",
        "--notes-file", $notes,
        "--draft",
        $packageAssetPath,
        $releaseInstallerManifestPath,
        $channelPath,
        $sumsPath
    )
    & gh @arguments
    if ($LASTEXITCODE -ne 0) {
        throw "GitHub draft release creation failed."
    }
}

[pscustomobject]@{
    Repository = $Repository
    Version = $Version
    Tag = $tag
    CommitSHA = $commitSha
    StagingDirectory = $releaseDirectory
    InstallerAsset = $packageAssetPath
    InstallerManifest = $releaseInstallerManifestPath
    ChannelManifest = $channelPath
    Checksums = $sumsPath
    DraftPublished = [bool]$PublishDraft
}
