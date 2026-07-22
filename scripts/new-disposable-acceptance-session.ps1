param(
    [Parameter(Mandatory = $true)]
    [string]$PackageDirectory,

    [Parameter(Mandatory = $true)]
    [string]$FixtureKitDirectory,

    [Parameter(Mandatory = $true)]
    [string]$SessionDirectory,

    [Parameter(Mandatory = $true)]
    [ValidateSet("WindowsSandbox", "VirtualMachine", "DedicatedTestMachine")]
    [string]$EnvironmentKind,

    [Parameter(Mandatory = $true)]
    [ValidateSet("false")]
    [string]$PrimaryMachine,

    [Parameter(Mandatory = $true)]
    [ValidateSet("true")]
    [string]$IsDisposableEnvironment,

    [Parameter(Mandatory = $true)]
    [string]$ResetCheckpointId,

    [Parameter(Mandatory = $true)]
    [string]$OperatorAttestation
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$requiredAttestation =
    "I CONFIRM THIS IS A DISPOSABLE WINDOWS TEST ENVIRONMENT"
$requiredCaseIds = @(
    "package-preflight",
    "uac-cancel-official-uninstall",
    "uac-cancel-migration",
    "cleanup-quarantine-restore",
    "app-cache-quarantine-restore",
    "startup-disable-restore",
    "official-uninstall-residue-review",
    "migration-complete-closure-monitor",
    "migration-failure-rollback",
    "undo-center-restore"
)

function Assert-NoReparseAncestor {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Label
    )

    $item = Get-Item -LiteralPath $Path
    $current = if ($item.PSIsContainer) {
        [IO.DirectoryInfo]$item
    } else {
        $item.Directory
    }
    while ($null -ne $current) {
        if (($current.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
            throw "$Label cannot use a ReparsePoint ancestor."
        }
        $current = $current.Parent
    }
}

if ($PrimaryMachine -ne "false") {
    throw "Behavioral acceptance cannot run on the primary machine."
}
if ($IsDisposableEnvironment -ne "true") {
    throw "The Windows environment must be explicitly disposable."
}
if ([string]::IsNullOrWhiteSpace($ResetCheckpointId)) {
    throw "ResetCheckpointId is required."
}
if (-not [string]::Equals(
        $OperatorAttestation,
        $requiredAttestation,
        [StringComparison]::Ordinal)) {
    throw "OperatorAttestation does not match the required statement."
}
if (-not [IO.Path]::IsPathRooted($SessionDirectory)) {
    throw "SessionDirectory must be a fully qualified local path."
}

$SessionDirectory = [IO.Path]::GetFullPath($SessionDirectory)
if (Test-Path -LiteralPath $SessionDirectory) {
    throw "SessionDirectory already exists."
}
$sessionParent = [IO.Directory]::GetParent($SessionDirectory)
if ($null -eq $sessionParent -or
    -not (Test-Path -LiteralPath $sessionParent.FullName -PathType Container)) {
    throw "SessionDirectory parent must already exist."
}
$sessionDrive = [IO.DriveInfo]::new(
    [IO.Path]::GetPathRoot($SessionDirectory))
if ($sessionDrive.DriveType -ne [IO.DriveType]::Fixed) {
    throw "SessionDirectory must be on a Fixed local drive."
}
Assert-NoReparseAncestor `
    -Path $sessionParent.FullName `
    -Label "SessionDirectory"

$candidateVerifier = Join-Path `
    $PSScriptRoot `
    "verify-signed-release-candidate.ps1"
if (-not (Test-Path -LiteralPath $candidateVerifier -PathType Leaf)) {
    throw "Signed release candidate verifier is missing."
}
$preflight = & $candidateVerifier -PackageDirectory $PackageDirectory
if ($preflight.CanBeginDisposableAcceptance -ne $true -or
    $preflight.DisposableMachineAcceptance -ne $false -or
    $preflight.AcceptanceStatus -ne "AwaitingBehavioralAcceptance") {
    throw "Signed release candidate preflight did not allow behavioral acceptance."
}

$fixtureVerifier = Join-Path `
    $PSScriptRoot `
    "verify-acceptance-fixture-kit.ps1"
if (-not (Test-Path -LiteralPath $fixtureVerifier -PathType Leaf)) {
    throw "Acceptance fixture kit verifier is missing."
}
$fixturePreflight = & $fixtureVerifier `
    -FixtureKitDirectory $FixtureKitDirectory
if ($fixturePreflight.FixtureKitVerified -ne $true -or
    $fixturePreflight.MutationCommandsRequireAttestation -ne $true -or
    $fixturePreflight.PrimaryMachineAllowed -ne $false) {
    throw "Acceptance fixture kit preflight did not pass."
}

$PackageDirectory = [IO.Path]::GetFullPath($PackageDirectory)
$FixtureKitDirectory = [IO.Path]::GetFullPath($FixtureKitDirectory)
$packagePrefix = $PackageDirectory.TrimEnd(
    [IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
$fixturePrefix = $FixtureKitDirectory.TrimEnd(
    [IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
$sessionPrefix = $SessionDirectory.TrimEnd(
    [IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
if ($SessionDirectory.StartsWith(
        $packagePrefix,
        [StringComparison]::OrdinalIgnoreCase) -or
    $PackageDirectory.StartsWith(
        $sessionPrefix,
        [StringComparison]::OrdinalIgnoreCase)) {
    throw "SessionDirectory and PackageDirectory must be separate trees."
}
if ($FixtureKitDirectory.StartsWith(
        $packagePrefix,
        [StringComparison]::OrdinalIgnoreCase) -or
    $PackageDirectory.StartsWith(
        $fixturePrefix,
        [StringComparison]::OrdinalIgnoreCase) -or
    $SessionDirectory.StartsWith(
        $fixturePrefix,
        [StringComparison]::OrdinalIgnoreCase) -or
    $FixtureKitDirectory.StartsWith(
        $sessionPrefix,
        [StringComparison]::OrdinalIgnoreCase)) {
    throw "Candidate, fixture kit, and session must use separate trees."
}

$candidateManifestPath = Join-Path $PackageDirectory "package-manifest.json"
$candidateManifestHash = (
    Get-FileHash -LiteralPath $candidateManifestPath -Algorithm SHA256).Hash
$fixtureManifestPath = Join-Path $FixtureKitDirectory "package-manifest.json"
$fixtureManifestHash = (
    Get-FileHash -LiteralPath $fixtureManifestPath -Algorithm SHA256).Hash
$createdUtc = [DateTimeOffset]::UtcNow.ToString("O")
$sessionId = [Guid]::NewGuid().ToString("D")
$environment = [ordered]@{
    Kind = $EnvironmentKind
    PrimaryMachine = $false
    IsDisposableEnvironment = $true
    ResetCheckpointId = $ResetCheckpointId.Trim()
    OperatorAttestation = $OperatorAttestation
}

$sessionManifest = [ordered]@{
    SchemaVersion = 1
    SessionKind = "DisposableWindowsBehavioralAcceptance"
    SessionId = $sessionId
    CreatedUtc = $createdUtc
    CandidateManifestSHA256 = $candidateManifestHash
    FixtureManifestSHA256 = $fixtureManifestHash
    SignerThumbprint = [string]$preflight.SignerThumbprint
    Environment = $environment
    RequiredCases = $requiredCaseIds
}

New-Item -ItemType Directory -Path $SessionDirectory | Out-Null
$evidenceDirectory = Join-Path $SessionDirectory "evidence"
New-Item -ItemType Directory -Path $evidenceDirectory | Out-Null
$sessionManifestPath = Join-Path $SessionDirectory "session-manifest.json"
$sessionManifest | ConvertTo-Json -Depth 8 |
    Set-Content -LiteralPath $sessionManifestPath -Encoding UTF8
$sessionManifestHash = (
    Get-FileHash -LiteralPath $sessionManifestPath -Algorithm SHA256).Hash

$receiptCases = @(
    foreach ($caseId in $requiredCaseIds) {
        [ordered]@{
            Id = $caseId
            Outcome = "NotRun"
            StartedUtc = $null
            FinishedUtc = $null
            OperatorNotes = ""
            Evidence = @()
        }
    }
)
$receiptTemplate = [ordered]@{
    SchemaVersion = 1
    ReceiptKind = "DisposableWindowsBehavioralAcceptanceReceipt"
    SessionId = $sessionId
    SessionManifestSHA256 = $sessionManifestHash
    CandidateManifestSHA256 = $candidateManifestHash
    FixtureManifestSHA256 = $fixtureManifestHash
    SignerThumbprint = [string]$preflight.SignerThumbprint
    Environment = [ordered]@{
        Kind = $EnvironmentKind
        PrimaryMachine = $false
        IsDisposableEnvironment = $true
        ResetCheckpointId = $ResetCheckpointId.Trim()
        ResetCompleted = $false
        ResetCompletedUtc = $null
    }
    OperatorName = ""
    Cases = $receiptCases
    KnownResidualRisks = @()
    CompletedUtc = $null
    FinalVerdict = "NotRun"
}
$receiptTemplatePath = Join-Path `
    $SessionDirectory `
    "acceptance-receipt.template.json"
$receiptTemplate | ConvertTo-Json -Depth 12 |
    Set-Content -LiteralPath $receiptTemplatePath -Encoding UTF8

[pscustomobject]@{
    SessionDirectory = $SessionDirectory
    SessionId = $sessionId
    SessionManifestPath = $sessionManifestPath
    ReceiptTemplatePath = $receiptTemplatePath
    EvidenceDirectory = $evidenceDirectory
    FixtureKitDirectory = $FixtureKitDirectory
    FixtureManifestSHA256 = $fixtureManifestHash
    RequiredCaseCount = $requiredCaseIds.Count
    BehavioralAcceptanceComplete = $false
    NextStep = "Follow the manual protocol and collect fixture-only evidence."
}
