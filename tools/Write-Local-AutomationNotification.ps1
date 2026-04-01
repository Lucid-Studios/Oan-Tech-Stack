param(
    [string] $RepoRoot,
    [string] $CyclePolicyPath = 'OAN Mortalis V1.1.1/build/local-automation-cycle.json',
    [string] $PreviousStatus,
    [string] $CurrentStatus,
    [switch] $ForceNotification,
    [switch] $SuppressPopup
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

$automationCascadePromptHelperPath = Join-Path $PSScriptRoot 'Automation-CascadePrompt.ps1'
. $automationCascadePromptHelperPath

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

function Show-BestEffortWindowsPopup {
    param(
        [string] $Title,
        [string] $Message,
        [int] $TimeoutSeconds
    )

    try {
        $shell = New-Object -ComObject WScript.Shell
        [void] $shell.Popup($Message, $TimeoutSeconds, $Title, 0x40)
        return $true
    }
    catch {
        return $false
    }
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$cyclePolicy = Get-Content -Raw -LiteralPath $resolvedCyclePolicyPath | ConvertFrom-Json
$cycleStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.statePath)
$notificationOutputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.notificationOutputRoot)
$notificationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.notificationStatePath)
$seededGovernanceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.seededGovernanceStatePath)
$schedulerReconciliationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.schedulerReconciliationStatePath)
$cmeConsolidationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.cmeConsolidationStatePath)
$notificationPolicy = $cyclePolicy.notificationPolicy

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
$notificationState = Read-JsonFileOrNull -Path $notificationStatePath
$seededGovernanceState = Read-JsonFileOrNull -Path $seededGovernanceStatePath
$schedulerReconciliationState = Read-JsonFileOrNull -Path $schedulerReconciliationStatePath
$cmeConsolidationState = Read-JsonFileOrNull -Path $cmeConsolidationStatePath

if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before notification can run.'
}

if ([string]::IsNullOrWhiteSpace($CurrentStatus)) {
    $CurrentStatus = [string] $cycleState.lastKnownStatus
}

if ([string]::IsNullOrWhiteSpace($PreviousStatus)) {
    $PreviousStatus = if ($null -ne $notificationState) { [string] $notificationState.lastEvaluatedStatus } else { $null }
}

$digestBundlePath = [string] $cycleState.lastDigestBundle
$resolvedDigestBundlePath = if ([string]::IsNullOrWhiteSpace($digestBundlePath)) {
    $null
} else {
    Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $digestBundlePath
}

$digestJson = $null
if ($null -ne $resolvedDigestBundlePath) {
    $digestJsonPath = Join-Path $resolvedDigestBundlePath 'release-candidate-digest.json'
    $digestJson = Read-JsonFileOrNull -Path $digestJsonPath
}

$recommendedAction = if ($null -ne $digestJson) { [string] $digestJson.recommendedAction } else { $null }
$requiresImmediateHitl = if ($null -ne $digestJson) { [bool] $digestJson.requiresImmediateHitl } else { $false }
$notifyOnStatuses = @($notificationPolicy.notifyOnStatuses | ForEach-Object { [string] $_ })
$statusEligible = $notifyOnStatuses -contains $CurrentStatus
$transitioned = $PreviousStatus -ne $CurrentStatus
$onlyOnTransition = [bool] $notificationPolicy.onlyNotifyOnTransition
$shouldNotify = $ForceNotification.IsPresent
$triggerReason = if ($ForceNotification.IsPresent) { 'forced-notification' } else { 'no-notification' }

if (-not $shouldNotify -and [bool] $notificationPolicy.enabled) {
    if ($statusEligible) {
        if (-not $onlyOnTransition -or $transitioned -or $null -eq $notificationState) {
            $shouldNotify = $true
            $triggerReason = if ($transitioned) { 'status-transition' } else { 'status-alert' }
        }
    }
}

$popupAttempted = $false
$popupSucceeded = $false
$bundlePath = $null

