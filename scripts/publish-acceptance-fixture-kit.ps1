param(
    [ValidateSet("Release")]
    [string]$Configuration = "Release",
    [string]$OutputDirectory
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
$artifactRoot = [IO.Path]::GetFullPath((Join-Path $repoRoot ".artifacts"))
if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $stamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $OutputDirectory = Join-Path `
        $artifactRoot `
        "OMNIX-Acceptance-Fixtures-$stamp"
} elseif ([IO.Path]::IsPathRooted($OutputDirectory)) {
    $OutputDirectory = [IO.Path]::GetFullPath($OutputDirectory)
} else {
    $OutputDirectory = [IO.Path]::GetFullPath((Join-Path $repoRoot $OutputDirectory))
}

$artifactPrefix = $artifactRoot.TrimEnd(
    [IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
if (-not $OutputDirectory.StartsWith(
        $artifactPrefix,
        [StringComparison]::OrdinalIgnoreCase)) {
    throw "OutputDirectory must be a new child under .artifacts."
}
if (Test-Path -LiteralPath $OutputDirectory) {
    throw "OutputDirectory already exists."
}
if (-not (Test-Path -LiteralPath $artifactRoot -PathType Container)) {
    New-Item -ItemType Directory -Path $artifactRoot | Out-Null
}
New-Item -ItemType Directory -Path $OutputDirectory | Out-Null

$project = Join-Path `
    $repoRoot `
    "src\Css.AcceptanceFixtures\Css.AcceptanceFixtures.csproj"
& dotnet publish $project `
    --configuration $Configuration `
    --no-restore `
    --no-self-contained `
    --output $OutputDirectory
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed for the acceptance fixture kit."
}

$requiredFiles = @(
    "Css.AcceptanceFixtures.exe",
    "Css.AcceptanceFixtures.dll",
    "Css.AcceptanceFixtures.deps.json",
    "Css.AcceptanceFixtures.runtimeconfig.json"
)
foreach ($requiredFile in $requiredFiles) {
    if (-not (Test-Path `
            -LiteralPath (Join-Path $OutputDirectory $requiredFile) `
            -PathType Leaf)) {
        throw "Required fixture file is missing: $requiredFile"
    }
}
foreach ($forbiddenProductFile in @("Css.App.exe", "Css.Elevated.exe")) {
    if (Test-Path `
        -LiteralPath (Join-Path $OutputDirectory $forbiddenProductFile)) {
        throw "Fixture output contains a product executable."
    }
}

$guideSource = Join-Path `
    $repoRoot `
    "docs\release\disposable-windows-acceptance.zh-CN.md"
$guideTarget = Join-Path `
    $OutputDirectory `
    "README-ACCEPTANCE-FIXTURES.zh-CN.md"
Copy-Item -LiteralPath $guideSource -Destination $guideTarget

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

$files = @(
    Get-ChildItem -LiteralPath $OutputDirectory -File -Recurse |
        Sort-Object FullName |
        ForEach-Object {
            $file = $_
            $hash = Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256
            [ordered]@{
                Path = Get-RelativePath `
                    -BaseDirectory $OutputDirectory `
                    -FilePath $file.FullName
                Length = $file.Length
                SHA256 = $hash.Hash
            }
        }
)
$manifest = [ordered]@{
    Product = "OMNIX-Entropy"
    PackageKind = "DisposableAcceptanceFixtureKit"
    GeneratedAtUtc = [DateTimeOffset]::UtcNow.ToString("O")
    Configuration = $Configuration
    RuntimeMode = "FrameworkDependent"
    RequiredRuntime = ".NET 8 Runtime"
    SourceProject = "Css.AcceptanceFixtures"
    NotForEndUsers = $true
    MutationCommandsRequireAttestation = $true
    PrimaryMachineAllowed = $false
    Commands = @("provision", "status", "uninstall", "lock", "reset")
    Files = $files
}
$manifestPath = Join-Path $OutputDirectory "package-manifest.json"
$utf8WithoutBom = [Text.UTF8Encoding]::new($false)
[IO.File]::WriteAllText(
    $manifestPath,
    ($manifest | ConvertTo-Json -Depth 8),
    $utf8WithoutBom)

$zipPath = "$OutputDirectory.zip"
if (Test-Path -LiteralPath $zipPath) {
    throw "Fixture ZIP output already exists."
}
Compress-Archive `
    -Path (Join-Path $OutputDirectory "*") `
    -DestinationPath $zipPath

[pscustomobject]@{
    PackageDirectory = $OutputDirectory
    ZipPath = $zipPath
    PackageKind = "DisposableAcceptanceFixtureKit"
    MutationCommandsRequireAttestation = $true
    PrimaryMachineAllowed = $false
    FilesHashed = $files.Count
}
