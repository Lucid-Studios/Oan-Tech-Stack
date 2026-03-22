param(
    [string] $RepoRoot,
    [string] $CyclePolicyPath = 'OAN Mortalis V1.0/build/local-automation-cycle.json'
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
    param([string] $BasePath, [string] $CandidatePath)

    if ([System.IO.Path]::IsPathRooted($CandidatePath)) {
        return [System.IO.Path]::GetFullPath($CandidatePath)
    }

    return [System.IO.Path]::GetFullPath((Join-Path $BasePath $CandidatePath))
}

function Get-RelativePathString {
    param([string] $BasePath, [string] $TargetPath)

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
    param([string] $Path, [object] $Value)

    $directory = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($directory)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }

    $Value | ConvertTo-Json -Depth 16 | Set-Content -LiteralPath $Path -Encoding utf8
}

function Get-ObjectPropertyValueOrNull {
    param([object] $InputObject, [string] $PropertyName)

    if ($null -eq $InputObject) {
        return $null
    }

    $property = $InputObject.PSObject.Properties[$PropertyName]
    if ($null -eq $property) {
        return $null
    }

    return $property.Value
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$cyclePolicy = Get-Content -Raw -LiteralPath $resolvedCyclePolicyPath | ConvertFrom-Json

$cycleStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.statePath)
$sanctuaryRuntimeWorkbenchSurfaceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.sanctuaryRuntimeWorkbenchSurfaceStatePath)
$amenableDayDreamTierAdmissibilityStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.amenableDayDreamTierAdmissibilityStatePath)
$selfRootedCrypticDepthGateStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.selfRootedCrypticDepthGateStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runtimeWorkbenchSessionLedgerOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runtimeWorkbenchSessionLedgerStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the runtime workbench session-ledger writer can run.'
}

$sanctuaryRuntimeWorkbenchSurfaceState = Read-JsonFileOrNull -Path $sanctuaryRuntimeWorkbenchSurfaceStatePath
$amenableDayDreamTierAdmissibilityState = Read-JsonFileOrNull -Path $amenableDayDreamTierAdmissibilityStatePath
$selfRootedCrypticDepthGateState = Read-JsonFileOrNull -Path $selfRootedCrypticDepthGateStatePath

$workbenchState = [string] (Get-ObjectPropertyValueOrNull -InputObject $sanctuaryRuntimeWorkbenchSurfaceState -PropertyName 'workbenchState')
$dayDreamTierState = [string] (Get-ObjectPropertyValueOrNull -InputObject $amenableDayDreamTierAdmissibilityState -PropertyName 'tierState')
$depthGateState = [string] (Get-ObjectPropertyValueOrNull -InputObject $selfRootedCrypticDepthGateState -PropertyName 'gateState')

