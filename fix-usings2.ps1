# PowerShell script to add missing System.IO usings
$files = @(
    "Utils\SvnPathHelper.cs",
    "Utils\PathHelper.cs",
    "Utils\ProcessHelper.cs",
    "Services\Svn\Core\SvnCommandService.cs",
    "Services\Svn\Operations\WorkingCopyService.cs",
    "Services\Svn\Parsers\SvnStatusParser.cs",
    "Services\Svn\Parsers\SvnDiffParser.cs",
    "ViewModels\MainWindowViewModel.cs",
    "Converters\BoolToVisibilityConverter.cs",
    "Models\SvnProperty.cs",
    "Models\SvnRepositoryInfo.cs",
    "Converters\FilePathConverter.cs"
)

$usingsToAdd = @(
    "using System.IO;",
    "using System.Text;"
)

Write-Host "Adding System.IO usings..."

foreach ($file in $files) {
    $filePath = Join-Path $PSScriptRoot $file

    if (Test-Path $filePath) {
        Write-Host "Processing: $file"

        $content = Get-Content $filePath -Raw
        $lines = $content -split "`r?`n"

        # Find where to insert usings (after existing usings)
        $insertIndex = 0
        $existingUsings = @()

        for ($i = 0; $i -lt $lines.Count; $i++) {
            if ($lines[$i] -match "^using\s+") {
                $existingUsings += $lines[$i]
                $insertIndex = $i + 1
            } else {
                break
            }
        }

        # Add missing usings
        $newLines = @()
        $added = $false

        for ($i = 0; $i -lt $lines.Count; $i++) {
            if ($i -lt $insertIndex) {
                $newLines += $lines[$i]
            } elseif (-not $added) {
                foreach ($using in $usingsToAdd) {
                    $usingName = $using -replace "^using ", "" -replace ";$", ""
                    if ($existingUsings -notmatch [regex]::Escape($usingName)) {
                        $newLines += $using
                        Write-Host "  Added: $using"
                    }
                }
                $newLines += $lines[$i]
                $added = $true
            } else {
                $newLines += $lines[$i]
            }
        }

        $newContent = $newLines -join "`r`n"
        Set-Content -Path $filePath -Value $newContent -NoNewline
    }
}

Write-Host "Done!"
