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
$runtimeDeployabilityEnvelopeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runtimeDeployabilityEnvelopeStatePath)
$sanctuaryRuntimeReadinessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.sanctuaryRuntimeReadinessStatePath)
$runtimeWorkSurfaceAdmissibilityStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runtimeWorkSurfaceAdmissibilityStatePath)
$bondedParticipationLocalityLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.bondedParticipationLocalityLedgerStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.sanctuaryRuntimeWorkbenchSurfaceOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.sanctuaryRuntimeWorkbenchSurfaceStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the Sanctuary runtime workbench writer can run.'
}

$runtimeDeployabilityEnvelopeState = Read-JsonFileOrNull -Path $runtimeDeployabilityEnvelopeStatePath
$sanctuaryRuntimeReadinessState = Read-JsonFileOrNull -Path $sanctuaryRuntimeReadinessStatePath
$runtimeWorkSurfaceAdmissibilityState = Read-JsonFileOrNull -Path $runtimeWorkSurfaceAdmissibilityStatePath
$bondedParticipationLocalityLedgerState = Read-JsonFileOrNull -Path $bondedParticipationLocalityLedgerStatePath

$deployabilityState = [string] (Get-ObjectPropertyValueOrNull -InputObject $runtimeDeployabilityEnvelopeState -PropertyName 'envelopeState')
$runtimeReadinessState = [string] (Get-ObjectPropertyValueOrNull -InputObject $sanctuaryRuntimeReadinessState -PropertyName 'readinessState')
$runtimeWorkState = [string] (Get-ObjectPropertyValueOrNull -InputObject $runtimeWorkSurfaceAdmissibilityState -PropertyName 'admissibilityState')
$bondedLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $bondedParticipationLocalityLedgerState -PropertyName 'ledgerState')

