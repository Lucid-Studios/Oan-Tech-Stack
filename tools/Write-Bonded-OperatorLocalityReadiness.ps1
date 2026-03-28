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

    $Value | ConvertTo-Json -Depth 16 | Set-Content -LiteralPath $Path -Encoding utf8
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
$reachAccessTopologyLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.reachAccessTopologyLedgerStatePath)
$sanctuaryRuntimeReadinessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.sanctuaryRuntimeReadinessStatePath)
$runtimeWorkSurfaceAdmissibilityStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runtimeWorkSurfaceAdmissibilityStatePath)
$nexusSingularPortalFacadeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.nexusSingularPortalFacadeStatePath)
$duplexPredicateEnvelopeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.duplexPredicateEnvelopeStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.bondedOperatorLocalityReadinessOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.bondedOperatorLocalityReadinessStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before bonded operator locality readiness can run.'
}

$reachAccessTopologyLedgerState = Read-JsonFileOrNull -Path $reachAccessTopologyLedgerStatePath
$sanctuaryRuntimeReadinessState = Read-JsonFileOrNull -Path $sanctuaryRuntimeReadinessStatePath
$runtimeWorkSurfaceAdmissibilityState = Read-JsonFileOrNull -Path $runtimeWorkSurfaceAdmissibilityStatePath
$nexusSingularPortalFacadeState = Read-JsonFileOrNull -Path $nexusSingularPortalFacadeStatePath
$duplexPredicateEnvelopeState = Read-JsonFileOrNull -Path $duplexPredicateEnvelopeStatePath

$readinessState = 'dormant-before-bond'
$reasonCode = 'bonded-operator-locality-readiness-dormant-before-bond'
$nextAction = 'continue-bounded-sanctuary-runtime-work'

$runtimeReadinessState = [string] (Get-ObjectPropertyValueOrNull -InputObject $sanctuaryRuntimeReadinessState -PropertyName 'readinessState')
$runtimeAdmissibilityState = [string] (Get-ObjectPropertyValueOrNull -InputObject $runtimeWorkSurfaceAdmissibilityState -PropertyName 'admissibilityState')
$reachLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $reachAccessTopologyLedgerState -PropertyName 'ledgerState')
$nexusPortalState = [string] (Get-ObjectPropertyValueOrNull -InputObject $nexusSingularPortalFacadeState -PropertyName 'portalState')
$duplexState = [string] (Get-ObjectPropertyValueOrNull -InputObject $duplexPredicateEnvelopeState -PropertyName 'duplexState')

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $readinessState = 'blocked'
    $reasonCode = 'bonded-operator-locality-readiness-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($null -eq $sanctuaryRuntimeReadinessState -or $runtimeReadinessState -ne 'bounded-working-state-ready') {
    $readinessState = 'dormant-before-bond'
    $reasonCode = 'bonded-operator-locality-readiness-runtime-not-ready'
    $nextAction = if ($null -ne $sanctuaryRuntimeReadinessState) { [string] $sanctuaryRuntimeReadinessState.nextAction } else { 'emit-sanctuary-runtime-readiness' }
} elseif ($null -eq $reachAccessTopologyLedgerState) {
    $readinessState = 'awaiting-access-topology'
    $reasonCode = 'bonded-operator-locality-readiness-access-topology-missing'
    $nextAction = 'emit-reach-access-topology-ledger'
} elseif ($nexusPortalState -eq 'portal-facade-ready' -and $duplexState -eq 'duplex-envelope-ready') {
    $readinessState = 'ready-for-bounded-rehearsal'
    $reasonCode = 'bonded-operator-locality-readiness-bounded-rehearsal-ready'
    $nextAction = 'emit-operator-actual-work-session-rehearsal'
} elseif ($reachLedgerState -eq 'provisional-reach-legibility' -and $runtimeAdmissibilityState -in @('bounded-internal-work-only', 'provisional-runtime-work')) {
    $readinessState = 'withheld-before-bounded-rehearsal'
    $reasonCode = 'bonded-operator-locality-readiness-reach-not-bound'
    $nextAction = 'bind-singular-nexus-and-duplex-predicate-envelope'
} else {
    $readinessState = 'provisional'
    $reasonCode = 'bonded-operator-locality-readiness-provisional'
    $nextAction = 'prepare-bounded-operator-actual-rehearsal'
}

$requiredSurfaces = @(
    [ordered]@{
        surfaceName = 'Nexus Singular Portal Facade'
        requirementClass = 'contact-surface'
    },
    [ordered]@{
        surfaceName = 'Duplex Predicate Envelope'
        requirementClass = 'governed-utility'
    },
    [ordered]@{
        surfaceName = 'Operator.actual Work Session Rehearsal'
        requirementClass = 'bounded-rehearsal'
    }
)

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'bonded-operator-locality-readiness.json'
$bundleMarkdownPath = Join-Path $bundlePath 'bonded-operator-locality-readiness.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    readinessState = $readinessState
    reasonCode = $reasonCode
    nextAction = $nextAction
    participationModel = 'co-realized-locality-not-admin-console'
    runtimeReadinessState = $runtimeReadinessState
    runtimeAdmissibilityState = $runtimeAdmissibilityState
    reachLedgerState = $reachLedgerState
    nexusPortalState = $nexusPortalState
    duplexState = $duplexState
    requiredSurfaces = $requiredSurfaces
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Bonded Operator Locality Readiness',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Readiness state: `{0}`' -f $payload.readinessState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Participation model: `{0}`' -f $payload.participationModel),
    ('- Runtime readiness state: `{0}`' -f $(if ($payload.runtimeReadinessState) { $payload.runtimeReadinessState } else { 'missing' })),
    ('- Runtime admissibility state: `{0}`' -f $(if ($payload.runtimeAdmissibilityState) { $payload.runtimeAdmissibilityState } else { 'missing' })),
    ('- Reach ledger state: `{0}`' -f $(if ($payload.reachLedgerState) { $payload.reachLedgerState } else { 'missing' })),
    ('- Nexus portal state: `{0}`' -f $(if ($payload.nexusPortalState) { $payload.nexusPortalState } else { 'missing' })),
    ('- Duplex state: `{0}`' -f $(if ($payload.duplexState) { $payload.duplexState } else { 'missing' })),
    '',
    '## Required Surfaces',
    ''
)

foreach ($surface in $requiredSurfaces) {
    $markdownLines += @(
        ('### {0}' -f [string] $surface.surfaceName),
        ('- Requirement class: `{0}`' -f [string] $surface.requirementClass),
        ''
    )
}

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    readinessState = $payload.readinessState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    runtimeReadinessState = $payload.runtimeReadinessState
    runtimeAdmissibilityState = $payload.runtimeAdmissibilityState
    reachLedgerState = $payload.reachLedgerState
    nexusPortalState = $payload.nexusPortalState
    duplexState = $payload.duplexState
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[bonded-operator-locality-readiness] Bundle: {0}' -f $bundlePath)
$bundlePath
