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
    $OutputDirectory = Join-Path $artifactRoot "OMNIX-Entropy-test-$stamp"
}
elseif ([IO.Path]::IsPathRooted($OutputDirectory)) {
    $OutputDirectory = [IO.Path]::GetFullPath($OutputDirectory)
}
else {
    $OutputDirectory = [IO.Path]::GetFullPath((Join-Path $repoRoot $OutputDirectory))
}

$artifactPrefix = $artifactRoot.TrimEnd([IO.Path]::DirectorySeparatorChar) +
    [IO.Path]::DirectorySeparatorChar
if (-not $OutputDirectory.StartsWith($artifactPrefix, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Output must be a new child directory under: $artifactRoot"
}

if (Test-Path -LiteralPath $OutputDirectory) {
    throw "Output already exists: $OutputDirectory"
}

if (-not (Test-Path -LiteralPath $artifactRoot)) {
    New-Item -ItemType Directory -Path $artifactRoot | Out-Null
}
New-Item -ItemType Directory -Path $OutputDirectory | Out-Null

$appProject = Join-Path $repoRoot "src\Css.App\Css.App.csproj"
$workerProject = Join-Path $repoRoot "src\Css.Elevated\Css.Elevated.csproj"

function Invoke-ProjectPublish {
    param([Parameter(Mandatory = $true)][string]$ProjectPath)

    & dotnet publish $ProjectPath `
        --configuration $Configuration `
        --no-restore `
        --no-self-contained `
        --output $OutputDirectory
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed for: $ProjectPath"
    }
}

