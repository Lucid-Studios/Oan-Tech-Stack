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
$sanctuaryRuntimeWorkbenchSurfaceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.sanctuaryRuntimeWorkbenchSurfaceStatePath)
$runtimeWorkbenchSessionLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runtimeWorkbenchSessionLedgerStatePath)
$reachReturnDissolutionReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.reachReturnDissolutionReceiptStatePath)
$localityDistinctionWitnessLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.localityDistinctionWitnessLedgerStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.localHostSanctuaryResidencyEnvelopeOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.localHostSanctuaryResidencyEnvelopeStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the local-host Sanctuary residency envelope writer can run.'
}

$sanctuaryRuntimeWorkbenchSurfaceState = Read-JsonFileOrNull -Path $sanctuaryRuntimeWorkbenchSurfaceStatePath
$runtimeWorkbenchSessionLedgerState = Read-JsonFileOrNull -Path $runtimeWorkbenchSessionLedgerStatePath
$reachReturnDissolutionReceiptState = Read-JsonFileOrNull -Path $reachReturnDissolutionReceiptStatePath
$localityDistinctionWitnessLedgerState = Read-JsonFileOrNull -Path $localityDistinctionWitnessLedgerStatePath

$workbenchState = [string] (Get-ObjectPropertyValueOrNull -InputObject $sanctuaryRuntimeWorkbenchSurfaceState -PropertyName 'workbenchState')
$sessionLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $runtimeWorkbenchSessionLedgerState -PropertyName 'sessionLedgerState')
$returnReceiptState = [string] (Get-ObjectPropertyValueOrNull -InputObject $reachReturnDissolutionReceiptState -PropertyName 'returnReceiptState')
$localityWitnessState = [string] (Get-ObjectPropertyValueOrNull -InputObject $localityDistinctionWitnessLedgerState -PropertyName 'witnessLedgerState')