if ($shouldNotify) {
    $timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
    $commitSha = [string] $cycleState.lastReleaseCandidateBundle
    $bundleId = if (-not [string]::IsNullOrWhiteSpace($commitSha)) {
        '{0}-{1}' -f $timestamp, ([System.IO.Path]::GetFileName($commitSha))
    } else {
        $timestamp
    }

    $bundlePath = Join-Path $notificationOutputRoot $bundleId
    $bundleJsonPath = Join-Path $bundlePath 'notification.json'
    $bundleMarkdownPath = Join-Path $bundlePath 'notification.md'

    $title = switch ($CurrentStatus) {
        'blocked' { 'OAN Automation Blocked' }
        'hitl-required' { 'OAN Automation Needs Review' }
        default { 'OAN Automation Notice' }
    }

    $messageLines = @(
        ('Status: {0}' -f $CurrentStatus),
        ('Recommended action: {0}' -f $(if ($recommendedAction) { $recommendedAction } else { 'review-state-surface' }))
    )

    if ($null -ne $seededGovernanceState) {
        $messageLines += ('Seeded governance: {0}' -f [string] $seededGovernanceState.disposition)
    }

    if ($null -ne $cmeConsolidationState) {
        $messageLines += ('CME consolidation: {0}' -f [string] $cmeConsolidationState.consolidationState)
    }

    if ($requiresImmediateHitl) {
        $messageLines += 'Immediate HITL is required.'
    }

    $message = $messageLines -join [Environment]::NewLine

    if ([bool] $notificationPolicy.bestEffortWindowsPopup -and -not $SuppressPopup.IsPresent) {
        $popupAttempted = $true
        $popupSucceeded = Show-BestEffortWindowsPopup -Title $title -Message $message -TimeoutSeconds ([int] $notificationPolicy.popupTimeoutSeconds)
    }

    $bundlePayload = [ordered]@{
        schemaVersion = 1
        generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
        previousStatus = $PreviousStatus
        currentStatus = $CurrentStatus
        recommendedAction = $recommendedAction
        requiresImmediateHitl = $requiresImmediateHitl
        triggerReason = $triggerReason
        seededGovernanceDisposition = if ($null -ne $seededGovernanceState) { [string] $seededGovernanceState.disposition } else { $null }
        schedulerAligned = if ($null -ne $schedulerReconciliationState) { [bool] $schedulerReconciliationState.aligned } else { $null }
        cmeConsolidationState = if ($null -ne $cmeConsolidationState) { [string] $cmeConsolidationState.consolidationState } else { $null }
        digestBundlePath = if ($null -ne $resolvedDigestBundlePath) { Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $resolvedDigestBundlePath } else { $null }
        taskStatusPath = '.audit/state/local-automation-tasking-status.md'
        popupAttempted = $popupAttempted
        popupSucceeded = $popupSucceeded
    }
    Add-AutomationCascadeOperatorPromptProperty -InputObject $bundlePayload | Out-Null

    Write-JsonFile -Path $bundleJsonPath -Value $bundlePayload

    $markdownLines = @(
        '# Local Automation Notification',
        '',
        ('- Generated at (UTC): `{0}`' -f $bundlePayload.generatedAtUtc),
        ('- Previous status: `{0}`' -f $(if ($bundlePayload.previousStatus) { $bundlePayload.previousStatus } else { 'none' })),
        ('- Current status: `{0}`' -f $bundlePayload.currentStatus),
        ('- Recommended action: `{0}`' -f $(if ($bundlePayload.recommendedAction) { $bundlePayload.recommendedAction } else { 'review-state-surface' })),
        ('- Immediate HITL required: `{0}`' -f $bundlePayload.requiresImmediateHitl),
        ('- Trigger reason: `{0}`' -f $bundlePayload.triggerReason),
        ('- Seeded governance disposition: `{0}`' -f $(if ($bundlePayload.seededGovernanceDisposition) { $bundlePayload.seededGovernanceDisposition } else { 'none' })),
        ('- Scheduler aligned: `{0}`' -f $(if ($null -ne $bundlePayload.schedulerAligned) { $bundlePayload.schedulerAligned } else { 'unknown' })),
        ('- CME consolidation state: `{0}`' -f $(if ($bundlePayload.cmeConsolidationState) { $bundlePayload.cmeConsolidationState } else { 'unknown' })),
        ('- Popup attempted: `{0}`' -f $bundlePayload.popupAttempted),
        ('- Popup succeeded: `{0}`' -f $bundlePayload.popupSucceeded)
    )

    $markdownLines = Add-AutomationCascadePromptMarkdownLines -MarkdownLines $markdownLines
    Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8
}

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    enabled = [bool] $notificationPolicy.enabled
    lastEvaluatedStatus = $CurrentStatus
    previousStatus = $PreviousStatus
    triggered = $shouldNotify
    triggerReason = $triggerReason
    lastNotificationBundle = if ($null -ne $bundlePath) {
        Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    } elseif ($null -ne $notificationState) {
        [string] $notificationState.lastNotificationBundle
    } else {
        $null
    }
    popupAttempted = $popupAttempted
    popupSucceeded = $popupSucceeded
}
Add-AutomationCascadeOperatorPromptProperty -InputObject $statePayload | Out-Null

Write-JsonFile -Path $notificationStatePath -Value $statePayload
Write-Host ('[local-automation-notification] State: {0}' -f $notificationStatePath)
$notificationStatePath