function Get-PackageRelativePath {
    param(
        [Parameter(Mandatory = $true)][string]$BaseDirectory,
        [Parameter(Mandatory = $true)][string]$FilePath
    )

    $baseUri = [Uri]($BaseDirectory.TrimEnd([IO.Path]::DirectorySeparatorChar) +
        [IO.Path]::DirectorySeparatorChar)
    $fileUri = [Uri]$FilePath
    return [Uri]::UnescapeDataString(
        $baseUri.MakeRelativeUri($fileUri).ToString()).Replace('/', '\')
}

function Test-ByteSequence {
    param(
        [Parameter(Mandatory = $true)][byte[]]$Buffer,
        [Parameter(Mandatory = $true)][byte[]]$Sequence
    )

    if ($Sequence.Length -eq 0 -or $Sequence.Length -gt $Buffer.Length) {
        return $false
    }

    for ($offset = 0; $offset -le $Buffer.Length - $Sequence.Length; $offset++) {
        $matches = $true
        for ($index = 0; $index -lt $Sequence.Length; $index++) {
            if ($Buffer[$offset + $index] -ne $Sequence[$index]) {
                $matches = $false
                break
            }
        }
        if ($matches) {
            return $true
        }
    }

    return $false
}

Invoke-ProjectPublish -ProjectPath $appProject
Invoke-ProjectPublish -ProjectPath $workerProject

$requiredFiles = @(
    "Css.App.exe",
    "Css.App.dll",
    "Css.App.runtimeconfig.json",
    "Css.Elevated.exe",
    "Css.Elevated.dll",
    "Css.Elevated.runtimeconfig.json",
    "rules.scan.json"
)

foreach ($requiredFile in $requiredFiles) {
    $requiredPath = Join-Path $OutputDirectory $requiredFile
    if (-not (Test-Path -LiteralPath $requiredPath -PathType Leaf)) {
        throw "Required package file is missing: $requiredFile"
    }
}

$workerAssemblyPath = Join-Path $OutputDirectory "Css.Elevated.dll"
$debugCommandToken = "official-uninstall-fake-worker"
$workerAssemblyBytes = [IO.File]::ReadAllBytes($workerAssemblyPath)
$utf8DebugCommand = [Text.Encoding]::UTF8.GetBytes($debugCommandToken)
$unicodeDebugCommand = [Text.Encoding]::Unicode.GetBytes($debugCommandToken)
if ((Test-ByteSequence -Buffer $workerAssemblyBytes -Sequence $utf8DebugCommand) -or
    (Test-ByteSequence -Buffer $workerAssemblyBytes -Sequence $unicodeDebugCommand)) {
    throw "Release worker includes debug-only command surface: $debugCommandToken"
}

$appPath = Join-Path $OutputDirectory "Css.App.exe"
$workerPath = Join-Path $OutputDirectory "Css.Elevated.exe"
$appSignature = Get-AuthenticodeSignature -LiteralPath $appPath
$workerSignature = Get-AuthenticodeSignature -LiteralPath $workerPath
$appSignatureStatus = $appSignature.Status.ToString()
$workerSignatureStatus = $workerSignature.Status.ToString()
$appSignerThumbprint = if ($null -eq $appSignature.SignerCertificate) {
    $null
} else {
    $appSignature.SignerCertificate.Thumbprint
}
$workerSignerThumbprint = if ($null -eq $workerSignature.SignerCertificate) {
    $null
} else {
    $workerSignature.SignerCertificate.Thumbprint
}
$validSameSigner =
    $appSignatureStatus -eq "Valid" -and
    $workerSignatureStatus -eq "Valid" -and
    -not [string]::IsNullOrWhiteSpace($appSignerThumbprint) -and
    [string]::Equals(
        $appSignerThumbprint,
        $workerSignerThumbprint,
        [StringComparison]::OrdinalIgnoreCase)

$mutationReadiness = if ($validSameSigner) {
    "EligibleForDisposableMachineAcceptance"
} else {
    "BlockedUntilValidSameSignerPackage"
}

$readmePath = Join-Path $OutputDirectory "README-TEST.txt"
$readmeTemplatePath = Join-Path $repoRoot "scripts\README-PORTABLE-TEST.zh-CN.txt"
if (-not (Test-Path -LiteralPath $readmeTemplatePath -PathType Leaf)) {
    throw "Test package readme template is missing: $readmeTemplatePath"
}
Copy-Item -LiteralPath $readmeTemplatePath -Destination $readmePath
$utf8WithoutBom = [Text.UTF8Encoding]::new($false)

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

$manifest = [ordered]@{
    Product = "OMNIX-Entropy"
    PackageKind = "PortableTestPackage"
    GeneratedAtUtc = [DateTimeOffset]::UtcNow.ToString("O")
    Configuration = $Configuration
    RuntimeMode = "FrameworkDependent"
    RequiredRuntime = ".NET 8 Desktop Runtime"
    ReleaseCommandSurface = "ProductionOnly"
    ValidSameSigner = $validSameSigner
    MutationReadiness = $mutationReadiness
    App = [ordered]@{
        File = "Css.App.exe"
        SignatureStatus = $appSignatureStatus
        SignerThumbprint = $appSignerThumbprint
    }
    Worker = [ordered]@{
        File = "Css.Elevated.exe"
        SignatureStatus = $workerSignatureStatus
        SignerThumbprint = $workerSignerThumbprint
    }
    Files = $packageFiles
}

$manifestPath = Join-Path $OutputDirectory "package-manifest.json"
$manifestJson = $manifest | ConvertTo-Json -Depth 8
[IO.File]::WriteAllText($manifestPath, $manifestJson, $utf8WithoutBom)

$zipPath = "$OutputDirectory.zip"
if (Test-Path -LiteralPath $zipPath) {
    throw "Output already exists: $zipPath"
}
Compress-Archive -Path (Join-Path $OutputDirectory "*") -DestinationPath $zipPath

[pscustomobject]@{
    PackageDirectory = $OutputDirectory
    ZipPath = $zipPath
    RuntimeMode = "FrameworkDependent"
    AppSignatureStatus = $appSignatureStatus
    WorkerSignatureStatus = $workerSignatureStatus
    ValidSameSigner = $validSameSigner
    MutationReadiness = $mutationReadiness
}
