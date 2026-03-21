param(
    [string] $RepoRoot,
    [string] $BaseRef,
    [string] $RequestedVersion,
    [string] $OutputPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-GitLines {
    param([string[]] $Arguments)

    $escapedArguments = $Arguments | ForEach-Object {
        if ($_ -match '[\s"]') {
            '"' + ($_ -replace '"', '\"') + '"'
        }
        else {
            $_
        }
    }

    $startInfo = New-Object System.Diagnostics.ProcessStartInfo
    $startInfo.FileName = 'git'
    $startInfo.Arguments = [string]::Join(' ', $escapedArguments)
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true

    $process = New-Object System.Diagnostics.Process
    $process.StartInfo = $startInfo
    [void] $process.Start()
    $stdout = $process.StandardOutput.ReadToEnd()
    $stderr = $process.StandardError.ReadToEnd()
    $process.WaitForExit()

    if ($process.ExitCode -ne 0) {
        return @()
    }

    return @(
        @($stdout, $stderr) |
            ForEach-Object { "$_".Trim() } |
            ForEach-Object { $_ -split "(`r`n|`n|`r)" } |
            Where-Object {
                -not [string]::IsNullOrWhiteSpace($_) -and
                -not $_.StartsWith('warning:', [System.StringComparison]::OrdinalIgnoreCase)
            }
    )
}

function Normalize-RepoPath {
    param([string] $Path)

    $normalized = $Path.Replace('\', '/')
    if ($normalized.StartsWith('OAN Mortalis V1.0/', [System.StringComparison]::Ordinal)) {
        return $normalized.Substring('OAN Mortalis V1.0/'.Length)
    }

    return $normalized
}

function ConvertTo-AssemblyVersion {
    param([string] $Version)

    $parts = $Version.Split('.')
    if ($parts.Count -ne 3) {
        throw "Version '$Version' is not in BuildVersion.ModularSet.PatchSet form."
    }

    return '{0}.{1}.{2}.0' -f $parts[0], $parts[1], $parts[2]
}

function Get-NextPatchVersion {
    param([string] $Version)

    $parts = $Version.Split('.')
    if ($parts.Count -ne 3) {
        throw "Version '$Version' is not in BuildVersion.ModularSet.PatchSet form."
    }

    $major = [int] $parts[0]
    $minor = [int] $parts[1]
    $patch = [int] $parts[2] + 1
    return '{0}.{1}.{2}' -f $major, $minor, $patch
}

function Get-ChangedFiles {
    param([string] $BaseRef)

    $untracked = Get-GitLines -Arguments @('ls-files', '--others', '--exclude-standard')

    if (-not [string]::IsNullOrWhiteSpace($BaseRef)) {
        $changed = Get-GitLines -Arguments @('diff', '--name-only', "$BaseRef..HEAD")
        return @($changed + $untracked | Sort-Object -Unique)
    }

    $worktreeChanged = Get-GitLines -Arguments @('diff', '--name-only', 'HEAD')
    if ($worktreeChanged.Count -gt 0 -or $untracked.Count -gt 0) {
        return @($worktreeChanged + $untracked | Sort-Object -Unique)
    }

    $previousHead = Get-GitLines -Arguments @('rev-parse', '--verify', 'HEAD~1')
    if ($previousHead.Count -gt 0) {
        return @(Get-GitLines -Arguments @('diff', '--name-only', 'HEAD~1', 'HEAD'))
    }

    return @()
}

function Write-JsonFile {
    param(
        [string] $Path,
        [object] $Value
    )

    $directory = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($directory)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }

    $Value | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $Path -Encoding utf8
}

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = Split-Path -Parent $PSScriptRoot
}

$activeBuildRoot = Join-Path $RepoRoot 'OAN Mortalis V1.0'
$policyPath = Join-Path $activeBuildRoot 'build\version-policy.json'
$familyMaturityPath = Join-Path $activeBuildRoot 'build\family-maturity.json'
$hitlGatesPath = Join-Path $activeBuildRoot 'build\hitl-gates.json'

$policy = Get-Content -Raw -LiteralPath $policyPath | ConvertFrom-Json
$familyMaturity = Get-Content -Raw -LiteralPath $familyMaturityPath | ConvertFrom-Json
$hitlGates = Get-Content -Raw -LiteralPath $hitlGatesPath | ConvertFrom-Json

