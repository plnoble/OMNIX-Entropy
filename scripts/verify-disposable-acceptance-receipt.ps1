param(
    [Parameter(Mandatory = $true)]
    [string]$PackageDirectory,

    [Parameter(Mandatory = $true)]
    [string]$FixtureKitDirectory,

    [Parameter(Mandatory = $true)]
    [string]$SessionDirectory,

    [Parameter(Mandatory = $true)]
    [string]$ReceiptPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$requiredAttestation =
    "I CONFIRM THIS IS A DISPOSABLE WINDOWS TEST ENVIRONMENT"
$allowedEnvironmentKinds = @(
    "WindowsSandbox",
    "VirtualMachine",
    "DedicatedTestMachine"
)
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

function Assert-NoReparsePath {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Label
    )

    $item = Get-Item -LiteralPath $Path
    if (($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
        throw "$Label cannot be a ReparsePoint."
    }
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

function ConvertTo-RequiredUtcTimestamp {
    param(
        [Parameter(Mandatory = $true)]$Value,
        [Parameter(Mandatory = $true)][string]$Label
    )

    $parsed = [DateTimeOffset]::MinValue
    if ($null -eq $Value -or
        -not [DateTimeOffset]::TryParse(
            [string]$Value,
            [Globalization.CultureInfo]::InvariantCulture,
            [Globalization.DateTimeStyles]::RoundtripKind,
            [ref]$parsed)) {
        throw "$Label must be a valid timestamp."
    }
    return $parsed.ToUniversalTime()
}

if (-not [IO.Path]::IsPathRooted($SessionDirectory)) {
    throw "SessionDirectory must be a fully qualified local path."
}
$SessionDirectory = [IO.Path]::GetFullPath($SessionDirectory)
if (-not (Test-Path -LiteralPath $SessionDirectory -PathType Container)) {
    throw "SessionDirectory does not exist."
}
$sessionDrive = [IO.DriveInfo]::new(
    [IO.Path]::GetPathRoot($SessionDirectory))
if ($sessionDrive.DriveType -ne [IO.DriveType]::Fixed) {
    throw "SessionDirectory must be on a Fixed local drive."
}
Assert-NoReparsePath -Path $SessionDirectory -Label "SessionDirectory"
foreach ($sessionItem in @(
        Get-ChildItem -LiteralPath $SessionDirectory -Recurse -Force)) {
    if (($sessionItem.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
        throw "Session content cannot contain a ReparsePoint."
    }
}

$sessionPrefix = $SessionDirectory.TrimEnd(
    [IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
if (-not [IO.Path]::IsPathRooted($ReceiptPath)) {
    $ReceiptPath = Join-Path $SessionDirectory $ReceiptPath
}
$ReceiptPath = [IO.Path]::GetFullPath($ReceiptPath)
if (-not $ReceiptPath.StartsWith(
        $sessionPrefix,
        [StringComparison]::OrdinalIgnoreCase) -or
    -not (Test-Path -LiteralPath $ReceiptPath -PathType Leaf)) {
    throw "ReceiptPath must be an existing file inside SessionDirectory."
}
Assert-NoReparsePath -Path $ReceiptPath -Label "ReceiptPath"

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
    throw "Signed release candidate preflight did not allow receipt verification."
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

$candidateManifestPath = Join-Path $PackageDirectory "package-manifest.json"
$candidateManifestHash = (
    Get-FileHash -LiteralPath $candidateManifestPath -Algorithm SHA256).Hash
$fixtureManifestPath = Join-Path $FixtureKitDirectory "package-manifest.json"
$fixtureManifestHash = (
    Get-FileHash -LiteralPath $fixtureManifestPath -Algorithm SHA256).Hash
$sessionManifestPath = Join-Path $SessionDirectory "session-manifest.json"
if (-not (Test-Path -LiteralPath $sessionManifestPath -PathType Leaf)) {
    throw "Session manifest is missing."
}
$sessionManifestHash = (
    Get-FileHash -LiteralPath $sessionManifestPath -Algorithm SHA256).Hash
$session = Get-Content -LiteralPath $sessionManifestPath -Raw |
    ConvertFrom-Json
$receipt = Get-Content -LiteralPath $ReceiptPath -Raw |
    ConvertFrom-Json

if ($session.SchemaVersion -ne 1 -or
    $session.SessionKind -ne "DisposableWindowsBehavioralAcceptance") {
    throw "Session manifest schema is unsupported."
}
if ($receipt.SchemaVersion -ne 1 -or
    $receipt.ReceiptKind -ne
        "DisposableWindowsBehavioralAcceptanceReceipt") {
    throw "Receipt schema is unsupported."
}
if ([string]::IsNullOrWhiteSpace([string]$session.SessionId) -or
    $receipt.SessionId -ne $session.SessionId) {
    throw "Receipt SessionId does not match the session manifest."
}
if (-not [string]::Equals(
        [string]$receipt.SessionManifestSHA256,
        $sessionManifestHash,
        [StringComparison]::OrdinalIgnoreCase)) {
    throw "SessionManifestSHA256 does not match the session manifest."
}
foreach ($record in @($session, $receipt)) {
    if (-not [string]::Equals(
            [string]$record.CandidateManifestSHA256,
            $candidateManifestHash,
            [StringComparison]::OrdinalIgnoreCase)) {
        throw "CandidateManifestSHA256 does not match the package manifest."
    }
    if (-not [string]::Equals(
            [string]$record.SignerThumbprint,
            [string]$preflight.SignerThumbprint,
            [StringComparison]::OrdinalIgnoreCase)) {
        throw "SignerThumbprint does not match the verified candidate."
    }
    if (-not [string]::Equals(
            [string]$record.FixtureManifestSHA256,
            $fixtureManifestHash,
            [StringComparison]::OrdinalIgnoreCase)) {
        throw "FixtureManifestSHA256 does not match the verified fixture kit."
    }
}

if ($session.Environment.PrimaryMachine -ne $false -or
    $receipt.Environment.PrimaryMachine -ne $false) {
    throw "PrimaryMachine must remain false."
}
if ($session.Environment.IsDisposableEnvironment -ne $true -or
    $receipt.Environment.IsDisposableEnvironment -ne $true) {
    throw "IsDisposableEnvironment must remain true."
}
if ($allowedEnvironmentKinds -notcontains [string]$session.Environment.Kind -or
    $receipt.Environment.Kind -ne $session.Environment.Kind) {
    throw "Environment kind is not allowlisted or changed."
}
if ([string]::IsNullOrWhiteSpace(
        [string]$session.Environment.ResetCheckpointId) -or
    $receipt.Environment.ResetCheckpointId -ne
        $session.Environment.ResetCheckpointId) {
    throw "ResetCheckpointId is missing or changed."
}
if ($session.Environment.OperatorAttestation -ne $requiredAttestation) {
    throw "Session operator attestation is invalid."
}
if ($receipt.Environment.ResetCompleted -ne $true) {
    throw "ResetCompleted must be true before final verification."
}

$createdUtc = ConvertTo-RequiredUtcTimestamp `
    -Value $session.CreatedUtc `
    -Label "CreatedUtc"
$completedUtc = ConvertTo-RequiredUtcTimestamp `
    -Value $receipt.CompletedUtc `
    -Label "CompletedUtc"
$resetCompletedUtc = ConvertTo-RequiredUtcTimestamp `
    -Value $receipt.Environment.ResetCompletedUtc `
    -Label "ResetCompletedUtc"
if ($completedUtc -lt $createdUtc -or
    $resetCompletedUtc -lt $completedUtc) {
    throw "Receipt completion or reset timestamps are out of order."
}
if ([string]::IsNullOrWhiteSpace([string]$receipt.OperatorName)) {
    throw "OperatorName is required."
}
if ($receipt.FinalVerdict -ne "Pass") {
    throw "FinalVerdict must be Pass."
}

$requiredSet = @{}
foreach ($requiredCaseId in $requiredCaseIds) {
    $requiredSet[$requiredCaseId] = $true
}
$sessionCaseSet = @{}
foreach ($sessionCaseId in @($session.RequiredCases)) {
    $caseId = [string]$sessionCaseId
    if ($sessionCaseSet.ContainsKey($caseId)) {
        throw "Session manifest contains a duplicate case id."
    }
    $sessionCaseSet[$caseId] = $true
}
if ($sessionCaseSet.Count -ne $requiredSet.Count) {
    throw "Session manifest does not contain the exact required case set."
}
foreach ($requiredCaseId in $requiredCaseIds) {
    if (-not $sessionCaseSet.ContainsKey($requiredCaseId)) {
        throw "Session manifest does not contain the exact required case set."
    }
}

$receiptCaseSet = @{}
$evidenceSet = @{}
$evidenceDirectory = Join-Path $SessionDirectory "evidence"
$evidencePrefix = [IO.Path]::GetFullPath($evidenceDirectory).TrimEnd(
    [IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
foreach ($case in @($receipt.Cases)) {
    $caseId = [string]$case.Id
    if ($receiptCaseSet.ContainsKey($caseId)) {
        throw "Receipt contains a duplicate case id."
    }
    $receiptCaseSet[$caseId] = $true
    if (-not $requiredSet.ContainsKey($caseId)) {
        throw "Receipt does not contain the exact required case set."
    }
    if ($case.Outcome -ne "Pass") {
        throw "Every required case Outcome must be Pass."
    }
    if ([string]::IsNullOrWhiteSpace([string]$case.OperatorNotes)) {
        throw "Every required case needs operator notes."
    }
    $startedUtc = ConvertTo-RequiredUtcTimestamp `
        -Value $case.StartedUtc `
        -Label "$caseId StartedUtc"
    $finishedUtc = ConvertTo-RequiredUtcTimestamp `
        -Value $case.FinishedUtc `
        -Label "$caseId FinishedUtc"
    if ($startedUtc -lt $createdUtc -or
        $finishedUtc -lt $startedUtc -or
        $finishedUtc -gt $completedUtc) {
        throw "Case timestamps are out of order: $caseId"
    }

    $evidenceEntries = @($case.Evidence)
    if ($evidenceEntries.Count -eq 0) {
        throw "Every required case needs at least one evidence file."
    }
    foreach ($evidence in $evidenceEntries) {
        $relativePath = [string]$evidence.Path
        $segments = $relativePath -split '[\\/]'
        if ([string]::IsNullOrWhiteSpace($relativePath) -or
            [IO.Path]::IsPathRooted($relativePath) -or
            $segments -contains "..") {
            throw "Evidence path must be relative and remain inside the evidence directory."
        }
        $evidencePath = [IO.Path]::GetFullPath((
            Join-Path $evidenceDirectory $relativePath))
        if (-not $evidencePath.StartsWith(
                $evidencePrefix,
                [StringComparison]::OrdinalIgnoreCase)) {
            throw "Evidence path must be relative and remain inside the evidence directory."
        }
        $evidenceKey = $evidencePath.ToUpperInvariant()
        if ($evidenceSet.ContainsKey($evidenceKey)) {
            throw "Evidence file cannot be reused by multiple case entries."
        }
        $evidenceSet[$evidenceKey] = $true
        if (-not (Test-Path -LiteralPath $evidencePath -PathType Leaf)) {
            throw "Evidence file is missing: $relativePath"
        }
        $file = Get-Item -LiteralPath $evidencePath
        if ($file.Length -ne [long]$evidence.Length) {
            throw "Evidence file length verification failed: $relativePath"
        }
        $hash = Get-FileHash -LiteralPath $evidencePath -Algorithm SHA256
        if (-not [string]::Equals(
                $hash.Hash,
                [string]$evidence.SHA256,
                [StringComparison]::OrdinalIgnoreCase)) {
            throw "Evidence file hash verification failed: $relativePath"
        }
    }
}
if ($receiptCaseSet.Count -ne $requiredSet.Count) {
    throw "Receipt does not contain the exact required case set."
}
foreach ($requiredCaseId in $requiredCaseIds) {
    if (-not $receiptCaseSet.ContainsKey($requiredCaseId)) {
        throw "Receipt does not contain the exact required case set."
    }
}

[pscustomobject]@{
    BehavioralAcceptanceComplete = $true
    AcceptanceStatus = "BehavioralAcceptancePassed"
    SessionId = [string]$session.SessionId
    SignerThumbprint = [string]$preflight.SignerThumbprint
    CandidateManifestSHA256 = $candidateManifestHash
    FixtureManifestSHA256 = $fixtureManifestHash
    RequiredCasesPassed = $receiptCaseSet.Count
    EvidenceFilesVerified = $evidenceSet.Count
    ResetCompleted = $true
}
