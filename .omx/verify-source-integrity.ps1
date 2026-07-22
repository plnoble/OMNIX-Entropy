param(
    [string[]]$Roots = @('src', 'tests')
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$strictUtf8 = New-Object System.Text.UTF8Encoding($false, $true)
$files = @(
    Get-ChildItem -Path $Roots -Recurse -File |
        Where-Object {
            ($_.Extension -eq '.cs' -or $_.Extension -eq '.xaml') -and
            $_.FullName -notmatch '[\\/](bin|obj)[\\/]'
        }
)
$invalidUtf8 = New-Object System.Collections.Generic.List[string]
$replacementCharacterFiles = New-Object System.Collections.Generic.List[string]

foreach ($file in $files) {
    try {
        $text = $strictUtf8.GetString([System.IO.File]::ReadAllBytes($file.FullName))
        if ($text.Contains([char]0xFFFD)) {
            $replacementCharacterFiles.Add($file.FullName)
        }
    }
    catch {
        $invalidUtf8.Add($file.FullName)
    }
}

$xamlFiles = @($files | Where-Object Extension -eq '.xaml')
$invalidXaml = New-Object System.Collections.Generic.List[string]
foreach ($file in $xamlFiles) {
    try {
        [xml](Get-Content -LiteralPath $file.FullName -Raw -Encoding UTF8) | Out-Null
    }
    catch {
        $invalidXaml.Add($file.FullName)
    }
}

$result = [pscustomobject]@{
    SourceFiles = $files.Count
    InvalidUtf8 = $invalidUtf8.Count
    ReplacementFiles = $replacementCharacterFiles.Count
    XamlFiles = $xamlFiles.Count
    InvalidXaml = $invalidXaml.Count
}
$result | Format-List

if ($invalidUtf8.Count -gt 0) {
    Write-Output 'Invalid UTF-8 files:'
    $invalidUtf8
}
if ($replacementCharacterFiles.Count -gt 0) {
    Write-Output 'Files containing U+FFFD:'
    $replacementCharacterFiles
}
if ($invalidXaml.Count -gt 0) {
    Write-Output 'Invalid XAML files:'
    $invalidXaml
}

if ($invalidUtf8.Count -gt 0 -or
    $replacementCharacterFiles.Count -gt 0 -or
    $invalidXaml.Count -gt 0) {
    exit 1
}
