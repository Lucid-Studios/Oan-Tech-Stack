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
$reachDuplexRealizationSeamStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.reachDuplexRealizationSeamStatePath)
$operatorActualWorkSessionRehearsalStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.operatorActualWorkSessionRehearsalStatePath)
$bondedOperatorLocalityReadinessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.bondedOperatorLocalityReadinessStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.bondedParticipationLocalityLedgerOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.bondedParticipationLocalityLedgerStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the bonded participation locality-ledger writer can run.'
}

$reachDuplexRealizationSeamState = Read-JsonFileOrNull -Path $reachDuplexRealizationSeamStatePath
$operatorActualWorkSessionRehearsalState = Read-JsonFileOrNull -Path $operatorActualWorkSessionRehearsalStatePath
$bondedOperatorLocalityReadinessState = Read-JsonFileOrNull -Path $bondedOperatorLocalityReadinessStatePath

$seamState = [string] (Get-ObjectPropertyValueOrNull -InputObject $reachDuplexRealizationSeamState -PropertyName 'seamState')
$rehearsalState = [string] (Get-ObjectPropertyValueOrNull -InputObject $operatorActualWorkSessionRehearsalState -PropertyName 'rehearsalState')
$operatorLocalityState = [string] (Get-ObjectPropertyValueOrNull -InputObject $bondedOperatorLocalityReadinessState -PropertyName 'readinessState')

$sourceFiles = Resolve-OanWorkspaceTouchPointFamily -BasePath $resolvedRepoRoot -FamilyName 'agenticore-actual-pair' -CyclePolicy $cyclePolicy
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$ledgerProjectionBound = $contractsSource.IndexOf('CreateBondedParticipationLocalityLedger', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('bonded-participation-locality-ledger-bound', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateBondedParticipationLocalityLedger', [System.StringComparison]::Ordinal) -ge 0

$ledgerState = 'awaiting-reach-duplex-realization'
$reasonCode = 'bonded-participation-locality-ledger-awaiting-reach-duplex-realization'
$nextAction = 'emit-reach-duplex-realization-seam'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $ledgerState = 'blocked'
    $reasonCode = 'bonded-participation-locality-ledger-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($seamState -ne 'reach-duplex-realization-ready') {
    $ledgerState = 'awaiting-reach-duplex-realization'
    $reasonCode = 'bonded-participation-locality-ledger-seam-not-ready'
    $nextAction = if ($null -ne $reachDuplexRealizationSeamState) { [string] $reachDuplexRealizationSeamState.nextAction } else { 'emit-reach-duplex-realization-seam' }
} elseif ($rehearsalState -ne 'rehearsal-bundle-ready') {
    $ledgerState = 'awaiting-bounded-operator-rehearsal'
    $reasonCode = 'bonded-participation-locality-ledger-rehearsal-not-ready'
    $nextAction = if ($null -ne $operatorActualWorkSessionRehearsalState) { [string] $operatorActualWorkSessionRehearsalState.nextAction } else { 'emit-operator-actual-work-session-rehearsal' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $ledgerProjectionBound -or -not $serviceBindingBound) {
    $ledgerState = 'awaiting-bonded-locality-ledger-binding'
    $reasonCode = 'bonded-participation-locality-ledger-source-missing'
    $nextAction = 'bind-bonded-participation-locality-ledger'
} else {
    $ledgerState = 'bonded-locality-ledger-ready'
    $reasonCode = 'bonded-participation-locality-ledger-bound'
    $nextAction = 'pull-forward-to-map-21'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'bonded-participation-locality-ledger.json'
$bundleMarkdownPath = Join-Path $bundlePath 'bonded-participation-locality-ledger.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    ledgerState = $ledgerState
    reasonCode = $reasonCode
    nextAction = $nextAction
    reachSeamState = $seamState
    operatorRehearsalState = $rehearsalState
    operatorLocalityState = $operatorLocalityState
    bondedParticipationProvisional = $true
    remoteControlDenied = $true
    coRealizedSurfaceCount = 3
    withheldSurfaceCount = 3
    ledgerProjectionBound = $ledgerProjectionBound
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
    '# Bonded Participation Locality Ledger',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Ledger state: `{0}`' -f $payload.ledgerState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Reach seam state: `{0}`' -f $(if ($payload.reachSeamState) { $payload.reachSeamState } else { 'missing' })),
    ('- Operator rehearsal state: `{0}`' -f $(if ($payload.operatorRehearsalState) { $payload.operatorRehearsalState } else { 'missing' })),
    ('- Operator locality state: `{0}`' -f $(if ($payload.operatorLocalityState) { $payload.operatorLocalityState } else { 'missing' })),
    ('- Bonded participation provisional: `{0}`' -f [bool] $payload.bondedParticipationProvisional),
    ('- Remote control denied: `{0}`' -f [bool] $payload.remoteControlDenied),
    ('- Co-realized surfaces: `{0}`' -f $payload.coRealizedSurfaceCount),
    ('- Withheld surfaces: `{0}`' -f $payload.withheldSurfaceCount),
    ('- Ledger projection bound: `{0}`' -f [bool] $payload.ledgerProjectionBound),
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
    ledgerState = $payload.ledgerState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    reachSeamState = $payload.reachSeamState
    operatorRehearsalState = $payload.operatorRehearsalState
    operatorLocalityState = $payload.operatorLocalityState
    bondedParticipationProvisional = $payload.bondedParticipationProvisional
    remoteControlDenied = $payload.remoteControlDenied
    coRealizedSurfaceCount = $payload.coRealizedSurfaceCount
    withheldSurfaceCount = $payload.withheldSurfaceCount
    ledgerProjectionBound = $payload.ledgerProjectionBound
    serviceBindingBound = $payload.serviceBindingBound
    sourceFileCount = $payload.sourceFileCount
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[bonded-participation-locality-ledger] Bundle: {0}' -f $bundlePath)
$bundlePath
