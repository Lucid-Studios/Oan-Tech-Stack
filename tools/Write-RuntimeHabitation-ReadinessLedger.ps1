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
$runtimeWorkbenchSessionLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runtimeWorkbenchSessionLedgerStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runtimeHabitationReadinessLedgerOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runtimeHabitationReadinessLedgerStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the runtime habitation readiness ledger writer can run.'
}

$localHostSanctuaryResidencyEnvelopeState = Read-JsonFileOrNull -Path $localHostSanctuaryResidencyEnvelopeStatePath
$runtimeWorkbenchSessionLedgerState = Read-JsonFileOrNull -Path $runtimeWorkbenchSessionLedgerStatePath

$envelopeState = [string] (Get-ObjectPropertyValueOrNull -InputObject $localHostSanctuaryResidencyEnvelopeState -PropertyName 'envelopeState')
$sessionLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $runtimeWorkbenchSessionLedgerState -PropertyName 'sessionLedgerState')

$sourceFiles = Resolve-OanWorkspaceTouchPointFamily -BasePath $resolvedRepoRoot -FamilyName 'sanctuary-workbench-base' -CyclePolicy $cyclePolicy
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$keysSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$readinessProjectionBound = $contractsSource.IndexOf('CreateRuntimeHabitationReadinessLedger', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('runtime-habitation-readiness-ledger-bound', [System.StringComparison]::Ordinal) -ge 0
$readinessKeyBound = $keysSource.IndexOf('CreateRuntimeHabitationReadinessLedgerHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateRuntimeHabitationReadinessLedger', [System.StringComparison]::Ordinal) -ge 0

$readinessLedgerState = 'awaiting-residency-envelope'
$reasonCode = 'runtime-habitation-readiness-ledger-awaiting-residency-envelope'
$nextAction = 'emit-local-host-sanctuary-residency-envelope'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $readinessLedgerState = 'blocked'
    $reasonCode = 'runtime-habitation-readiness-ledger-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($envelopeState -ne 'local-host-sanctuary-residency-envelope-ready') {
    $readinessLedgerState = 'awaiting-residency-envelope'
    $reasonCode = 'runtime-habitation-readiness-ledger-residency-envelope-not-ready'
    $nextAction = if ($null -ne $localHostSanctuaryResidencyEnvelopeState) { [string] $localHostSanctuaryResidencyEnvelopeState.nextAction } else { 'emit-local-host-sanctuary-residency-envelope' }
} elseif ($sessionLedgerState -ne 'runtime-workbench-session-ledger-ready') {
    $readinessLedgerState = 'awaiting-session-ledger'
    $reasonCode = 'runtime-habitation-readiness-ledger-session-ledger-not-ready'
    $nextAction = if ($null -ne $runtimeWorkbenchSessionLedgerState) { [string] $runtimeWorkbenchSessionLedgerState.nextAction } else { 'emit-runtime-workbench-session-ledger' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $readinessProjectionBound -or -not $readinessKeyBound -or -not $serviceBindingBound) {
    $readinessLedgerState = 'awaiting-readiness-binding'
    $reasonCode = 'runtime-habitation-readiness-ledger-source-missing'
    $nextAction = 'bind-runtime-habitation-readiness-ledger'
} else {
    $readinessLedgerState = 'runtime-habitation-readiness-ledger-ready'
    $reasonCode = 'runtime-habitation-readiness-ledger-bound'
    $nextAction = 'emit-bounded-inhabitation-launch-rehearsal'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'runtime-habitation-readiness-ledger.json'
$bundleMarkdownPath = Join-Path $bundlePath 'runtime-habitation-readiness-ledger.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    readinessLedgerState = $readinessLedgerState
    reasonCode = $reasonCode
    nextAction = $nextAction
    envelopeState = $envelopeState
    sessionLedgerState = $sessionLedgerState
    habitationState = 'bounded-habitation-ready'
    habitationClass = 'bounded-recurring-local-habitation'
    readyConditionCount = 4
    withheldConditionCount = 3
    recurringWorkReady = $true
    returnLawBound = $true
    bondedReleaseDenied = $true
    publicationMaturityDenied = $true
    mosBearingDepthDenied = $true
    readinessProjectionBound = $readinessProjectionBound
    readinessKeyBound = $readinessKeyBound
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
    '# Runtime Habitation Readiness Ledger',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Readiness-ledger state: `{0}`' -f $payload.readinessLedgerState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Residency-envelope state: `{0}`' -f $(if ($payload.envelopeState) { $payload.envelopeState } else { 'missing' })),
    ('- Session-ledger state: `{0}`' -f $(if ($payload.sessionLedgerState) { $payload.sessionLedgerState } else { 'missing' })),
    ('- Habitation state: `{0}`' -f $payload.habitationState),
    ('- Habitation class: `{0}`' -f $payload.habitationClass),
    ('- Ready-condition count: `{0}`' -f $payload.readyConditionCount),
    ('- Withheld-condition count: `{0}`' -f $payload.withheldConditionCount),
    ('- Recurring work ready: `{0}`' -f [bool] $payload.recurringWorkReady),
    ('- Return law bound: `{0}`' -f [bool] $payload.returnLawBound),
    ('- Bonded release denied: `{0}`' -f [bool] $payload.bondedReleaseDenied),
    ('- Publication maturity denied: `{0}`' -f [bool] $payload.publicationMaturityDenied),
    ('- MoS-bearing depth denied: `{0}`' -f [bool] $payload.mosBearingDepthDenied),
    ('- Readiness projection bound: `{0}`' -f [bool] $payload.readinessProjectionBound),
    ('- Readiness key bound: `{0}`' -f [bool] $payload.readinessKeyBound),
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
    readinessLedgerState = $payload.readinessLedgerState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    envelopeState = $payload.envelopeState
    sessionLedgerState = $payload.sessionLedgerState
    habitationState = $payload.habitationState
    habitationClass = $payload.habitationClass
    readyConditionCount = $payload.readyConditionCount
    withheldConditionCount = $payload.withheldConditionCount
    recurringWorkReady = $payload.recurringWorkReady
    returnLawBound = $payload.returnLawBound
    bondedReleaseDenied = $payload.bondedReleaseDenied
    publicationMaturityDenied = $payload.publicationMaturityDenied
    mosBearingDepthDenied = $payload.mosBearingDepthDenied
    readinessProjectionBound = $payload.readinessProjectionBound
    serviceBindingBound = $payload.serviceBindingBound
    sourceFileCount = $payload.sourceFileCount
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[runtime-habitation-readiness-ledger] Bundle: {0}' -f $bundlePath)
$bundlePath
