param(
    [string] $RepoRoot,
    [string] $CyclePolicyPath = 'OAN Mortalis V1.1.1/build/local-automation-cycle.json'
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

    $Value | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $Path -Encoding utf8
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
$notificationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.notificationStatePath)
$dormantWindowLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.dormantWindowLedgerStatePath)
$silentCadenceIntegrityOutputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.silentCadenceIntegrityOutputRoot)
$silentCadenceIntegrityStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.silentCadenceIntegrityStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before silent cadence integrity can run.'
}

$notificationState = Read-JsonFileOrNull -Path $notificationStatePath
$dormantWindowLedgerState = Read-JsonFileOrNull -Path $dormantWindowLedgerStatePath
$nowUtc = (Get-Date).ToUniversalTime()
$nextMandatoryHitlReviewUtc = Get-OptionalDateTimeUtc -Value $cycleState.nextMandatoryHitlReviewUtc

$integrityState = 'candidate-only'
$reasonCode = 'silent-cadence-integrity-candidate-only'
$nextAction = 'continue-candidate-automation'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $integrityState = 'blocked'
    $reasonCode = 'silent-cadence-integrity-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($null -eq $notificationState -or $null -eq $dormantWindowLedgerState) {
    $integrityState = 'awaiting-evidence'
    $reasonCode = 'silent-cadence-integrity-evidence-missing'
    $nextAction = 'complete-map-11-prerequisites'
} elseif ([string] $cycleState.lastKnownStatus -eq 'candidate-ready' -and [bool] $notificationState.triggered) {
    $integrityState = 'noise-detected'
    $reasonCode = 'silent-cadence-integrity-candidate-ready-notified'
    $nextAction = 'inspect-notification-transition'
} elseif ($null -ne $nextMandatoryHitlReviewUtc -and $nextMandatoryHitlReviewUtc -le $nowUtc -and [string] $cycleState.lastKnownStatus -eq 'candidate-ready') {
    $integrityState = 'review-overdue'
    $reasonCode = 'silent-cadence-integrity-daily-review-overdue'
    $nextAction = 'emit-digest-and-surface-hitl-window'
} elseif ([string] $cycleState.lastKnownStatus -eq 'candidate-ready') {
    $integrityState = 'silent-and-stable'
    $reasonCode = 'silent-cadence-integrity-stable'
    $nextAction = 'continue-unattended-until-review-edge'
} elseif ([string] $cycleState.lastKnownStatus -eq 'hitl-required') {
    $integrityState = 'aligned-with-review-posture'
    $reasonCode = 'silent-cadence-integrity-hitl-posture'
    $nextAction = 'surface-next-review-bundle'
} else {
    $integrityState = 'aligned-with-posture'
    $reasonCode = 'silent-cadence-integrity-aligned'
    $nextAction = 'continue-governed-automation'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$commitKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $silentCadenceIntegrityOutputRoot ('{0}-{1}' -f $timestamp, $commitKey)
$bundleJsonPath = Join-Path $bundlePath 'silent-cadence-integrity.json'
$bundleMarkdownPath = Join-Path $bundlePath 'silent-cadence-integrity.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $nowUtc.ToString('o')
    integrityState = $integrityState
    reasonCode = $reasonCode
    nextAction = $nextAction
    currentPosture = [string] $cycleState.lastKnownStatus
    notificationTriggered = if ($null -ne $notificationState) { [bool] $notificationState.triggered } else { $null }
    notificationReason = if ($null -ne $notificationState) { [string] $notificationState.triggerReason } else { $null }
    dormantWindowLedgerState = if ($null -ne $dormantWindowLedgerState) { [string] $dormantWindowLedgerState.ledgerState } else { $null }
    consecutiveDormantWindows = if ($null -ne $dormantWindowLedgerState) { [int] $dormantWindowLedgerState.consecutiveDormantWindows } else { 0 }
    nextMandatoryHitlReviewUtc = if ($null -ne $nextMandatoryHitlReviewUtc) { $nextMandatoryHitlReviewUtc.ToString('o') } else { $null }
    sourceCandidateBundle = [string] $cycleState.lastReleaseCandidateBundle
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Silent Cadence Integrity',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Integrity state: `{0}`' -f $payload.integrityState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Current posture: `{0}`' -f $payload.currentPosture),
    ('- Notification triggered: `{0}`' -f $payload.notificationTriggered),
    ('- Notification reason: `{0}`' -f $(if ($payload.notificationReason) { $payload.notificationReason } else { 'none' })),
    ('- Dormant window ledger state: `{0}`' -f $(if ($payload.dormantWindowLedgerState) { $payload.dormantWindowLedgerState } else { 'missing' })),
    ('- Consecutive dormant windows: `{0}`' -f $payload.consecutiveDormantWindows),
    ('- Next mandatory HITL review (UTC): `{0}`' -f $(if ($payload.nextMandatoryHitlReviewUtc) { $payload.nextMandatoryHitlReviewUtc } else { 'missing' })),
    ('- Source candidate bundle: `{0}`' -f $(if ($payload.sourceCandidateBundle) { $payload.sourceCandidateBundle } else { 'missing' }))
)

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    integrityState = $payload.integrityState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    consecutiveDormantWindows = $payload.consecutiveDormantWindows
}

Write-JsonFile -Path $silentCadenceIntegrityStatePath -Value $statePayload
Write-Host ('[silent-cadence-integrity] Bundle: {0}' -f $bundlePath)
$bundlePath