$projectEntries = @($familyMaturity.projects | ForEach-Object {
    [pscustomobject]@{
        project = [string] $_.project
        path = [string] $_.path
        projectDirectory = ((Split-Path -Parent ([string] $_.path)).Replace('\', '/') + '/')
        family = [string] $_.family
        status = [string] $_.status
        buildable = [bool] $_.buildable
        deployable = [bool] $_.deployable
        operational = [bool] $_.operational
        promotable = [bool] $_.promotable
        authoritative = [bool] $_.authoritative
        excludedFromFirstPublish = [bool] $_.excludedFromFirstPublish
    }
})

$changedFiles = @(Get-ChangedFiles -BaseRef $BaseRef | ForEach-Object { Normalize-RepoPath -Path $_ })
$touchedProjects = New-Object System.Collections.Generic.List[object]
$touchedProjectNames = New-Object System.Collections.Generic.HashSet[string]([System.StringComparer]::Ordinal)
$unmatchedSourcePaths = New-Object System.Collections.Generic.List[string]

foreach ($changedFile in $changedFiles) {
    $matches = @($projectEntries | Where-Object { $changedFile.StartsWith($_.projectDirectory, [System.StringComparison]::Ordinal) })

    if ($matches.Count -eq 0) {
        if ($changedFile.StartsWith('src/', [System.StringComparison]::Ordinal) -or
            $changedFile.StartsWith('tests/', [System.StringComparison]::Ordinal)) {
            $unmatchedSourcePaths.Add($changedFile) | Out-Null
        }

        continue
    }

    foreach ($match in $matches) {
        if ($touchedProjectNames.Add($match.project)) {
            $touchedProjects.Add([ordered]@{
                project = $match.project
                path = $match.path
                family = $match.family
                status = $match.status
                buildable = $match.buildable
                deployable = $match.deployable
                operational = $match.operational
                promotable = $match.promotable
                authoritative = $match.authoritative
                excludedFromFirstPublish = $match.excludedFromFirstPublish
            }) | Out-Null
        }
    }
}

$touchedFamilies = @($touchedProjects | ForEach-Object { $_.family } | Sort-Object -Unique)
$reasonCodes = New-Object System.Collections.Generic.List[string]
$gatesTriggered = New-Object System.Collections.Generic.List[string]
$currentVersion = [string] $policy.currentVersion
$currentAssemblyVersion = [string] $policy.currentAssemblyVersion
$proposedVersion = $currentVersion
$proposedAssemblyVersion = $currentAssemblyVersion
$classification = 'no-change'
$requiresHitl = $false

if ($unmatchedSourcePaths.Count -gt 0) {
    $classification = 'hold'
    $requiresHitl = $true
    $reasonCodes.Add([string] $policy.reasonCodes.unknownProjectHold) | Out-Null
    $gatesTriggered.Add('unknown-project-touch') | Out-Null
}
elseif (-not [string]::IsNullOrWhiteSpace($RequestedVersion) -and
        $RequestedVersion -ne $currentVersion) {
    $requestedParts = $RequestedVersion.Split('.')
    $currentParts = $currentVersion.Split('.')

    if ($requestedParts.Count -ne 3) {
        throw "RequestedVersion must be in BuildVersion.ModularSet.PatchSet form."
    }

    $sameBuildLine = ($requestedParts[0] -eq $currentParts[0])
    $sameModularSet = ($requestedParts[1] -eq $currentParts[1])

    if ($sameBuildLine -and $sameModularSet) {
        $classification = 'patch-requested'
        $proposedVersion = $RequestedVersion
        $proposedAssemblyVersion = ConvertTo-AssemblyVersion -Version $RequestedVersion
        $reasonCodes.Add([string] $policy.reasonCodes.patchRequested) | Out-Null
    }
    else {
        $classification = 'hold'
        $requiresHitl = $true
        $reasonCodes.Add([string] $policy.reasonCodes.modularSetHold) | Out-Null
        $gatesTriggered.Add('modular-set-promotion') | Out-Null
    }
}
elseif (@($touchedProjects | Where-Object { $_.promotable }).Count -gt 0) {
    $classification = 'patch-auto'
    $proposedVersion = Get-NextPatchVersion -Version $currentVersion
    $proposedAssemblyVersion = ConvertTo-AssemblyVersion -Version $proposedVersion
    $reasonCodes.Add([string] $policy.reasonCodes.patchAuto) | Out-Null
}
else {
    $classification = 'no-change'
    $reasonCodes.Add([string] $policy.reasonCodes.noPromotableChange) | Out-Null
}

$changeMode = 'none'
if (-not [string]::IsNullOrWhiteSpace($BaseRef)) {
    $changeMode = 'base-ref'
}
elseif ($changedFiles.Count -gt 0) {
    $changeMode = 'worktree-or-last-commit'
}

$changedFilesArray = @($changedFiles | ForEach-Object { $_ })
$touchedProjectsArray = @($touchedProjects | ForEach-Object { $_ })
$touchedFamiliesArray = @($touchedFamilies | ForEach-Object { $_ })
$unmatchedSourcePathsArray = @($unmatchedSourcePaths | ForEach-Object { $_ })
$reasonCodesArray = @($reasonCodes | ForEach-Object { $_ })
$gatesTriggeredArray = @($gatesTriggered | ForEach-Object { $_ })
$declaredHitlGatesArray = @($hitlGates.gates | ForEach-Object { [string] $_.id })

$decision = @{}
$decision['schemaVersion'] = 1
$decision['generatedAtUtc'] = (Get-Date).ToUniversalTime().ToString('o')
$decision['policySource'] = 'OAN Mortalis V1.0/build/version-policy.json'
$decision['familyMaturitySource'] = 'OAN Mortalis V1.0/build/family-maturity.json'
$decision['hitlGateSource'] = 'OAN Mortalis V1.0/build/hitl-gates.json'
$decision['changeBasis'] = @{
    mode = $changeMode
    baseRef = $BaseRef
}
$decision['changedFiles'] = $changedFilesArray
$decision['touchedProjects'] = $touchedProjectsArray
$decision['touchedFamilies'] = $touchedFamiliesArray
$decision['unmatchedSourcePaths'] = $unmatchedSourcePathsArray
$decision['versionDecision'] = @{
    currentVersion = $currentVersion
    currentAssemblyVersion = $currentAssemblyVersion
    proposedVersion = $proposedVersion
    proposedAssemblyVersion = $proposedAssemblyVersion
    classification = $classification
    reasonCodes = $reasonCodesArray
    requiresHitl = $requiresHitl
}
$decision['gatesTriggered'] = $gatesTriggeredArray
$decision['declaredHitlGates'] = $declaredHitlGatesArray

if (-not [string]::IsNullOrWhiteSpace($OutputPath)) {
    Write-JsonFile -Path $OutputPath -Value $decision
}

$decision | ConvertTo-Json -Depth 12