$sourceFiles = @(
    (Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath 'OAN Mortalis V1.0/src/Oan.Common/SanctuaryWorkbenchContracts.cs'),
    (Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath 'OAN Mortalis V1.0/src/Oan.Common/SanctuaryWorkbenchKeys.cs'),
    (Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath 'OAN Mortalis V1.0/src/CradleTek.Runtime/SanctuaryRuntimeWorkbenchService.cs')
)
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$keysSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$sessionProjectionBound = $contractsSource.IndexOf('CreateRuntimeWorkbenchSessionLedger', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateSessionEvent', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateBoundaryCondition', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('runtime-workbench-session-ledger-bound', [System.StringComparison]::Ordinal) -ge 0
$sessionKeyBound = $keysSource.IndexOf('CreateRuntimeWorkbenchSessionLedgerHandle', [System.StringComparison]::Ordinal) -ge 0 -and
    $keysSource.IndexOf('CreateWorkbenchSessionEventHandle', [System.StringComparison]::Ordinal) -ge 0 -and
    $keysSource.IndexOf('CreateBoundaryConditionHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateRuntimeWorkbenchSessionLedger', [System.StringComparison]::Ordinal) -ge 0

$sessionLedgerState = 'awaiting-runtime-workbench-surface'
$reasonCode = 'runtime-workbench-session-ledger-awaiting-runtime-workbench-surface'
$nextAction = 'emit-sanctuary-runtime-workbench-surface'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $sessionLedgerState = 'blocked'
    $reasonCode = 'runtime-workbench-session-ledger-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($workbenchState -ne 'sanctuary-runtime-workbench-ready') {
    $sessionLedgerState = 'awaiting-runtime-workbench-surface'
    $reasonCode = 'runtime-workbench-session-ledger-workbench-not-ready'
    $nextAction = if ($null -ne $sanctuaryRuntimeWorkbenchSurfaceState) { [string] $sanctuaryRuntimeWorkbenchSurfaceState.nextAction } else { 'emit-sanctuary-runtime-workbench-surface' }
} elseif ($dayDreamTierState -ne 'amenable-day-dream-tier-ready') {
    $sessionLedgerState = 'awaiting-day-dream-tier'
    $reasonCode = 'runtime-workbench-session-ledger-day-dream-not-ready'
    $nextAction = if ($null -ne $amenableDayDreamTierAdmissibilityState) { [string] $amenableDayDreamTierAdmissibilityState.nextAction } else { 'emit-amenable-day-dream-tier-admissibility' }
} elseif ($depthGateState -ne 'self-rooted-cryptic-depth-gate-ready') {
    $sessionLedgerState = 'awaiting-depth-gate'
    $reasonCode = 'runtime-workbench-session-ledger-depth-gate-not-ready'
    $nextAction = if ($null -ne $selfRootedCrypticDepthGateState) { [string] $selfRootedCrypticDepthGateState.nextAction } else { 'emit-self-rooted-cryptic-depth-gate' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $sessionProjectionBound -or -not $sessionKeyBound -or -not $serviceBindingBound) {
    $sessionLedgerState = 'awaiting-session-ledger-binding'
    $reasonCode = 'runtime-workbench-session-ledger-source-missing'
    $nextAction = 'bind-runtime-workbench-session-ledger'
} else {
    $sessionLedgerState = 'runtime-workbench-session-ledger-ready'
    $reasonCode = 'runtime-workbench-session-ledger-bound'
    $nextAction = 'emit-day-dream-collapse-receipt'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'runtime-workbench-session-ledger.json'
$bundleMarkdownPath = Join-Path $bundlePath 'runtime-workbench-session-ledger.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    sessionLedgerState = $sessionLedgerState
    reasonCode = $reasonCode
    nextAction = $nextAction
    workbenchState = $workbenchState
    dayDreamTierState = $dayDreamTierState
    depthGateState = $depthGateState
    sessionState = 'bounded-session-open'
    sessionPosture = 'bounded-session-open'
    returnPosture = 'return-through-bounded-workbench'
    admittedLaneCount = 3
    withheldLaneCount = 3
    sessionEventCount = 3
    boundaryConditionCount = 1
    sessionProjectionBound = $sessionProjectionBound
    sessionKeyBound = $sessionKeyBound
    serviceBindingBound = $serviceBindingBound
    sourceFileCount = @($sourceFiles).Count
    missingSourceFileCount = @($missingSourceFiles).Count
    sourceFiles = @(
        foreach ($file in $sourceFiles) {
            [ordered]@{
                path = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $file
                present = (Test-Path -LiteralPath $file -PathType Leaf)
            }
        }
    )
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Runtime Workbench Session Ledger',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Session-ledger state: `{0}`' -f $payload.sessionLedgerState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Workbench state: `{0}`' -f $(if ($payload.workbenchState) { $payload.workbenchState } else { 'missing' })),
    ('- Day-dream tier state: `{0}`' -f $(if ($payload.dayDreamTierState) { $payload.dayDreamTierState } else { 'missing' })),
    ('- Depth-gate state: `{0}`' -f $(if ($payload.depthGateState) { $payload.depthGateState } else { 'missing' })),
    ('- Session state: `{0}`' -f $payload.sessionState),
    ('- Session posture: `{0}`' -f $payload.sessionPosture),
    ('- Return posture: `{0}`' -f $payload.returnPosture),
    ('- Admitted lane count: `{0}`' -f $payload.admittedLaneCount),
    ('- Withheld lane count: `{0}`' -f $payload.withheldLaneCount),
    ('- Session event count: `{0}`' -f $payload.sessionEventCount),
    ('- Boundary-condition count: `{0}`' -f $payload.boundaryConditionCount),
    ('- Session projection bound: `{0}`' -f [bool] $payload.sessionProjectionBound),
    ('- Session key bound: `{0}`' -f [bool] $payload.sessionKeyBound),
    ('- Service binding bound: `{0}`' -f [bool] $payload.serviceBindingBound),
    ('- Source files present: `{0}/{1}`' -f ($payload.sourceFileCount - $payload.missingSourceFileCount), $payload.sourceFileCount),
    ''
)

foreach ($file in @($payload.sourceFiles)) {
    $markdownLines += @(
        ('## {0}' -f [string] $file.path),
        ('- Present: `{0}`' -f [bool] $file.present),
        ''
    )
}

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    sessionLedgerState = $payload.sessionLedgerState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    workbenchState = $payload.workbenchState
    dayDreamTierState = $payload.dayDreamTierState
    depthGateState = $payload.depthGateState
    sessionState = $payload.sessionState
    sessionPosture = $payload.sessionPosture
    returnPosture = $payload.returnPosture
    admittedLaneCount = $payload.admittedLaneCount
    withheldLaneCount = $payload.withheldLaneCount
    sessionEventCount = $payload.sessionEventCount
    boundaryConditionCount = $payload.boundaryConditionCount
    sessionProjectionBound = $payload.sessionProjectionBound
    serviceBindingBound = $payload.serviceBindingBound
    sourceFileCount = $payload.sourceFileCount
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[runtime-workbench-session-ledger] Bundle: {0}' -f $bundlePath)
$bundlePath
