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
$bondedOperatorLocalityReadinessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.bondedOperatorLocalityReadinessStatePath)
$nexusSingularPortalFacadeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.nexusSingularPortalFacadeStatePath)
$duplexPredicateEnvelopeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.duplexPredicateEnvelopeStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.operatorActualWorkSessionRehearsalOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.operatorActualWorkSessionRehearsalStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the operator.actual work-session rehearsal can run.'
}

$reachAccessTopologyLedgerState = Read-JsonFileOrNull -Path $reachAccessTopologyLedgerStatePath
$bondedOperatorLocalityReadinessState = Read-JsonFileOrNull -Path $bondedOperatorLocalityReadinessStatePath
$nexusSingularPortalFacadeState = Read-JsonFileOrNull -Path $nexusSingularPortalFacadeStatePath
$duplexPredicateEnvelopeState = Read-JsonFileOrNull -Path $duplexPredicateEnvelopeStatePath

$rehearsalState = 'awaiting-singular-portal'
$reasonCode = 'operator-actual-work-session-rehearsal-awaiting-singular-portal'
$nextAction = 'emit-nexus-singular-portal-facade'

$reachLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $reachAccessTopologyLedgerState -PropertyName 'ledgerState')
$bondedReadinessState = [string] (Get-ObjectPropertyValueOrNull -InputObject $bondedOperatorLocalityReadinessState -PropertyName 'readinessState')
$nexusPortalState = [string] (Get-ObjectPropertyValueOrNull -InputObject $nexusSingularPortalFacadeState -PropertyName 'portalState')
$duplexState = [string] (Get-ObjectPropertyValueOrNull -InputObject $duplexPredicateEnvelopeState -PropertyName 'duplexState')

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $rehearsalState = 'blocked'
    $reasonCode = 'operator-actual-work-session-rehearsal-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($null -eq $nexusSingularPortalFacadeState -or $nexusPortalState -ne 'portal-facade-ready') {
    $rehearsalState = 'awaiting-singular-portal'
    $reasonCode = 'operator-actual-work-session-rehearsal-portal-not-ready'
    $nextAction = if ($null -ne $nexusSingularPortalFacadeState) { [string] $nexusSingularPortalFacadeState.nextAction } else { 'emit-nexus-singular-portal-facade' }
} elseif ($null -eq $duplexPredicateEnvelopeState -or $duplexState -ne 'duplex-envelope-ready') {
    $rehearsalState = 'awaiting-duplex-envelope'
    $reasonCode = 'operator-actual-work-session-rehearsal-duplex-not-ready'
    $nextAction = if ($null -ne $duplexPredicateEnvelopeState) { [string] $duplexPredicateEnvelopeState.nextAction } else { 'emit-duplex-predicate-envelope' }
} elseif ($reachLedgerState -ne 'provisional-reach-legibility') {
    $rehearsalState = 'awaiting-reach-legibility'
    $reasonCode = 'operator-actual-work-session-rehearsal-reach-not-ready'
    $nextAction = 'emit-reach-access-topology-ledger'
} else {
    $rehearsalState = 'rehearsal-bundle-ready'
    $reasonCode = 'operator-actual-work-session-rehearsal-bounded'
    $nextAction = 'review-hitl-bounded-operator-actual-rehearsal'
}

$coRealizedSurfaces = @(
    [ordered]@{
        surfaceName = 'Sanctuary.actual Bounded Runtime Work'
        locality = 'Sanctuary.actual'
        realizationClass = 'protected-inner-actuality'
    },
    [ordered]@{
        surfaceName = 'AgentiCore.actual Duplex Utility Surface'
        locality = 'AgentiCore.actual'
        realizationClass = 'governed-public-utility'
    },
    [ordered]@{
        surfaceName = 'Operator.actual Bounded Rehearsal Locality'
        locality = 'Operator.actual'
        realizationClass = 'co-realized-participation-locality'
    }
)

$withheldSurfaces = @(
    [ordered]@{
        surfaceName = 'Ratified Bonded Operator Actual Runtime'
        withholdingReason = 'bonded-operator-actual-remains-unratified'
    },
    [ordered]@{
        surfaceName = 'Deep Self-Rooted Cryptic Descent'
        withholdingReason = 'self-rooted-cryptic-depth-gate-not-yet-bound'
    },
    [ordered]@{
        surfaceName = 'Ambient Cross-Locality Grant'
        withholdingReason = 'ambient-cross-locality-grant-prohibited'
    }
)

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'operator-actual-work-session-rehearsal.json'
$bundleMarkdownPath = Join-Path $bundlePath 'operator-actual-work-session-rehearsal.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    rehearsalState = $rehearsalState
    reasonCode = $reasonCode
    nextAction = $nextAction
    participationModel = 'co-realized-locality-not-admin-console'
    reachLedgerState = $reachLedgerState
    bondedOperatorLocalityReadinessState = $bondedReadinessState
    nexusPortalState = $nexusPortalState
    duplexState = $duplexState
    returnCondition = 'dissolve-to-bounded-localities'
    dissolutionPath = 'return-to-sanctuary-actual-plus-operator-locality-separation'
    coRealizedSurfaces = $coRealizedSurfaces
    withheldSurfaces = $withheldSurfaces
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Operator.actual Work Session Rehearsal',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Rehearsal state: `{0}`' -f $payload.rehearsalState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Participation model: `{0}`' -f $payload.participationModel),
    ('- Reach ledger state: `{0}`' -f $(if ($payload.reachLedgerState) { $payload.reachLedgerState } else { 'missing' })),
    ('- Bonded operator locality readiness state: `{0}`' -f $(if ($payload.bondedOperatorLocalityReadinessState) { $payload.bondedOperatorLocalityReadinessState } else { 'missing' })),
    ('- Nexus portal state: `{0}`' -f $(if ($payload.nexusPortalState) { $payload.nexusPortalState } else { 'missing' })),
    ('- Duplex state: `{0}`' -f $(if ($payload.duplexState) { $payload.duplexState } else { 'missing' })),
    ('- Return condition: `{0}`' -f $payload.returnCondition),
    ('- Dissolution path: `{0}`' -f $payload.dissolutionPath),
    '',
    '## Co-Realized Surfaces',
    ''
)

foreach ($surface in @($coRealizedSurfaces)) {
    $markdownLines += @(
        ('### {0}' -f [string] $surface.surfaceName),
        ('- Locality: `{0}`' -f [string] $surface.locality),
        ('- Realization class: `{0}`' -f [string] $surface.realizationClass),
        ''
    )
}

$markdownLines += @(
    '## Withheld Surfaces',
    ''
)

foreach ($surface in @($withheldSurfaces)) {
    $markdownLines += @(
        ('### {0}' -f [string] $surface.surfaceName),
        ('- Withholding reason: `{0}`' -f [string] $surface.withholdingReason),
        ''
    )
}

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    rehearsalState = $payload.rehearsalState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    reachLedgerState = $payload.reachLedgerState
    bondedOperatorLocalityReadinessState = $payload.bondedOperatorLocalityReadinessState
    nexusPortalState = $payload.nexusPortalState
    duplexState = $payload.duplexState
    coRealizedSurfaceCount = @($coRealizedSurfaces).Count
    withheldSurfaceCount = @($withheldSurfaces).Count
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[operator-actual-work-session-rehearsal] Bundle: {0}' -f $bundlePath)
$bundlePath
