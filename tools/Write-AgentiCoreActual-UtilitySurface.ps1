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
$governedThreadBirthReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.governedThreadBirthReceiptStatePath)
$duplexPredicateEnvelopeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.duplexPredicateEnvelopeStatePath)
$nexusSingularPortalFacadeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.nexusSingularPortalFacadeStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.agentiCoreActualUtilitySurfaceOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.agentiCoreActualUtilitySurfaceStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the AgentiCore.actual utility-surface writer can run.'
}

$governedThreadBirthReceiptState = Read-JsonFileOrNull -Path $governedThreadBirthReceiptStatePath
$duplexPredicateEnvelopeState = Read-JsonFileOrNull -Path $duplexPredicateEnvelopeStatePath
$nexusSingularPortalFacadeState = Read-JsonFileOrNull -Path $nexusSingularPortalFacadeStatePath

$threadBirthState = [string] (Get-ObjectPropertyValueOrNull -InputObject $governedThreadBirthReceiptState -PropertyName 'receiptState')
$duplexState = [string] (Get-ObjectPropertyValueOrNull -InputObject $duplexPredicateEnvelopeState -PropertyName 'duplexState')
$portalState = [string] (Get-ObjectPropertyValueOrNull -InputObject $nexusSingularPortalFacadeState -PropertyName 'portalState')

$sourceFiles = @(
    (Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath 'OAN Mortalis V1.0/src/Oan.Common/AgentiActualizationContracts.cs'),
    (Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath 'OAN Mortalis V1.0/src/Oan.Common/AgentiActualizationKeys.cs'),
    (Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath 'OAN Mortalis V1.0/src/AgentiCore/Services/GovernedReachRealizationService.cs'),
    (Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath 'OAN Mortalis V1.0/src/AgentiCore.Runtime/AgentiRuntime.cs')
)
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$keysSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$runtimeSource = if (Test-Path -LiteralPath $sourceFiles[3] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[3] } else { '' }
$utilityProjectionBound = $contractsSource.IndexOf('CreateAgentiActualUtilitySurface', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('agenticore-actual-utility-surface-bound', [System.StringComparison]::Ordinal) -ge 0
$utilityKeyBound = $keysSource.IndexOf('CreateAgentiActualUtilitySurfaceHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateAgentiActualUtilitySurface', [System.StringComparison]::Ordinal) -ge 0
$runtimeSurfacePresent = $runtimeSource.IndexOf('SendDuplexIntentAsync', [System.StringComparison]::Ordinal) -ge 0

$utilityState = 'awaiting-governed-thread-birth'
$reasonCode = 'agenticore-actual-utility-awaiting-governed-thread-birth'
$nextAction = 'emit-governed-thread-birth-receipt'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $utilityState = 'blocked'
    $reasonCode = 'agenticore-actual-utility-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($threadBirthState -ne 'thread-birth-ready') {
    $utilityState = 'awaiting-governed-thread-birth'
    $reasonCode = 'agenticore-actual-utility-thread-birth-not-ready'
    $nextAction = if ($null -ne $governedThreadBirthReceiptState) { [string] $governedThreadBirthReceiptState.nextAction } else { 'emit-governed-thread-birth-receipt' }
} elseif ($duplexState -ne 'duplex-envelope-ready') {
    $utilityState = 'awaiting-duplex-envelope'
    $reasonCode = 'agenticore-actual-utility-duplex-not-ready'
    $nextAction = if ($null -ne $duplexPredicateEnvelopeState) { [string] $duplexPredicateEnvelopeState.nextAction } else { 'emit-duplex-predicate-envelope' }
} elseif ($portalState -ne 'portal-facade-ready') {
    $utilityState = 'awaiting-singular-portal'
    $reasonCode = 'agenticore-actual-utility-portal-not-ready'
    $nextAction = if ($null -ne $nexusSingularPortalFacadeState) { [string] $nexusSingularPortalFacadeState.nextAction } else { 'emit-nexus-singular-portal-facade' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $utilityProjectionBound -or -not $utilityKeyBound -or -not $serviceBindingBound -or -not $runtimeSurfacePresent) {
    $utilityState = 'awaiting-agenticore-actual-binding'
    $reasonCode = 'agenticore-actual-utility-source-missing'
    $nextAction = 'bind-agenticore-actual-utility-surface'
} else {
    $utilityState = 'agenticore-actual-utility-ready'
    $reasonCode = 'agenticore-actual-utility-surface-bound'
    $nextAction = 'emit-reach-duplex-realization-seam'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'agenticore-actual-utility-surface.json'
$bundleMarkdownPath = Join-Path $bundlePath 'agenticore-actual-utility-surface.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    utilityState = $utilityState
    reasonCode = $reasonCode
    nextAction = $nextAction
    threadBirthState = $threadBirthState
    duplexState = $duplexState
    portalState = $portalState
    utilityPosture = 'governed-utility-virtualized'
    sovereigntyDenied = $true
    remoteControlDenied = $true
    utilityProjectionBound = $utilityProjectionBound
    utilityKeyBound = $utilityKeyBound
    serviceBindingBound = $serviceBindingBound
    runtimeSurfacePresent = $runtimeSurfacePresent
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
    '# AgentiCore.actual Utility Surface',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Utility state: `{0}`' -f $payload.utilityState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Thread-birth state: `{0}`' -f $(if ($payload.threadBirthState) { $payload.threadBirthState } else { 'missing' })),
    ('- Duplex state: `{0}`' -f $(if ($payload.duplexState) { $payload.duplexState } else { 'missing' })),
    ('- Portal state: `{0}`' -f $(if ($payload.portalState) { $payload.portalState } else { 'missing' })),
    ('- Utility posture: `{0}`' -f $payload.utilityPosture),
    ('- Sovereignty denied: `{0}`' -f [bool] $payload.sovereigntyDenied),
    ('- Remote control denied: `{0}`' -f [bool] $payload.remoteControlDenied),
    ('- Utility projection bound: `{0}`' -f [bool] $payload.utilityProjectionBound),
    ('- Utility key bound: `{0}`' -f [bool] $payload.utilityKeyBound),
    ('- Service binding bound: `{0}`' -f [bool] $payload.serviceBindingBound),
    ('- Runtime surface present: `{0}`' -f [bool] $payload.runtimeSurfacePresent),
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
    utilityState = $payload.utilityState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    threadBirthState = $payload.threadBirthState
    duplexState = $payload.duplexState
    portalState = $payload.portalState
    utilityPosture = $payload.utilityPosture
    utilityProjectionBound = $payload.utilityProjectionBound
    serviceBindingBound = $payload.serviceBindingBound
    runtimeSurfacePresent = $payload.runtimeSurfacePresent
    sourceFileCount = $payload.sourceFileCount
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[agenticore-actual-utility-surface] Bundle: {0}' -f $bundlePath)
$bundlePath
