param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("I approve CurrentUser OMNIX personal publisher trust")]
    [string]$Attestation,
    [string]$RootTrustAttestation
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$subject = "CN=OMNIX-Entropy Personal Publisher"
$friendlyName = "OMNIX-Entropy Personal Code Signing"
$myStore = "Cert:\CurrentUser\My"
$trustedPeopleStore = "Cert:\CurrentUser\TrustedPeople"
$trustedPublisherStore = "Cert:\CurrentUser\TrustedPublisher"
$rootStore = "Cert:\CurrentUser\Root"
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
$artifactRoot = [IO.Path]::GetFullPath((Join-Path $repoRoot ".artifacts"))
$evidenceDirectory = Join-Path $artifactRoot "personal-signing"
$publicCertificatePath = Join-Path $evidenceDirectory "OMNIX-Entropy-Personal-Publisher.cer"

if ($Attestation -ne "I approve CurrentUser OMNIX personal publisher trust") {
    throw "Exact personal trust attestation is required."
}

$matching = @(
    Get-ChildItem -LiteralPath $myStore |
        Where-Object {
            $_.Subject -eq $subject -and
            $_.FriendlyName -eq $friendlyName
        }
)
if ($matching.Count -gt 1) {
    throw "Multiple OMNIX personal signing certificates already exist."
}

$created = $false
if ($matching.Count -eq 0) {
    $certificate = New-SelfSignedCertificate `
        -Type CodeSigningCert `
        -Subject $subject `
        -FriendlyName $friendlyName `
        -CertStoreLocation $myStore `
        -KeyAlgorithm RSA `
        -KeyLength 3072 `
        -HashAlgorithm SHA256 `
        -KeyExportPolicy NonExportable `
        -NotAfter (Get-Date).AddYears(3)
    $created = $true
} else {
    $certificate = $matching[0]
}

if (-not $certificate.HasPrivateKey -or
    $null -eq $certificate.PublicKey -or
    $null -eq $certificate.PublicKey.Oid -or
    $certificate.PublicKey.Oid.Value -ne "1.2.840.113549.1.1.1") {
    throw "OMNIX personal certificate is not an RSA certificate with a private key."
}

if (-not (Test-Path -LiteralPath $artifactRoot -PathType Container)) {
    New-Item -ItemType Directory -Path $artifactRoot | Out-Null
}
if (-not (Test-Path -LiteralPath $evidenceDirectory -PathType Container)) {
    New-Item -ItemType Directory -Path $evidenceDirectory | Out-Null
}
if (-not (Test-Path -LiteralPath $publicCertificatePath -PathType Leaf)) {
    Export-Certificate `
        -Cert $certificate `
        -FilePath $publicCertificatePath `
        -Type CERT | Out-Null
}

foreach ($store in @($trustedPeopleStore, $trustedPublisherStore)) {
    $trustedPath = Join-Path $store $certificate.Thumbprint
    if (-not (Test-Path -LiteralPath $trustedPath -PathType Leaf)) {
        Import-Certificate `
            -FilePath $publicCertificatePath `
            -CertStoreLocation $store | Out-Null
    }
}

$rootStoreModified = $false
if (-not [string]::IsNullOrWhiteSpace($RootTrustAttestation)) {
    $expectedRootAttestation =
        "I approve CurrentUser Root trust for $($certificate.Thumbprint)"
    if ($RootTrustAttestation -ne $expectedRootAttestation) {
        throw "Exact thumbprint-bound CurrentUser Root attestation is required."
    }

    $rootPath = Join-Path $rootStore $certificate.Thumbprint
    if (-not (Test-Path -LiteralPath $rootPath -PathType Leaf)) {
        Import-Certificate `
            -FilePath $publicCertificatePath `
            -CertStoreLocation $rootStore | Out-Null
        $rootStoreModified = $true
    }
}

[pscustomobject]@{
    Scope = "CurrentUser"
    Subject = $certificate.Subject
    FriendlyName = $certificate.FriendlyName
    Thumbprint = $certificate.Thumbprint
    HasPrivateKey = $certificate.HasPrivateKey
    PublicKeyAlgorithm = $certificate.PublicKey.Oid.FriendlyName
    PrivateKeyExported = $false
    RootStoreModified = $rootStoreModified
    RootTrusted = Test-Path -LiteralPath (Join-Path $rootStore $certificate.Thumbprint)
    TrustedPeople = Test-Path -LiteralPath (Join-Path $trustedPeopleStore $certificate.Thumbprint)
    TrustedPublisher = Test-Path -LiteralPath (Join-Path $trustedPublisherStore $certificate.Thumbprint)
    PublicCertificatePath = $publicCertificatePath
    Created = $created
}
