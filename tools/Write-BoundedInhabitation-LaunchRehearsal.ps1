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
$localHostSanctuaryResidencyEnvelopeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.localHostSanctuaryResidencyEnvelopeStatePath)
$runtimeHabitationReadinessLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runtimeHabitationReadinessLedgerStatePath)
$reachReturnDissolutionReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.reachReturnDissolutionReceiptStatePath)
$localityDistinctionWitnessLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.localityDistinctionWitnessLedgerStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.boundedInhabitationLaunchRehearsalOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.boundedInhabitationLaunchRehearsalStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the bounded inhabitation launch rehearsal writer can run.'
}

$localHostSanctuaryResidencyEnvelopeState = Read-JsonFileOrNull -Path $localHostSanctuaryResidencyEnvelopeStatePath
$runtimeHabitationReadinessLedgerState = Read-JsonFileOrNull -Path $runtimeHabitationReadinessLedgerStatePath
$reachReturnDissolutionReceiptState = Read-JsonFileOrNull -Path $reachReturnDissolutionReceiptStatePath
$localityDistinctionWitnessLedgerState = Read-JsonFileOrNull -Path $localityDistinctionWitnessLedgerStatePath

$envelopeState = [string] (Get-ObjectPropertyValueOrNull -InputObject $localHostSanctuaryResidencyEnvelopeState -PropertyName 'envelopeState')
$readinessLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $runtimeHabitationReadinessLedgerState -PropertyName 'readinessLedgerState')
$returnReceiptState = [string] (Get-ObjectPropertyValueOrNull -InputObject $reachReturnDissolutionReceiptState -PropertyName 'returnReceiptState')
$localityWitnessState = [string] (Get-ObjectPropertyValueOrNull -InputObject $localityDistinctionWitnessLedgerState -PropertyName 'witnessLedgerState')

