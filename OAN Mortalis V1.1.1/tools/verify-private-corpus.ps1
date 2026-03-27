param(
    [string] $CorpusPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$script:ResolvedRepoRoot = [System.IO.Path]::GetFullPath((Split-Path -Parent (Split-Path -Parent $PSScriptRoot)))

Set-Location -LiteralPath $script:ResolvedRepoRoot

$script:TextExtensions = @(
    ".cs", ".csproj", ".json", ".jsonl", ".lisp", ".md", ".props", ".ps1",
    ".py", ".sln", ".targets", ".txt", ".xml", ".yaml", ".yml"
)

function Get-TrackedFiles {
    $files = & git -C $script:ResolvedRepoRoot ls-files
    if ($LASTEXITCODE -ne 0) {
        throw "Unable to list tracked files. Run this script from inside a git repository."
    }

    return $files | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
}

function Get-RepoRoot {
    return $script:ResolvedRepoRoot
}

function Get-LocalCorpusPath {
    param(
        [string] $ExplicitPath
    )

    if (-not [string]::IsNullOrWhiteSpace($ExplicitPath)) {
        return $ExplicitPath
    }

    $localConfigPath = Join-Path (Get-RepoRoot) "OAN Mortalis V1.1.1\.local\private_corpus_root.txt"
    if (Test-Path -LiteralPath $localConfigPath -PathType Leaf) {
        return (Get-Content -LiteralPath $localConfigPath -TotalCount 1).Trim()
    }

    if (-not [string]::IsNullOrWhiteSpace($env:OAN_REFERENCE_CORPUS)) {
        return $env:OAN_REFERENCE_CORPUS
    }

    return $null
}

function Get-ManagedTrackedTextFiles {
    param(
        [string[]] $Files
    )

    foreach ($file in $Files) {
        if ($file -like "OAN Mortalis V0.1 Archive/*") {
            continue
        }

        $isManagedSurface = (
            $file -eq "README.md" -or
            $file -eq "AGENTS.md" -or
            $file -eq "SECURITY.md" -or
            $file -eq "SECURITY_HARDENING.md" -or
            $file -eq "global.json" -or
            $file -like ".github/*" -or
            $file -like "docs/*" -or
            $file -like "Build Contracts/*" -or
            $file -like "OAN Mortalis V1.1.1/*"
        )
        if (-not $isManagedSurface) {
            continue
        }

        $extension = [System.IO.Path]::GetExtension($file)
        if ($script:TextExtensions -notcontains $extension) {
            continue
        }

        if (Test-Path -LiteralPath $file -PathType Leaf) {
            $file
        }
    }
}

function Test-CorpusPathLeak {
    param(
        [Parameter(Mandatory = $true)]
        [string] $CorpusPath,

        [Parameter(Mandatory = $true)]
        [string[]] $Files
    )

    $leaks = New-Object System.Collections.Generic.List[string]

    foreach ($file in $Files) {
        if (-not (Test-Path -LiteralPath $file -PathType Leaf)) {
            continue
        }

        $match = Select-String -LiteralPath $file -SimpleMatch $CorpusPath -Quiet -ErrorAction SilentlyContinue
        if ($match) {
            $leaks.Add($file)
        }
    }

    return $leaks
}

function Test-ExternalAbsolutePathLeak {
    param(
        [Parameter(Mandatory = $true)]
        [string[]] $Files
    )

    $regex = [regex]'(?i)(?<![A-Za-z0-9])[A-Z]:\\[^`"''\r\n<>|]+'
    $leaks = New-Object System.Collections.Generic.List[string]

    foreach ($file in $Files) {
        $matches = Select-String -LiteralPath $file -Pattern $regex -AllMatches -ErrorAction SilentlyContinue
        foreach ($match in $matches) {
            foreach ($value in $match.Matches.Value) {
                try {
                    $fullPath = [System.IO.Path]::GetFullPath($value)
                }
                catch {
                    $fullPath = $value
                }

                $leaks.Add(("{0}: {1}" -f $file, $fullPath))
            }
        }
    }

    return $leaks
}

$trackedFiles = Get-TrackedFiles
$managedFiles = @(Get-ManagedTrackedTextFiles -Files $trackedFiles)
$rawCorpusPath = Get-LocalCorpusPath -ExplicitPath $CorpusPath

$corpusLeaks = @()
if (-not [string]::IsNullOrWhiteSpace($rawCorpusPath)) {
    $resolvedCorpusPath = [System.IO.Path]::GetFullPath($rawCorpusPath)
    $corpusLeaks = @(Test-CorpusPathLeak -CorpusPath $resolvedCorpusPath -Files $managedFiles)
}

$externalLeaks = @(Test-ExternalAbsolutePathLeak -Files $managedFiles)

if ($corpusLeaks.Count -gt 0 -or $externalLeaks.Count -gt 0) {
    Write-Host "FAIL: Detected out-of-scope path leakage in tracked managed files."
    if ($corpusLeaks.Count -gt 0) {
        Write-Host "Reference identifier: Lucid Research Corpus"
        Write-Host "Files containing leaked corpus path:"
        $corpusLeaks | Sort-Object -Unique | ForEach-Object { Write-Host " - $_" }
    }

    if ($externalLeaks.Count -gt 0) {
        Write-Host "Files containing external absolute paths:"
        $externalLeaks | Sort-Object -Unique | ForEach-Object { Write-Host " - $_" }
    }

    exit 1
}

if ([string]::IsNullOrWhiteSpace($rawCorpusPath)) {
    Write-Host "PASS: No local corpus path is configured for this shell or ignored local config."
}
else {
    Write-Host "PASS: No tracked managed files contain the private corpus path."
    Write-Host "Reference identifier: Lucid Research Corpus"
}

Write-Host "PASS: No tracked managed files in the V1.1.1 hardened surface contain external absolute paths."
exit 0
