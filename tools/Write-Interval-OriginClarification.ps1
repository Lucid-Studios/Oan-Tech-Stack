param(
    [string] $RepoRoot,
    [string] $CyclePolicyPath = 'OAN Mortalis V1.1.1/Automation/local-automation-cycle.json'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    if (-not [string]::IsNullOrWhiteSpace($PSScriptRoot)) {
        $RepoRoot = Split-Path -Parent $PSScriptRoot
    } else {
        $RepoRoot = (Get-Location).Path
    }
}

function Resolve-PathFromRepo {
    param(
        [string] $BasePath,
        [string] $CandidatePath
    )

    if ([System.IO.Path]::IsPathRooted($CandidatePath)) {
        return [System.IO.Path]::GetFullPath($CandidatePath)
    }

    return [System.IO.Path]::GetFullPath((Join-Path $BasePath $CandidatePath))
}

function Get-RelativePathString {
    param(
        [string] $BasePath,
        [string] $TargetPath
    )

    $resolvedBase = [System.IO.Path]::GetFullPath($BasePath)
    if (Test-Path -LiteralPath $resolvedBase -PathType Leaf) {
        $resolvedBase = Split-Path -Parent $resolvedBase
    }

    $resolvedTarget = [System.IO.Path]::GetFullPath($TargetPath)
    $baseUri = New-Object System.Uri(($resolvedBase.TrimEnd('\') + '\'))
    $targetUri = New-Object System.Uri($resolvedTarget)
    return [System.Uri]::UnescapeDataString($baseUri.MakeRelativeUri($targetUri).ToString()).Replace('\', '/')
}

function Read-JsonFileOrNull {
    param([string] $Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return $null
    }

    return Get-Content -Raw -LiteralPath $Path | ConvertFrom-Json
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

    $Value | ConvertTo-Json -Depth 14 | Set-Content -LiteralPath $Path -Encoding utf8
}

function Get-OptionalDateTimeUtc {
    param([object] $Value)

    if ($null -eq $Value) {
        return $null
    }

    $stringValue = [string] $Value
    if ([string]::IsNullOrWhiteSpace($stringValue)) {
        return $null
    }

    return [datetime]::Parse($stringValue, [System.Globalization.CultureInfo]::InvariantCulture, [System.Globalization.DateTimeStyles]::RoundtripKind).ToUniversalTime()
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$cyclePolicy = Get-Content -Raw -LiteralPath $resolvedCyclePolicyPath | ConvertFrom-Json

$cycleStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.statePath)
$schedulerExecutionReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.schedulerExecutionReceiptStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.intervalOriginClarificationOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.intervalOriginClarificationStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
$schedulerExecutionReceiptState = Read-JsonFileOrNull -Path $schedulerExecutionReceiptStatePath

$originState = 'candidate-only'
$reasonCode = 'interval-origin-clarification-candidate-only'
$nextAction = 'continue-candidate-automation'
$minutesBetweenRuns = $null

$lastReleaseCandidateRunUtc = Get-OptionalDateTimeUtc -Value $(if ($null -ne $cycleState) { $cycleState.lastReleaseCandidateRunUtc } else { $null })
$lastScheduledRunUtc = Get-OptionalDateTimeUtc -Value $(if ($null -ne $schedulerExecutionReceiptState) { $schedulerExecutionReceiptState.lastScheduledRunUtc } else { $null })

if ($null -eq $cycleState -or $null -eq $schedulerExecutionReceiptState) {
    $originState = 'awaiting-evidence'
    $reasonCode = 'interval-origin-clarification-evidence-missing'
    $nextAction = 'complete-interval-origin-prerequisites'
} elseif ($null -eq $lastScheduledRunUtc) {
    $originState = 'manual-continuity-only'
    $reasonCode = 'interval-origin-clarification-manual-only'
    $nextAction = 'allow-scheduled-cycle-to-fire'
} elseif ($null -eq $lastReleaseCandidateRunUtc) {
    $originState = 'scheduler-observed-without-release-anchor'
    $reasonCode = 'interval-origin-clarification-release-anchor-missing'
    $nextAction = 'capture-next-release-candidate-anchor'
} else {
    $minutesBetweenRuns = [math]::Abs([math]::Round(($lastReleaseCandidateRunUtc - $lastScheduledRunUtc).TotalMinutes, 2))
    if ($minutesBetweenRuns -le 10) {
        $originState = 'scheduler-observed-continuity'
        $reasonCode = 'interval-origin-clarification-scheduler-observed'
        $nextAction = 'continue-with-observed-cadence'
    } else {
        $originState = 'mixed-origin-unreconciled'
        $reasonCode = 'interval-origin-clarification-manual-overhang'
        $nextAction = 'reconcile-manual-overhang'
    }
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundlePath = Join-Path $outputRoot $timestamp
$bundleJsonPath = Join-Path $bundlePath 'interval-origin-clarification.json'
$bundleMarkdownPath = Join-Path $bundlePath 'interval-origin-clarification.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    originState = $originState
    reasonCode = $reasonCode
    nextAction = $nextAction
    lastReleaseCandidateRunUtc = if ($null -ne $lastReleaseCandidateRunUtc) { $lastReleaseCandidateRunUtc.ToString('o') } else { $null }
    lastScheduledRunUtc = if ($null -ne $lastScheduledRunUtc) { $lastScheduledRunUtc.ToString('o') } else { $null }
    minutesBetweenRuns = $minutesBetweenRuns
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Interval Origin Clarification',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Origin state: `{0}`' -f $payload.originState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Last release-candidate run (UTC): `{0}`' -f $(if ($payload.lastReleaseCandidateRunUtc) { $payload.lastReleaseCandidateRunUtc } else { 'missing' })),
    ('- Last scheduled run (UTC): `{0}`' -f $(if ($payload.lastScheduledRunUtc) { $payload.lastScheduledRunUtc } else { 'missing' })),
    ('- Minutes between runs: `{0}`' -f $(if ($null -ne $payload.minutesBetweenRuns) { $payload.minutesBetweenRuns } else { 'unknown' }))
)

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    originState = $payload.originState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[interval-origin-clarification] Bundle: {0}' -f $bundlePath)
$bundlePath
