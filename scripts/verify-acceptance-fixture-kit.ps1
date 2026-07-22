param(
    [Parameter(Mandatory = $true)]
    [string]$FixtureKitDirectory
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-RelativePath {
    param(
        [Parameter(Mandatory = $true)][string]$BaseDirectory,
        [Parameter(Mandatory = $true)][string]$FilePath
    )
    $baseUri = [Uri]($BaseDirectory.TrimEnd(
        [IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar)
    return [Uri]::UnescapeDataString(
        $baseUri.MakeRelativeUri([Uri]$FilePath).ToString()).Replace('/', '\')
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

if (-not [IO.Path]::IsPathRooted($FixtureKitDirectory)) {
    throw "FixtureKitDirectory must be a fully qualified local path."
}
$FixtureKitDirectory = [IO.Path]::GetFullPath($FixtureKitDirectory)
if (-not (Test-Path -LiteralPath $FixtureKitDirectory -PathType Container)) {
    throw "FixtureKitDirectory does not exist."
}
$drive = [IO.DriveInfo]::new([IO.Path]::GetPathRoot($FixtureKitDirectory))
if ($drive.DriveType -ne [IO.DriveType]::Fixed) {
    throw "FixtureKitDirectory must be on a Fixed local drive."
}
$items = @(Get-ChildItem -LiteralPath $FixtureKitDirectory -Recurse -Force)
$current = Get-Item -LiteralPath $FixtureKitDirectory
while ($null -ne $current) {
    if (($current.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
        throw "FixtureKitDirectory cannot use a ReparsePoint."
    }
    $current = $current.Parent
}
foreach ($item in $items) {
    if (($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
        throw "Fixture kit content cannot contain a ReparsePoint."
    }
}

$manifestPath = Join-Path $FixtureKitDirectory "package-manifest.json"
if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
    throw "Fixture package manifest is missing."
}
$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
if ($manifest.PackageKind -ne "DisposableAcceptanceFixtureKit" -or
    $manifest.Configuration -ne "Release" -or
    $manifest.SourceProject -ne "Css.AcceptanceFixtures" -or
    $manifest.NotForEndUsers -ne $true -or
    $manifest.MutationCommandsRequireAttestation -ne $true -or
    $manifest.PrimaryMachineAllowed -ne $false) {
    throw "Fixture package manifest safety state is invalid."
}

$prefix = $FixtureKitDirectory.TrimEnd(
    [IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
$listed = @{}
foreach ($entry in @($manifest.Files)) {
    $relativePath = [string]$entry.Path
    $segments = $relativePath -split '[\\/]'
    if ([string]::IsNullOrWhiteSpace($relativePath) -or
        [IO.Path]::IsPathRooted($relativePath) -or
        $segments -contains "..") {
        throw "Fixture manifest contains an unsafe path."
    }
    $key = $relativePath.Replace('/', '\').ToUpperInvariant()
    if ($listed.ContainsKey($key)) {
        throw "Fixture manifest contains a duplicate path."
    }
    $listed[$key] = $true
    $fullPath = [IO.Path]::GetFullPath((
        Join-Path $FixtureKitDirectory $relativePath))
    if (-not $fullPath.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase) -or
        -not (Test-Path -LiteralPath $fullPath -PathType Leaf)) {
        throw "Fixture manifest path is missing or escapes its package."
    }
    $file = Get-Item -LiteralPath $fullPath
    if ($file.Length -ne [long]$entry.Length) {
        throw "Fixture file length verification failed: $relativePath"
    }
    $hash = Get-FileHash -LiteralPath $fullPath -Algorithm SHA256
    if (-not [string]::Equals(
            $hash.Hash,
            [string]$entry.SHA256,
            [StringComparison]::OrdinalIgnoreCase)) {
        throw "Fixture file hash verification failed: $relativePath"
    }
}

$requiredFiles = @(
    "Css.AcceptanceFixtures.exe",
    "Css.AcceptanceFixtures.dll",
    "Css.AcceptanceFixtures.deps.json",
    "Css.AcceptanceFixtures.runtimeconfig.json",
    "README-ACCEPTANCE-FIXTURES.zh-CN.md"
)
foreach ($requiredFile in $requiredFiles) {
    if (-not $listed.ContainsKey($requiredFile.ToUpperInvariant())) {
        throw "Fixture manifest does not cover a required file: $requiredFile"
    }
}
foreach ($actualFile in @($items | Where-Object { -not $_.PSIsContainer })) {
    $relativePath = Get-RelativePath `
        -BaseDirectory $FixtureKitDirectory `
        -FilePath $actualFile.FullName
    if ($relativePath -eq "package-manifest.json") {
        continue
    }
    if (-not $listed.ContainsKey($relativePath.ToUpperInvariant())) {
        throw "Unlisted fixture payload file: $relativePath"
    }
}
foreach ($forbiddenProductFile in @("Css.App.exe", "Css.Elevated.exe")) {
    if (Test-Path `
        -LiteralPath (Join-Path $FixtureKitDirectory $forbiddenProductFile)) {
        throw "Fixture kit contains a product executable."
    }
}

$assemblyPath = Join-Path $FixtureKitDirectory "Css.AcceptanceFixtures.dll"
$assemblyBytes = [IO.File]::ReadAllBytes($assemblyPath)
$attestation = "I CONFIRM THIS IS A DISPOSABLE WINDOWS TEST ENVIRONMENT"
if (-not (Test-ByteSequence `
        -Buffer $assemblyBytes `
        -Sequence ([Text.Encoding]::UTF8.GetBytes($attestation))) -and
    -not (Test-ByteSequence `
        -Buffer $assemblyBytes `
        -Sequence ([Text.Encoding]::Unicode.GetBytes($attestation)))) {
    throw "Fixture assembly does not contain the required attestation authority."
}

[pscustomobject]@{
    FixtureKitDirectory = $FixtureKitDirectory
    FixtureKitVerified = $true
    MutationCommandsRequireAttestation = $true
    PrimaryMachineAllowed = $false
    FixtureManifestSHA256 = (
        Get-FileHash -LiteralPath $manifestPath -Algorithm SHA256).Hash
    VerifiedPayloadFiles = $listed.Count
}
