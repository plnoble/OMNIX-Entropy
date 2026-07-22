param(
    [string]$SignToolPath,
    [switch]$AsJson
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$codeSigningOid = "1.3.6.1.5.5.7.3.3"
$rsaPublicKeyOid = "1.2.840.113549.1.1.1"
$certificateStorePath = "Cert:\CurrentUser\My"

function Test-SignToolFile {
    param([Parameter(Mandatory = $true)][string]$Path)

    if (-not [IO.Path]::IsPathRooted($Path)) {
        return $null
    }

    $resolved = [IO.Path]::GetFullPath($Path)
    if (-not (Test-Path -LiteralPath $resolved -PathType Leaf)) {
        return $null
    }
    if (-not [string]::Equals(
            [IO.Path]::GetFileName($resolved),
            "signtool.exe",
            [StringComparison]::OrdinalIgnoreCase)) {
        return $null
    }

    return $resolved
}

function Test-CodeSigningEku {
    param(
        [Parameter(Mandatory = $true)]
        [Security.Cryptography.X509Certificates.X509Certificate2]$Certificate
    )

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

    return $null -ne $Certificate.PublicKey -and
        $null -ne $Certificate.PublicKey.Oid -and
        $Certificate.PublicKey.Oid.Value -eq $rsaPublicKeyOid
}

function Get-WindowsKitsInstallRoots {
    $roots = @()
    $seen = New-Object 'Collections.Generic.HashSet[string]' (
        [StringComparer]::OrdinalIgnoreCase)
    $registryPaths = @(
        "HKLM:\SOFTWARE\Microsoft\Windows Kits\Installed Roots",
        "HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows Kits\Installed Roots"
    )

    foreach ($registryPath in $registryPaths) {
        try {
            $property = Get-ItemProperty -LiteralPath $registryPath `
                -Name "KitsRoot10" -ErrorAction Stop
            $candidate = [string]$property.KitsRoot10
            if ([string]::IsNullOrWhiteSpace($candidate) -or
                -not [IO.Path]::IsPathRooted($candidate)) {
                continue
            }

            $resolved = [IO.Path]::GetFullPath($candidate)
            if ((Test-Path -LiteralPath $resolved -PathType Container) -and
                $seen.Add($resolved)) {
                $roots += [pscustomobject]@{
                    Path = $resolved
                    Resolution = "WindowsKitsRegistry"
                }
            }
        }
        catch {
            continue
        }
    }

    $programFilesX86 = [Environment]::GetFolderPath(
        [Environment+SpecialFolder]::ProgramFilesX86)
    if (-not [string]::IsNullOrWhiteSpace($programFilesX86)) {
        $defaultRoot = Join-Path $programFilesX86 "Windows Kits\10"
        if ((Test-Path -LiteralPath $defaultRoot -PathType Container) -and
            $seen.Add($defaultRoot)) {
            $roots += [pscustomobject]@{
                Path = [IO.Path]::GetFullPath($defaultRoot)
                Resolution = "WindowsKitsDefault"
            }
        }
    }

    return $roots
}

function Find-SignToolUnderWindowsKitsRoot {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$Resolution
    )

    if (-not [IO.Path]::IsPathRooted($Root)) {
        return $null
    }

    $resolvedRoot = [IO.Path]::GetFullPath($Root)
    if (-not (Test-Path -LiteralPath $resolvedRoot -PathType Container)) {
        return $null
    }

    $windowsKitsBin = Join-Path $resolvedRoot "bin"
    if (-not (Test-Path -LiteralPath $windowsKitsBin -PathType Container)) {
        return $null
    }

    $versionDirectories = @(
        Get-ChildItem -LiteralPath $windowsKitsBin -Directory -ErrorAction SilentlyContinue |
            Sort-Object Name -Descending
    )
    foreach ($versionDirectory in $versionDirectories) {
        foreach ($architecture in @("x64", "x86", "arm64")) {
            $candidate = Join-Path $versionDirectory.FullName `
                (Join-Path $architecture "signtool.exe")
            $resolved = Test-SignToolFile -Path $candidate
            if ($null -ne $resolved) {
                return [pscustomobject]@{
                    Path = $resolved
                    Resolution = $Resolution
                }
            }
        }
    }

    return $null
}

function Find-BoundedSignTool {
    $pathCommand = Get-Command signtool.exe -ErrorAction SilentlyContinue |
        Select-Object -First 1
    if ($null -ne $pathCommand) {
        $fromPath = Test-SignToolFile -Path ([string]$pathCommand.Source)
        if ($null -ne $fromPath) {
            return [pscustomobject]@{
                Path = $fromPath
                Resolution = "Path"
            }
        }
    }

    foreach ($root in @(Get-WindowsKitsInstallRoots)) {
        $fromRoot = Find-SignToolUnderWindowsKitsRoot `
            -Root ([string]$root.Path) `
            -Resolution ([string]$root.Resolution)
        if ($null -ne $fromRoot) {
            return $fromRoot
        }
    }

    return $null
}

$signTool = $null
$signToolResolution = "NotFound"
$explicitSignToolInvalid = $false
if (-not [string]::IsNullOrWhiteSpace($SignToolPath)) {
    $resolvedExplicit = Test-SignToolFile -Path $SignToolPath
    if ($null -eq $resolvedExplicit) {
        $explicitSignToolInvalid = $true
        $signToolResolution = "ExplicitPathInvalid"
    }
    else {
        $signTool = $resolvedExplicit
        $signToolResolution = "ExplicitPath"
    }
}
else {
    $boundedSignTool = Find-BoundedSignTool
    if ($null -ne $boundedSignTool) {
        $signTool = [string]$boundedSignTool.Path
        $signToolResolution = [string]$boundedSignTool.Resolution
    }
}

$certificateStoreReadable = $true
$certificates = @()
try {
    $certificates = @(
        Get-ChildItem -LiteralPath $certificateStorePath -ErrorAction Stop)
}
catch {
    $certificateStoreReadable = $false
    $certificates = @()
}

$eligibleCertificates = @()
if ($certificateStoreReadable) {
    $now = Get-Date
    $eligibleCertificates = @(@(
        foreach ($certificate in $certificates) {
            if (-not $certificate.HasPrivateKey -or
                $certificate.NotBefore -gt $now -or
                $certificate.NotAfter -le $now -or
                -not (Test-RsaPublicKey -Certificate $certificate) -or
                -not (Test-CodeSigningEku -Certificate $certificate)) {
                continue
            }

            [pscustomobject]@{
                Thumbprint = ([string]$certificate.Thumbprint -replace "\s", "").ToUpperInvariant()
                Subject = [string]$certificate.Subject
                NotAfter = $certificate.NotAfter.ToUniversalTime().ToString(
                    "o",
                    [Globalization.CultureInfo]::InvariantCulture)
                HasPrivateKey = $true
                HasCodeSigningEku = $true
                PublicKeyAlgorithm = "RSA"
            }
        }
    ) | Sort-Object NotAfter, Thumbprint)
}

$missingRequirements = @()
if ($explicitSignToolInvalid) {
    $missingRequirements += "The explicit SignToolPath is not a fully qualified existing signtool.exe."
}
elseif ($null -eq $signTool) {
    $missingRequirements += "Windows SDK signtool.exe was not found on PATH or in bounded Windows Kits locations."
}
if (-not $certificateStoreReadable) {
    $missingRequirements += "Cert:\CurrentUser\My could not be read."
}
elseif ($eligibleCertificates.Count -eq 0) {
    $missingRequirements += "No currently valid RSA CurrentUser code-signing certificate with an accessible private key was found."
}

$canCreateSignedCandidate =
    $null -ne $signTool -and
    $certificateStoreReadable -and
    $eligibleCertificates.Count -gt 0

$report = [pscustomobject]@{
    InspectionMode = "ReadOnly"
    InspectedAtUtc = [DateTimeOffset]::UtcNow.ToString(
        "o",
        [Globalization.CultureInfo]::InvariantCulture)
    SignToolFound = $null -ne $signTool
    SignToolPath = $signTool
    SignToolResolution = $signToolResolution
    CertificateStore = "CurrentUser\My"
    CertificateStoreReadable = $certificateStoreReadable
    CodeSigningCertificateCount = $eligibleCertificates.Count
    EligibleCodeSigningCertificates = $eligibleCertificates
    RequiresExplicitCertificateSelection = $eligibleCertificates.Count -gt 0
    CanCreateSignedCandidate = $canCreateSignedCandidate
    MissingRequirements = $missingRequirements
    Readiness = if ($canCreateSignedCandidate) {
        "ReadyForExplicitCertificateSelection"
    }
    else {
        "MissingPrerequisites"
    }
}

if ($AsJson) {
    $report | ConvertTo-Json -Depth 5
}
else {
    $report
}
