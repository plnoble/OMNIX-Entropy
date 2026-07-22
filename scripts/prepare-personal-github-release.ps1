param(
    [Parameter(Mandatory = $true)]
    [string]$PackageDirectory,
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
        throw "PackageDirectory must be under: $artifactRoot"
    }
    if (-not (Test-Path -LiteralPath $resolved -PathType Container)) {
        throw "PackageDirectory does not exist: $resolved"
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

$PackageDirectory = Resolve-ArtifactDirectory -Path $PackageDirectory
$packageManifestPath = Join-Path $PackageDirectory "package-manifest.json"
if (-not (Test-Path -LiteralPath $packageManifestPath -PathType Leaf)) {
    throw "Signed package manifest is missing."
}

$verifier = Join-Path $PSScriptRoot "verify-signed-release-candidate.ps1"
& powershell -NoProfile -ExecutionPolicy Bypass -File $verifier `
    -PackageDirectory $PackageDirectory
if ($LASTEXITCODE -ne 0) {
    throw "Signed package verification failed."
}

$packageManifest = Get-Content -LiteralPath $packageManifestPath -Raw |
    ConvertFrom-Json
if ($packageManifest.PackageKind -ne "SignedReleaseCandidate" -or
    $packageManifest.ValidSameSigner -ne $true -or
    $packageManifest.App.SignatureStatus -ne "Valid" -or
    $packageManifest.Worker.SignatureStatus -ne "Valid" -or
    -not [string]::Equals(
        [string]$packageManifest.App.SignerThumbprint,
        [string]$packageManifest.Worker.SignerThumbprint,
        [StringComparison]::OrdinalIgnoreCase)) {
    throw "Package is not a valid same-signer release candidate."
}

$sourceZipPath = "$PackageDirectory.zip"
if (-not (Test-Path -LiteralPath $sourceZipPath -PathType Leaf)) {
    throw "Signed package ZIP is missing: $sourceZipPath"
}

$tag = "v$Version"
$releaseDirectory = Join-Path $artifactRoot "GitHub-$tag"
if (Test-Path -LiteralPath $releaseDirectory) {
    throw "Release staging directory already exists: $releaseDirectory"
}
New-Item -ItemType Directory -Path $releaseDirectory | Out-Null

$packageAssetName = "OMNIX-Entropy-$Version-win-x64.zip"
$packageAssetPath = Join-Path $releaseDirectory $packageAssetName
Copy-Item -LiteralPath $sourceZipPath -Destination $packageAssetPath
$packageHash = Get-FileHash -LiteralPath $packageAssetPath -Algorithm SHA256
$packageFile = Get-Item -LiteralPath $packageAssetPath
$packageManifestHash = Get-FileHash `
    -LiteralPath $packageManifestPath `
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
        PackageManifestSHA256 = $packageManifestHash.Hash
        SignerThumbprint = ([string]$packageManifest.App.SignerThumbprint).ToUpperInvariant()
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
    PackageAsset = $packageAssetPath
    ChannelManifest = $channelPath
    Checksums = $sumsPath
    DraftPublished = [bool]$PublishDraft
}