$sourceFiles = @(
    (Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath 'OAN Mortalis V1.0/src/Oan.Common/SanctuaryWorkbenchContracts.cs'),
    (Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath 'OAN Mortalis V1.0/src/Oan.Common/SanctuaryWorkbenchKeys.cs'),
    (Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath 'OAN Mortalis V1.0/src/CradleTek.Runtime/SanctuaryRuntimeWorkbenchService.cs')
)
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$keysSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$workbenchProjectionBound = $contractsSource.IndexOf('CreateRuntimeWorkbenchSurface', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('sanctuary-runtime-workbench-surface-bound', [System.StringComparison]::Ordinal) -ge 0
$workbenchKeyBound = $keysSource.IndexOf('CreateSanctuaryRuntimeWorkbenchHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateRuntimeWorkbenchSurface', [System.StringComparison]::Ordinal) -ge 0

$workbenchState = 'awaiting-runtime-deployability'
$reasonCode = 'sanctuary-runtime-workbench-awaiting-runtime-deployability'
$nextAction = 'derive-runtime-deployability-envelope'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $workbenchState = 'blocked'
    $reasonCode = 'sanctuary-runtime-workbench-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($deployabilityState -ne 'deployable-candidate-ready') {
    $workbenchState = 'awaiting-runtime-deployability'
    $reasonCode = 'sanctuary-runtime-workbench-deployability-not-ready'
    $nextAction = if ($null -ne $runtimeDeployabilityEnvelopeState) { [string] $runtimeDeployabilityEnvelopeState.nextAction } else { 'derive-runtime-deployability-envelope' }
} elseif ($runtimeReadinessState -ne 'bounded-working-state-ready') {
    $workbenchState = 'awaiting-sanctuary-runtime-readiness'
    $reasonCode = 'sanctuary-runtime-workbench-readiness-not-ready'
    $nextAction = if ($null -ne $sanctuaryRuntimeReadinessState) { [string] $sanctuaryRuntimeReadinessState.nextAction } else { 'derive-sanctuary-runtime-readiness' }
} elseif ($runtimeWorkState -ne 'provisional-runtime-work') {
    $workbenchState = 'awaiting-runtime-work-admissibility'
    $reasonCode = 'sanctuary-runtime-workbench-work-surface-not-ready'
    $nextAction = if ($null -ne $runtimeWorkSurfaceAdmissibilityState) { [string] $runtimeWorkSurfaceAdmissibilityState.nextAction } else { 'derive-runtime-work-surface-admissibility' }
} elseif ($bondedLedgerState -ne 'bonded-locality-ledger-ready') {
    $workbenchState = 'awaiting-bonded-locality-ledger'
    $reasonCode = 'sanctuary-runtime-workbench-bonded-ledger-not-ready'
    $nextAction = if ($null -ne $bondedParticipationLocalityLedgerState) { [string] $bondedParticipationLocalityLedgerState.nextAction } else { 'emit-bonded-participation-locality-ledger' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $workbenchProjectionBound -or -not $workbenchKeyBound -or -not $serviceBindingBound) {
    $workbenchState = 'awaiting-runtime-workbench-binding'
    $reasonCode = 'sanctuary-runtime-workbench-source-missing'
    $nextAction = 'bind-sanctuary-runtime-workbench-surface'
} else {
    $workbenchState = 'sanctuary-runtime-workbench-ready'
    $reasonCode = 'sanctuary-runtime-workbench-surface-bound'
    $nextAction = 'emit-amenable-day-dream-tier-admissibility'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'sanctuary-runtime-workbench-surface.json'
$bundleMarkdownPath = Join-Path $bundlePath 'sanctuary-runtime-workbench-surface.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    workbenchState = $workbenchState
    reasonCode = $reasonCode
    nextAction = $nextAction
    runtimeDeployabilityState = $deployabilityState
    sanctuaryRuntimeReadinessState = $runtimeReadinessState
    runtimeWorkAdmissibilityState = $runtimeWorkState
    bondedParticipationLocalityLedgerState = $bondedLedgerState
    sessionPosture = 'bounded-workbench-ready'
    boundedWorkClass = 'bounded-local-candidate-sanctuary-workbench'
    bondedOperatorLaneWithheld = $true
    mosBearingReleaseDenied = $true
    workbenchProjectionBound = $workbenchProjectionBound
    workbenchKeyBound = $workbenchKeyBound
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
    '# Sanctuary Runtime Workbench Surface',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Workbench state: `{0}`' -f $payload.workbenchState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Runtime deployability state: `{0}`' -f $(if ($payload.runtimeDeployabilityState) { $payload.runtimeDeployabilityState } else { 'missing' })),
    ('- Sanctuary runtime readiness state: `{0}`' -f $(if ($payload.sanctuaryRuntimeReadinessState) { $payload.sanctuaryRuntimeReadinessState } else { 'missing' })),
    ('- Runtime work admissibility state: `{0}`' -f $(if ($payload.runtimeWorkAdmissibilityState) { $payload.runtimeWorkAdmissibilityState } else { 'missing' })),
    ('- Bonded participation locality-ledger state: `{0}`' -f $(if ($payload.bondedParticipationLocalityLedgerState) { $payload.bondedParticipationLocalityLedgerState } else { 'missing' })),
    ('- Session posture: `{0}`' -f $payload.sessionPosture),
    ('- Bounded work class: `{0}`' -f $payload.boundedWorkClass),
    ('- Bonded operator lane withheld: `{0}`' -f [bool] $payload.bondedOperatorLaneWithheld),
    ('- MoS-bearing release denied: `{0}`' -f [bool] $payload.mosBearingReleaseDenied),
    ('- Workbench projection bound: `{0}`' -f [bool] $payload.workbenchProjectionBound),
    ('- Workbench key bound: `{0}`' -f [bool] $payload.workbenchKeyBound),
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
    workbenchState = $payload.workbenchState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    runtimeDeployabilityState = $payload.runtimeDeployabilityState
    sanctuaryRuntimeReadinessState = $payload.sanctuaryRuntimeReadinessState
    runtimeWorkAdmissibilityState = $payload.runtimeWorkAdmissibilityState
    bondedParticipationLocalityLedgerState = $payload.bondedParticipationLocalityLedgerState
    sessionPosture = $payload.sessionPosture
    boundedWorkClass = $payload.boundedWorkClass
    workbenchProjectionBound = $payload.workbenchProjectionBound
    serviceBindingBound = $payload.serviceBindingBound
    sourceFileCount = $payload.sourceFileCount
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[sanctuary-runtime-workbench-surface] Bundle: {0}' -f $bundlePath)
$bundlePath
