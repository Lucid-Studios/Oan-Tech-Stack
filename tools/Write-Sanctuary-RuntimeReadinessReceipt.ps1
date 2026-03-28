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

function Get-ObjectPropertyValueOrNull {
    param(
        [object] $InputObject,
        [string] $PropertyName
    )

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
$runtimeDeployabilityStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runtimeDeployabilityEnvelopeStatePath)
$cmeFormationAndOfficeLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.cmeFormationAndOfficeLedgerStatePath)
$seededGovernanceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.seededGovernanceStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.sanctuaryRuntimeReadinessOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.sanctuaryRuntimeReadinessStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before sanctuary runtime readiness can run.'
}

$runtimeDeployabilityState = Read-JsonFileOrNull -Path $runtimeDeployabilityStatePath
$cmeFormationAndOfficeLedgerState = Read-JsonFileOrNull -Path $cmeFormationAndOfficeLedgerStatePath
$seededGovernanceState = Read-JsonFileOrNull -Path $seededGovernanceStatePath

$readinessState = 'candidate-only'
$reasonCode = 'sanctuary-runtime-readiness-candidate-only'
$nextAction = 'continue-candidate-automation'
$workingStateClass = 'bounded-local-candidate-sanctuary'

$runtimeEnvelopeState = [string] (Get-ObjectPropertyValueOrNull -InputObject $runtimeDeployabilityState -PropertyName 'envelopeState')
$cmeLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $cmeFormationAndOfficeLedgerState -PropertyName 'ledgerState')
$cmeOfficeLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $cmeFormationAndOfficeLedgerState -PropertyName 'officeLedgerState')
$seedDisposition = [string] (Get-ObjectPropertyValueOrNull -InputObject $seededGovernanceState -PropertyName 'disposition')
$seedReadyState = [string] (Get-ObjectPropertyValueOrNull -InputObject $seededGovernanceState -PropertyName 'readyState')

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $readinessState = 'blocked'
    $reasonCode = 'sanctuary-runtime-readiness-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($null -eq $runtimeDeployabilityState) {
    $readinessState = 'awaiting-runtime-envelope'
    $reasonCode = 'sanctuary-runtime-readiness-envelope-missing'
    $nextAction = 'emit-runtime-deployability-envelope'
} elseif ($runtimeEnvelopeState -ne 'deployable-candidate-ready') {
    $readinessState = 'awaiting-runtime-envelope'
    $reasonCode = 'sanctuary-runtime-readiness-envelope-not-ready'
    $nextAction = [string] (Get-ObjectPropertyValueOrNull -InputObject $runtimeDeployabilityState -PropertyName 'nextAction')
} elseif ($null -eq $cmeFormationAndOfficeLedgerState -or $null -eq $seededGovernanceState) {
    $readinessState = 'awaiting-governed-readiness'
    $reasonCode = 'sanctuary-runtime-readiness-governance-evidence-missing'
    $nextAction = 'emit-governed-readiness-surfaces'
} elseif ($seedDisposition -ne 'Accepted' -or $seedReadyState -ne 'ready') {
    $readinessState = 'awaiting-governed-readiness'
    $reasonCode = 'sanctuary-runtime-readiness-seed-not-ready'
    $nextAction = 'bring-seeded-governance-to-ready-state'
} elseif ($cmeLedgerState -in @('Provisional', 'Open', 'Active') -and $cmeOfficeLedgerState -in @('Provisional', 'Open')) {
    $readinessState = 'bounded-working-state-ready'
    $reasonCode = 'sanctuary-runtime-readiness-provisional-office-ready'
    $nextAction = 'continue-bounded-sanctuary-runtime-work'
} else {
    $readinessState = 'awaiting-governed-readiness'
    $reasonCode = 'sanctuary-runtime-readiness-office-not-open'
    $nextAction = 'continue-office-formation-under-law'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'sanctuary-runtime-readiness-receipt.json'
$bundleMarkdownPath = Join-Path $bundlePath 'sanctuary-runtime-readiness-receipt.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    readinessState = $readinessState
    reasonCode = $reasonCode
    nextAction = $nextAction
    workingStateClass = $workingStateClass
    runtimeDeployabilityEnvelopeState = $runtimeEnvelopeState
    cmeLedgerState = $cmeLedgerState
    cmeOfficeLedgerState = $cmeOfficeLedgerState
    cmeCareerContinuityLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $cmeFormationAndOfficeLedgerState -PropertyName 'careerContinuityLedgerState')
    seededGovernanceDisposition = $seedDisposition
    seededGovernanceReadyState = $seedReadyState
    sourceCandidateBundle = [string] $cycleState.lastReleaseCandidateBundle
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Sanctuary Runtime Readiness Receipt',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Readiness state: `{0}`' -f $payload.readinessState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Working state class: `{0}`' -f $payload.workingStateClass),
    ('- Runtime deployability envelope state: `{0}`' -f $(if ($payload.runtimeDeployabilityEnvelopeState) { $payload.runtimeDeployabilityEnvelopeState } else { 'missing' })),
    ('- CME ledger state: `{0}`' -f $(if ($payload.cmeLedgerState) { $payload.cmeLedgerState } else { 'missing' })),
    ('- CME office ledger state: `{0}`' -f $(if ($payload.cmeOfficeLedgerState) { $payload.cmeOfficeLedgerState } else { 'missing' })),
    ('- CME career continuity state: `{0}`' -f $(if ($payload.cmeCareerContinuityLedgerState) { $payload.cmeCareerContinuityLedgerState } else { 'missing' })),
    ('- Seeded governance disposition: `{0}`' -f $(if ($payload.seededGovernanceDisposition) { $payload.seededGovernanceDisposition } else { 'missing' })),
    ('- Seeded governance ready state: `{0}`' -f $(if ($payload.seededGovernanceReadyState) { $payload.seededGovernanceReadyState } else { 'missing' })),
    ('- Source candidate bundle: `{0}`' -f $(if ($payload.sourceCandidateBundle) { $payload.sourceCandidateBundle } else { 'missing' }))
)

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    readinessState = $payload.readinessState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    workingStateClass = $payload.workingStateClass
    runtimeDeployabilityEnvelopeState = $payload.runtimeDeployabilityEnvelopeState
    cmeLedgerState = $payload.cmeLedgerState
    cmeOfficeLedgerState = $payload.cmeOfficeLedgerState
    seededGovernanceDisposition = $payload.seededGovernanceDisposition
    seededGovernanceReadyState = $payload.seededGovernanceReadyState
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[sanctuary-runtime-readiness] Bundle: {0}' -f $bundlePath)
$bundlePath