$sourceFiles = Resolve-OanWorkspaceTouchPointFamily -BasePath $resolvedRepoRoot -FamilyName 'sanctuary-workbench-base' -CyclePolicy $cyclePolicy
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$keysSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$launchProjectionBound = $contractsSource.IndexOf('CreateBoundedInhabitationLaunchRehearsal', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('bounded-inhabitation-launch-rehearsal-bound', [System.StringComparison]::Ordinal) -ge 0
$launchKeyBound = $keysSource.IndexOf('CreateBoundedInhabitationLaunchRehearsalHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateBoundedInhabitationLaunchRehearsal', [System.StringComparison]::Ordinal) -ge 0

$launchRehearsalState = 'awaiting-residency-envelope'
$reasonCode = 'bounded-inhabitation-launch-rehearsal-awaiting-residency-envelope'
$nextAction = 'emit-local-host-sanctuary-residency-envelope'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $launchRehearsalState = 'blocked'
    $reasonCode = 'bounded-inhabitation-launch-rehearsal-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($envelopeState -ne 'local-host-sanctuary-residency-envelope-ready') {
    $launchRehearsalState = 'awaiting-residency-envelope'
    $reasonCode = 'bounded-inhabitation-launch-rehearsal-residency-envelope-not-ready'
    $nextAction = if ($null -ne $localHostSanctuaryResidencyEnvelopeState) { [string] $localHostSanctuaryResidencyEnvelopeState.nextAction } else { 'emit-local-host-sanctuary-residency-envelope' }
} elseif ($readinessLedgerState -ne 'runtime-habitation-readiness-ledger-ready') {
    $launchRehearsalState = 'awaiting-readiness-ledger'
    $reasonCode = 'bounded-inhabitation-launch-rehearsal-readiness-ledger-not-ready'
    $nextAction = if ($null -ne $runtimeHabitationReadinessLedgerState) { [string] $runtimeHabitationReadinessLedgerState.nextAction } else { 'emit-runtime-habitation-readiness-ledger' }
} elseif ($returnReceiptState -ne 'reach-return-dissolution-receipt-ready') {
    $launchRehearsalState = 'awaiting-return-dissolution'
    $reasonCode = 'bounded-inhabitation-launch-rehearsal-return-not-ready'
    $nextAction = if ($null -ne $reachReturnDissolutionReceiptState) { [string] $reachReturnDissolutionReceiptState.nextAction } else { 'emit-reach-return-dissolution-receipt' }
} elseif ($localityWitnessState -ne 'locality-distinction-witness-ledger-ready') {
    $launchRehearsalState = 'awaiting-locality-witness'
    $reasonCode = 'bounded-inhabitation-launch-rehearsal-locality-witness-not-ready'
    $nextAction = if ($null -ne $localityDistinctionWitnessLedgerState) { [string] $localityDistinctionWitnessLedgerState.nextAction } else { 'emit-locality-distinction-witness-ledger' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $launchProjectionBound -or -not $launchKeyBound -or -not $serviceBindingBound) {
    $launchRehearsalState = 'awaiting-launch-binding'
    $reasonCode = 'bounded-inhabitation-launch-rehearsal-source-missing'
    $nextAction = 'bind-bounded-inhabitation-launch-rehearsal'
} else {
    $launchRehearsalState = 'bounded-inhabitation-launch-rehearsal-ready'
    $reasonCode = 'bounded-inhabitation-launch-rehearsal-bound'
    $nextAction = 'continue-bounded-sanctuary-habitation'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'bounded-inhabitation-launch-rehearsal.json'
$bundleMarkdownPath = Join-Path $bundlePath 'bounded-inhabitation-launch-rehearsal.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    launchRehearsalState = $launchRehearsalState
    reasonCode = $reasonCode
    nextAction = $nextAction
    envelopeState = $envelopeState
    readinessLedgerState = $readinessLedgerState
    returnReceiptState = $returnReceiptState
    localityWitnessState = $localityWitnessState
    launchState = 'bounded-inhabitation-launch-ready'
    entryConditionCount = 3
    deniedLaneCount = 3
    returnClosureState = 'dissolution-witnessed'
    launchBounded = $true
    returnClosureWitnessed = $true
    ambientBondDenied = $true
    publicationPromotionDenied = $true
    launchProjectionBound = $launchProjectionBound
    launchKeyBound = $launchKeyBound
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
    '# Bounded Inhabitation Launch Rehearsal',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Launch-rehearsal state: `{0}`' -f $payload.launchRehearsalState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Residency-envelope state: `{0}`' -f $(if ($payload.envelopeState) { $payload.envelopeState } else { 'missing' })),
    ('- Readiness-ledger state: `{0}`' -f $(if ($payload.readinessLedgerState) { $payload.readinessLedgerState } else { 'missing' })),
    ('- Return-receipt state: `{0}`' -f $(if ($payload.returnReceiptState) { $payload.returnReceiptState } else { 'missing' })),
    ('- Locality-witness state: `{0}`' -f $(if ($payload.localityWitnessState) { $payload.localityWitnessState } else { 'missing' })),
    ('- Launch state: `{0}`' -f $payload.launchState),
    ('- Entry-condition count: `{0}`' -f $payload.entryConditionCount),
    ('- Denied-lane count: `{0}`' -f $payload.deniedLaneCount),
    ('- Return-closure state: `{0}`' -f $payload.returnClosureState),
    ('- Launch bounded: `{0}`' -f [bool] $payload.launchBounded),
    ('- Return closure witnessed: `{0}`' -f [bool] $payload.returnClosureWitnessed),
    ('- Ambient bond denied: `{0}`' -f [bool] $payload.ambientBondDenied),
    ('- Publication promotion denied: `{0}`' -f [bool] $payload.publicationPromotionDenied),
    ('- Launch projection bound: `{0}`' -f [bool] $payload.launchProjectionBound),
    ('- Launch key bound: `{0}`' -f [bool] $payload.launchKeyBound),
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
    launchRehearsalState = $payload.launchRehearsalState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    envelopeState = $payload.envelopeState
    readinessLedgerState = $payload.readinessLedgerState
    returnReceiptState = $payload.returnReceiptState
    localityWitnessState = $payload.localityWitnessState
    launchState = $payload.launchState
    entryConditionCount = $payload.entryConditionCount
    deniedLaneCount = $payload.deniedLaneCount
    returnClosureState = $payload.returnClosureState
    launchBounded = $payload.launchBounded
    returnClosureWitnessed = $payload.returnClosureWitnessed
    ambientBondDenied = $payload.ambientBondDenied
    publicationPromotionDenied = $payload.publicationPromotionDenied
    launchProjectionBound = $payload.launchProjectionBound
    serviceBindingBound = $payload.serviceBindingBound
    sourceFileCount = $payload.sourceFileCount
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[bounded-inhabitation-launch-rehearsal] Bundle: {0}' -f $bundlePath)
$bundlePath