$sourceFiles = Resolve-OanWorkspaceTouchPointFamily -BasePath $resolvedRepoRoot -FamilyName 'sanctuary-workbench-base' -CyclePolicy $cyclePolicy
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$keysSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$residencyProjectionBound = $contractsSource.IndexOf('CreateLocalHostSanctuaryResidencyEnvelope', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('local-host-sanctuary-residency-envelope-bound', [System.StringComparison]::Ordinal) -ge 0
$residencyKeyBound = $keysSource.IndexOf('CreateLocalHostSanctuaryResidencyEnvelopeHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateLocalHostSanctuaryResidencyEnvelope', [System.StringComparison]::Ordinal) -ge 0

$envelopeState = 'awaiting-runtime-workbench-surface'
$reasonCode = 'local-host-sanctuary-residency-envelope-awaiting-runtime-workbench-surface'
$nextAction = 'emit-sanctuary-runtime-workbench-surface'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $envelopeState = 'blocked'
    $reasonCode = 'local-host-sanctuary-residency-envelope-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($workbenchState -ne 'sanctuary-runtime-workbench-ready') {
    $envelopeState = 'awaiting-runtime-workbench-surface'
    $reasonCode = 'local-host-sanctuary-residency-envelope-workbench-not-ready'
    $nextAction = if ($null -ne $sanctuaryRuntimeWorkbenchSurfaceState) { [string] $sanctuaryRuntimeWorkbenchSurfaceState.nextAction } else { 'emit-sanctuary-runtime-workbench-surface' }
} elseif ($sessionLedgerState -ne 'runtime-workbench-session-ledger-ready') {
    $envelopeState = 'awaiting-session-ledger'
    $reasonCode = 'local-host-sanctuary-residency-envelope-session-ledger-not-ready'
    $nextAction = if ($null -ne $runtimeWorkbenchSessionLedgerState) { [string] $runtimeWorkbenchSessionLedgerState.nextAction } else { 'emit-runtime-workbench-session-ledger' }
} elseif ($returnReceiptState -ne 'reach-return-dissolution-receipt-ready') {
    $envelopeState = 'awaiting-return-dissolution'
    $reasonCode = 'local-host-sanctuary-residency-envelope-return-not-ready'
    $nextAction = if ($null -ne $reachReturnDissolutionReceiptState) { [string] $reachReturnDissolutionReceiptState.nextAction } else { 'emit-reach-return-dissolution-receipt' }
} elseif ($localityWitnessState -ne 'locality-distinction-witness-ledger-ready') {
    $envelopeState = 'awaiting-locality-witness'
    $reasonCode = 'local-host-sanctuary-residency-envelope-locality-witness-not-ready'
    $nextAction = if ($null -ne $localityDistinctionWitnessLedgerState) { [string] $localityDistinctionWitnessLedgerState.nextAction } else { 'emit-locality-distinction-witness-ledger' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $residencyProjectionBound -or -not $residencyKeyBound -or -not $serviceBindingBound) {
    $envelopeState = 'awaiting-residency-binding'
    $reasonCode = 'local-host-sanctuary-residency-envelope-source-missing'
    $nextAction = 'bind-local-host-sanctuary-residency-envelope'
} else {
    $envelopeState = 'local-host-sanctuary-residency-envelope-ready'
    $reasonCode = 'local-host-sanctuary-residency-envelope-bound'
    $nextAction = 'emit-runtime-habitation-readiness-ledger'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'local-host-sanctuary-residency-envelope.json'
$bundleMarkdownPath = Join-Path $bundlePath 'local-host-sanctuary-residency-envelope.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    envelopeState = $envelopeState
    reasonCode = $reasonCode
    nextAction = $nextAction
    workbenchState = $workbenchState
    sessionLedgerState = $sessionLedgerState
    returnReceiptState = $returnReceiptState
    localityWitnessState = $localityWitnessState
    residencyState = 'bounded-local-sanctuary-residency'
    residencyClass = 'bounded-local-sanctuary-residency'
    hostLocalResourceCount = 3
    admittedResidencyLaneCount = 3
    withheldResidencyLaneCount = 3
    bondedReleaseDenied = $true
    publicationMaturityDenied = $true
    mosBearingDepthDenied = $true
    residencyProjectionBound = $residencyProjectionBound
    residencyKeyBound = $residencyKeyBound
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
    '# Local Host Sanctuary Residency Envelope',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Envelope state: `{0}`' -f $payload.envelopeState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Workbench state: `{0}`' -f $(if ($payload.workbenchState) { $payload.workbenchState } else { 'missing' })),
    ('- Session-ledger state: `{0}`' -f $(if ($payload.sessionLedgerState) { $payload.sessionLedgerState } else { 'missing' })),
    ('- Return-receipt state: `{0}`' -f $(if ($payload.returnReceiptState) { $payload.returnReceiptState } else { 'missing' })),
    ('- Locality-witness state: `{0}`' -f $(if ($payload.localityWitnessState) { $payload.localityWitnessState } else { 'missing' })),
    ('- Residency state: `{0}`' -f $payload.residencyState),
    ('- Residency class: `{0}`' -f $payload.residencyClass),
    ('- Host-local resource count: `{0}`' -f $payload.hostLocalResourceCount),
    ('- Admitted residency-lane count: `{0}`' -f $payload.admittedResidencyLaneCount),
    ('- Withheld residency-lane count: `{0}`' -f $payload.withheldResidencyLaneCount),
    ('- Bonded release denied: `{0}`' -f [bool] $payload.bondedReleaseDenied),
    ('- Publication maturity denied: `{0}`' -f [bool] $payload.publicationMaturityDenied),
    ('- MoS-bearing depth denied: `{0}`' -f [bool] $payload.mosBearingDepthDenied),
    ('- Residency projection bound: `{0}`' -f [bool] $payload.residencyProjectionBound),
    ('- Residency key bound: `{0}`' -f [bool] $payload.residencyKeyBound),
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
    envelopeState = $payload.envelopeState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    workbenchState = $payload.workbenchState
    sessionLedgerState = $payload.sessionLedgerState
    returnReceiptState = $payload.returnReceiptState
    localityWitnessState = $payload.localityWitnessState
    residencyState = $payload.residencyState
    residencyClass = $payload.residencyClass
    hostLocalResourceCount = $payload.hostLocalResourceCount
    admittedResidencyLaneCount = $payload.admittedResidencyLaneCount
    withheldResidencyLaneCount = $payload.withheldResidencyLaneCount
    bondedReleaseDenied = $payload.bondedReleaseDenied
    publicationMaturityDenied = $payload.publicationMaturityDenied
    mosBearingDepthDenied = $payload.mosBearingDepthDenied
    residencyProjectionBound = $payload.residencyProjectionBound
    serviceBindingBound = $payload.serviceBindingBound
    sourceFileCount = $payload.sourceFileCount
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[local-host-sanctuary-residency-envelope] Bundle: {0}' -f $bundlePath)
$bundlePath
