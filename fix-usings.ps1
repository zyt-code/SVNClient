# PowerShell script to fix missing using statements
# This script adds necessary using directives to C# files

$files = @(
    "Models\SvnBlameLine.cs",
    "Models\SvnChangeList.cs",
    "Models\SvnDiffResult.cs",
    "Models\SvnInfo.cs",
    "Models\SvnLogEntry.cs",
    "Models\WorkingCopyInfo.cs",
    "Services\Svn\Core\SvnCommand.cs",
    "Services\Svn\Core\SvnCommandService.cs",
    "Services\Svn\Core\SvnResult.cs",
    "Services\Svn\Operations\WorkingCopyService.cs",
    "Services\Svn\Parsers\SvnStatusParser.cs",
    "Services\Svn\Parsers\SvnLogParser.cs",
    "Services\Svn\Parsers\SvnListParser.cs",
    "Utils\ProcessHelper.cs",
    "Converters\BoolToVisibilityConverter.cs",
    "Converters\DateTimeConverter.cs",
    "Converters\FilePathConverter.cs",
    "Converters\IntToStringConverter.cs",
    "Converters\SvnStatusToIconConverter.cs",
    "Converters\SvnStatusToColorConverter.cs",
    "Converters\SvnStatusToStringConverter.cs",
    "Converters\InverseBoolConverter.cs"
)

$baseUsings = @(
    "using System;",
    "using System.Collections.Generic;",
    "using System.Linq;",
    "using System.Threading;",
    "using System.Threading.Tasks;",
    "using System.Globalization;"
)

Write-Host "Fixing using statements in files..."

foreach ($file in $files) {
    $filePath = Join-Path $PSScriptRoot $file

    if (Test-Path $filePath) {
        Write-Host "Processing: $file"

        $content = Get-Content $filePath -Raw
        $lines = $content -split "`r?`n"

        # Find namespace line
        $namespaceIndex = -1
        $firstNonCommentIndex = -1

        for ($i = 0; $i -lt $lines.Count; $i++) {
            $line = $lines[$i].Trim()

            # Skip empty lines and comments
            if ([string]::IsNullOrWhiteSpace($line) -or $line.StartsWith("//") -or $line.StartsWith("/*")) {
                continue
            }

            if ($line.StartsWith("using ")) {
                continue
            }

            if ($line.StartsWith("namespace ")) {
                $namespaceIndex = $i
                break
            }

            if ($firstNonCommentIndex -eq -1) {
                $firstNonCommentIndex = $i
            }
        }

        # Collect existing usings
        $existingUsings = @()
        $insertIndex = 0

        for ($i = 0; $i -lt $lines.Count; $i++) {
            $line = $lines[$i].Trim()
            if ($line.StartsWith("using ")) {
                $existingUsings += $line.Substring(6).TrimEnd(';').Trim()
                $insertIndex = $i + 1
            } else {
                break
            }
        }

        # Add missing usings
        $usingsToAdd = @()
        foreach ($using in $baseUsings) {
            $usingName = $using -replace "^using ", "" -replace ";$", ""
            if ($usingName -notin $existingUsings) {
                $usingsToAdd += $using
            }
        }

        if ($usingsToAdd.Count -gt 0) {
            $newLines = @()
            for ($i = 0; $i -lt $insertIndex; $i++) {
                $newLines += $lines[$i]
            }

            # Add new usings
            $newLines += $usingsToAdd

            # Add rest of file
            for ($i = $insertIndex; $i -lt $lines.Count; $i++) {
                $newLines += $lines[$i]
            }

            $newContent = $newLines -join "`r`n"
            Set-Content -Path $filePath -Value $newContent -NoNewline
            Write-Host "  Added $($usingsToAdd.Count) using statements"
        }
    }
}

Write-Host "Done!"
