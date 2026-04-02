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
$sanctuaryRuntimeReadinessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.sanctuaryRuntimeReadinessStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.identityInvariantThreadRootOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.identityInvariantThreadRootStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the identity-invariant thread-root writer can run.'
}

$sanctuaryRuntimeReadinessState = Read-JsonFileOrNull -Path $sanctuaryRuntimeReadinessStatePath
$runtimeReadinessState = [string] (Get-ObjectPropertyValueOrNull -InputObject $sanctuaryRuntimeReadinessState -PropertyName 'readinessState')

if (-not (Get-Command -Name Resolve-OanWorkspaceTouchPointFamily -ErrorAction SilentlyContinue)) {
    $oanWorkspaceResolverPath = Join-Path $PSScriptRoot 'Resolve-OanWorkspacePath.ps1'
    if (Test-Path -LiteralPath $oanWorkspaceResolverPath -PathType Leaf) {
        . $oanWorkspaceResolverPath
    }
}

$familyResolution = @(Get-OanWorkspaceTouchPointFamilyResolution -BasePath $resolvedRepoRoot -FamilyName 'worker-thread-identity' -CyclePolicy $cyclePolicy)
$sourceFiles = @($familyResolution | ForEach-Object { [string] $_.SelectedPath })
$missingSourceFiles = @($familyResolution | Where-Object { -not [bool] $_.SelectedPathExists })
$missingBuildTouchPoints = @($missingSourceFiles | Where-Object { [string] $_.TouchPointStatus -ne 'research-handoff' })
$researchHandOffTouchPoints = @($missingSourceFiles | Where-Object { [string] $_.TouchPointStatus -eq 'research-handoff' })
$contractsSource = if ($sourceFiles.Count -gt 0 -and (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf)) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$serviceSource = if ($sourceFiles.Count -gt 2 -and (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf)) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$threadRootProjectionBound = $contractsSource.IndexOf('CreateIdentityInvariantThreadRoot', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateIdentityThreadRoot', [System.StringComparison]::Ordinal) -ge 0

$threadRootState = 'awaiting-bounded-runtime-readiness'
$reasonCode = 'identity-invariant-thread-root-awaiting-bounded-runtime-readiness'
$nextAction = 'emit-sanctuary-runtime-readiness-receipt'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $threadRootState = 'blocked'
    $reasonCode = 'identity-invariant-thread-root-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($runtimeReadinessState -ne 'bounded-working-state-ready') {
    $threadRootState = 'awaiting-bounded-runtime-readiness'
    $reasonCode = 'identity-invariant-thread-root-runtime-not-ready'
    $nextAction = if ($null -ne $sanctuaryRuntimeReadinessState) { [string] $sanctuaryRuntimeReadinessState.nextAction } else { 'emit-sanctuary-runtime-readiness-receipt' }
} elseif ($missingBuildTouchPoints.Count -gt 0 -or -not $threadRootProjectionBound -or -not $serviceBindingBound) {
    $threadRootState = 'awaiting-thread-root-binding'
    $reasonCode = 'identity-invariant-thread-root-source-missing'
    $nextAction = 'bind-worker-thread-root-governance-surface'
} elseif ($researchHandOffTouchPoints.Count -gt 0) {
    $threadRootState = 'awaiting-thread-root-research-return'
    $reasonCode = 'identity-invariant-thread-root-research-handoff-pending'
    $nextAction = 'review-source-bucket-return-for-worker-thread-root'
} else {
    $threadRootState = 'identity-thread-root-ready'
    $reasonCode = 'identity-invariant-thread-root-bound'
    $nextAction = 'emit-governed-thread-birth-receipt'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'identity-invariant-thread-root.json'
$bundleMarkdownPath = Join-Path $bundlePath 'identity-invariant-thread-root.md'

$projectSpaceId = 'oan-mortalis-v1.1.1'
$threadId = 'worker-thread-root://local-main-worker'
$governanceRootId = 'identity-invariant://bounded-local-governance-root'
$scopeClass = 'bounded-local-thread-governance'
$bindBurdenClass = 'worker-thread-governance-root'
$continuityParent = 'none'
$authorizationBasis = 'bounded-runtime-readiness'
$carryForwardPolicy = 'receipted-thread-local-only'
$witnessEventId = 'thread-root-bind-{0}' -f $timestamp

$stateClass = 'provisional'
$bindState = 'provisional-bind'
$witnessStatus = 'awaiting-thread-root-bind-witness'

if ($threadRootState -eq 'blocked') {
    $stateClass = 'hold'
    $bindState = 'hold'
    $witnessStatus = 'thread-root-hold-witnessed'
} elseif ($threadRootState -eq 'identity-thread-root-ready') {
    $stateClass = 'ready'
    $bindState = 'bound'
    $witnessStatus = 'thread-root-bind-witnessed'
} elseif ($threadRootState -eq 'awaiting-bounded-runtime-readiness') {
    $bindState = 'configured'
}

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    threadRootState = $threadRootState
    reasonCode = $reasonCode
    nextAction = $nextAction
    researchHandOffPending = $researchHandOffTouchPoints.Count -gt 0
    researchHandOffTouchPointIds = @($researchHandOffTouchPoints | ForEach-Object { [string] $_.TouchPointId })
    projectSpaceId = $projectSpaceId
    threadId = $threadId
    governanceRootId = $governanceRootId
    scopeClass = $scopeClass
    stateClass = $stateClass
    bindState = $bindState
    witnessStatus = $witnessStatus
    witnessEventId = $witnessEventId
    bindTimestampUtc = (Get-Date).ToUniversalTime().ToString('o')
    bindBurdenClass = $bindBurdenClass
    continuityParent = $continuityParent
    authorizationBasis = $authorizationBasis
    carryForwardPolicy = $carryForwardPolicy
    runtimeReadinessState = $runtimeReadinessState
    continuityClass = 'identity-invariant-local-thread-root'
    ambientSharedIdentityDenied = $true
    interWorkerBraidRequired = $true
    threadRootHandlePrefix = 'worker-thread-root://'
    identityInvariantHandlePrefix = 'identity-invariant://'
    threadRootProjectionBound = $threadRootProjectionBound
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
    '# Identity-Invariant Thread Root',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Thread-root state: `{0}`' -f $payload.threadRootState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Research handoff pending: `{0}`' -f [bool] $payload.researchHandOffPending),
    ('- Research handoff touchpoints: `{0}`' -f $(if (@($payload.researchHandOffTouchPointIds).Count -gt 0) { (@($payload.researchHandOffTouchPointIds) -join '`, `') } else { 'none' })),
    ('- Project-space id: `{0}`' -f $payload.projectSpaceId),
    ('- Thread id: `{0}`' -f $payload.threadId),
    ('- Governance-root id: `{0}`' -f $payload.governanceRootId),
    ('- Scope class: `{0}`' -f $payload.scopeClass),
    ('- State class: `{0}`' -f $payload.stateClass),
    ('- Bind state: `{0}`' -f $payload.bindState),
    ('- Witness status: `{0}`' -f $payload.witnessStatus),
    ('- Witness event id: `{0}`' -f $payload.witnessEventId),
    ('- Bind timestamp (UTC): `{0}`' -f $payload.bindTimestampUtc),
    ('- Bind burden class: `{0}`' -f $payload.bindBurdenClass),
    ('- Continuity parent: `{0}`' -f $payload.continuityParent),
    ('- Authorization basis: `{0}`' -f $payload.authorizationBasis),
    ('- Carry-forward policy: `{0}`' -f $payload.carryForwardPolicy),
    ('- Runtime readiness state: `{0}`' -f $(if ($payload.runtimeReadinessState) { $payload.runtimeReadinessState } else { 'missing' })),
    ('- Continuity class: `{0}`' -f $payload.continuityClass),
    ('- Ambient shared identity denied: `{0}`' -f [bool] $payload.ambientSharedIdentityDenied),
    ('- Inter-worker braid required: `{0}`' -f [bool] $payload.interWorkerBraidRequired),
    ('- Thread-root projection bound: `{0}`' -f [bool] $payload.threadRootProjectionBound),
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
    threadRootState = $payload.threadRootState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    researchHandOffPending = $payload.researchHandOffPending
    researchHandOffTouchPointIds = $payload.researchHandOffTouchPointIds
    projectSpaceId = $payload.projectSpaceId
    threadId = $payload.threadId
    governanceRootId = $payload.governanceRootId
    scopeClass = $payload.scopeClass
    stateClass = $payload.stateClass
    bindState = $payload.bindState
    witnessStatus = $payload.witnessStatus
    witnessEventId = $payload.witnessEventId
    bindTimestampUtc = $payload.bindTimestampUtc
    bindBurdenClass = $payload.bindBurdenClass
    continuityParent = $payload.continuityParent
    authorizationBasis = $payload.authorizationBasis
    carryForwardPolicy = $payload.carryForwardPolicy
    runtimeReadinessState = $payload.runtimeReadinessState
    threadRootProjectionBound = $payload.threadRootProjectionBound
    serviceBindingBound = $payload.serviceBindingBound
    sourceFileCount = $payload.sourceFileCount
    missingSourceFileCount = $payload.missingSourceFileCount
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[identity-invariant-thread-root] Bundle: {0}' -f $bundlePath)
$bundlePath
