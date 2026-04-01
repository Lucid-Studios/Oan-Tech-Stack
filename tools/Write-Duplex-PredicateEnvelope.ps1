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

    if (-not (Get-Command -Name Resolve-OanWorkspacePath -ErrorAction SilentlyContinue)) {
        $oanWorkspaceResolverPath = Join-Path $PSScriptRoot 'Resolve-OanWorkspacePath.ps1'
        if (Test-Path -LiteralPath $oanWorkspaceResolverPath -PathType Leaf) {
            . $oanWorkspaceResolverPath
        }
    }

    if (Get-Command -Name Resolve-OanWorkspacePath -ErrorAction SilentlyContinue) {
        return Resolve-OanWorkspacePath -BasePath $BasePath -CandidatePath $CandidatePath
    }

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
$runtimeWorkSurfaceAdmissibilityStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runtimeWorkSurfaceAdmissibilityStatePath)
$nexusSingularPortalFacadeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.nexusSingularPortalFacadeStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.duplexPredicateEnvelopeOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.duplexPredicateEnvelopeStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the duplex predicate envelope can run.'
}

$runtimeWorkSurfaceAdmissibilityState = Read-JsonFileOrNull -Path $runtimeWorkSurfaceAdmissibilityStatePath
$nexusSingularPortalFacadeState = Read-JsonFileOrNull -Path $nexusSingularPortalFacadeStatePath

$duplexSourceFiles = Resolve-OanWorkspaceTouchPointFamily -BasePath $resolvedRepoRoot -FamilyName 'duplex-envelope-base' -CyclePolicy $cyclePolicy
$missingDuplexSourceFiles = @($duplexSourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$agentiRuntimePath = Resolve-OanWorkspaceTouchPoint -BasePath $resolvedRepoRoot -TouchPointId 'runtime.agentiRuntime' -CyclePolicy $cyclePolicy
$agentiRuntimeSource = if (Test-Path -LiteralPath $agentiRuntimePath -PathType Leaf) { Get-Content -Raw -LiteralPath $agentiRuntimePath } else { '' }

$duplexState = 'awaiting-runtime-admissibility'
$reasonCode = 'duplex-predicate-envelope-awaiting-runtime-admissibility'
$nextAction = 'emit-runtime-work-surface-admissibility'

$runtimeAdmissibilityState = [string] (Get-ObjectPropertyValueOrNull -InputObject $runtimeWorkSurfaceAdmissibilityState -PropertyName 'admissibilityState')
$nexusPortalState = [string] (Get-ObjectPropertyValueOrNull -InputObject $nexusSingularPortalFacadeState -PropertyName 'portalState')
$sendDuplexIntentBound = $agentiRuntimeSource.IndexOf('SendDuplexIntentAsync', [System.StringComparison]::Ordinal) -ge 0

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $duplexState = 'blocked'
    $reasonCode = 'duplex-predicate-envelope-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($null -eq $runtimeWorkSurfaceAdmissibilityState -or $runtimeAdmissibilityState -notin @('bounded-internal-work-only', 'provisional-runtime-work')) {
    $duplexState = 'awaiting-runtime-admissibility'
    $reasonCode = 'duplex-predicate-envelope-runtime-not-admissible'
    $nextAction = if ($null -ne $runtimeWorkSurfaceAdmissibilityState) { [string] $runtimeWorkSurfaceAdmissibilityState.nextAction } else { 'emit-runtime-work-surface-admissibility' }
} elseif ($null -eq $nexusSingularPortalFacadeState -or $nexusPortalState -ne 'portal-facade-ready') {
    $duplexState = 'awaiting-singular-portal'
    $reasonCode = 'duplex-predicate-envelope-portal-not-ready'
    $nextAction = if ($null -ne $nexusSingularPortalFacadeState) { [string] $nexusSingularPortalFacadeState.nextAction } else { 'emit-nexus-singular-portal-facade' }
} elseif ($missingDuplexSourceFiles.Count -gt 0 -or -not $sendDuplexIntentBound) {
    $duplexState = 'awaiting-duplex-binding'
    $reasonCode = 'duplex-predicate-envelope-source-missing'
    $nextAction = 'bind-agenticore-duplex-envelope'
} else {
    $duplexState = 'duplex-envelope-ready'
    $reasonCode = 'duplex-predicate-envelope-bound'
    $nextAction = 'emit-operator-actual-work-session-rehearsal'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'duplex-predicate-envelope.json'
$bundleMarkdownPath = Join-Path $bundlePath 'duplex-predicate-envelope.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    duplexState = $duplexState
    reasonCode = $reasonCode
    nextAction = $nextAction
    runtimeAdmissibilityState = $runtimeAdmissibilityState
    nexusPortalState = $nexusPortalState
    workPredicateBound = (Test-Path -LiteralPath $duplexSourceFiles[0] -PathType Leaf)
    governancePredicateBound = $sendDuplexIntentBound
    sourceFileCount = @($duplexSourceFiles).Count
    missingSourceFileCount = @($missingDuplexSourceFiles).Count
    sourceFiles = @(
        foreach ($file in $duplexSourceFiles) {
            [ordered]@{
                path = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $file
                present = (Test-Path -LiteralPath $file -PathType Leaf)
            }
        }
    )
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Duplex Predicate Envelope',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Duplex state: `{0}`' -f $payload.duplexState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Runtime admissibility state: `{0}`' -f $(if ($payload.runtimeAdmissibilityState) { $payload.runtimeAdmissibilityState } else { 'missing' })),
    ('- Nexus portal state: `{0}`' -f $(if ($payload.nexusPortalState) { $payload.nexusPortalState } else { 'missing' })),
    ('- Work predicate bound: `{0}`' -f [bool] $payload.workPredicateBound),
    ('- Governance predicate bound: `{0}`' -f [bool] $payload.governancePredicateBound),
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
    duplexState = $payload.duplexState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    runtimeAdmissibilityState = $payload.runtimeAdmissibilityState
    nexusPortalState = $payload.nexusPortalState
    workPredicateBound = $payload.workPredicateBound
    governancePredicateBound = $payload.governancePredicateBound
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[duplex-predicate-envelope] Bundle: {0}' -f $bundlePath)
$bundlePath
