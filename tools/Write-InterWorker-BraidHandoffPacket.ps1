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
$governedThreadBirthReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.governedThreadBirthReceiptStatePath)
$duplexPredicateEnvelopeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.duplexPredicateEnvelopeStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.interWorkerBraidHandoffPacketOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.interWorkerBraidHandoffPacketStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the inter-worker braid-handoff packet writer can run.'
}

$governedThreadBirthReceiptState = Read-JsonFileOrNull -Path $governedThreadBirthReceiptStatePath
$duplexPredicateEnvelopeState = Read-JsonFileOrNull -Path $duplexPredicateEnvelopeStatePath

$threadBirthState = [string] (Get-ObjectPropertyValueOrNull -InputObject $governedThreadBirthReceiptState -PropertyName 'receiptState')
$duplexState = [string] (Get-ObjectPropertyValueOrNull -InputObject $duplexPredicateEnvelopeState -PropertyName 'duplexState')

$sourceFiles = @(
    (Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath 'OAN Mortalis V1.0/src/Oan.Common/WorkerThreadGovernanceContracts.cs')
    (Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath 'OAN Mortalis V1.0/src/AgentiCore/Services/GovernedWorkerThreadService.cs')
)
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$braidPacketProjectionBound = $contractsSource.IndexOf('CreateInterWorkerBraidHandoffPacket', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateInterWorkerBraidHandoff', [System.StringComparison]::Ordinal) -ge 0

$packetState = 'awaiting-governed-thread-birth'
$reasonCode = 'inter-worker-braid-handoff-packet-awaiting-governed-thread-birth'
$nextAction = 'emit-governed-thread-birth-receipt'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $packetState = 'blocked'
    $reasonCode = 'inter-worker-braid-handoff-packet-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($threadBirthState -ne 'thread-birth-ready') {
    $packetState = 'awaiting-governed-thread-birth'
    $reasonCode = 'inter-worker-braid-handoff-packet-thread-birth-not-ready'
    $nextAction = if ($null -ne $governedThreadBirthReceiptState) { [string] $governedThreadBirthReceiptState.nextAction } else { 'emit-governed-thread-birth-receipt' }
} elseif ($duplexState -ne 'duplex-envelope-ready') {
    $packetState = 'awaiting-duplex-envelope'
    $reasonCode = 'inter-worker-braid-handoff-packet-duplex-not-ready'
    $nextAction = if ($null -ne $duplexPredicateEnvelopeState) { [string] $duplexPredicateEnvelopeState.nextAction } else { 'emit-duplex-predicate-envelope' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $braidPacketProjectionBound -or -not $serviceBindingBound) {
    $packetState = 'awaiting-braid-binding'
    $reasonCode = 'inter-worker-braid-handoff-packet-source-missing'
    $nextAction = 'bind-explicit-braid-handoff-surface'
} else {
    $packetState = 'braid-handoff-ready'
    $reasonCode = 'inter-worker-braid-handoff-explicit'
    $nextAction = 'pull-forward-to-map-20'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'inter-worker-braid-handoff-packet.json'
$bundleMarkdownPath = Join-Path $bundlePath 'inter-worker-braid-handoff-packet.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    packetState = $packetState
    reasonCode = $reasonCode
    nextAction = $nextAction
    threadBirthState = $threadBirthState
    duplexState = $duplexState
    predicateContextClass = 'duplex-predicate-bridged-context'
    identityInheritanceDenied = $true
    ambientSharedIdentityDenied = $true
    withheldIdentityHandleClass = 'thread-root-and-identity-invariant'
    braidPacketProjectionBound = $braidPacketProjectionBound
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
    '# Inter-Worker Braid Handoff Packet',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Packet state: `{0}`' -f $payload.packetState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Thread-birth state: `{0}`' -f $(if ($payload.threadBirthState) { $payload.threadBirthState } else { 'missing' })),
    ('- Duplex state: `{0}`' -f $(if ($payload.duplexState) { $payload.duplexState } else { 'missing' })),
    ('- Predicate context class: `{0}`' -f $payload.predicateContextClass),
    ('- Identity inheritance denied: `{0}`' -f [bool] $payload.identityInheritanceDenied),
    ('- Ambient shared identity denied: `{0}`' -f [bool] $payload.ambientSharedIdentityDenied),
    ('- Withheld identity-handle class: `{0}`' -f $payload.withheldIdentityHandleClass),
    ('- Braid-packet projection bound: `{0}`' -f [bool] $payload.braidPacketProjectionBound),
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
    packetState = $payload.packetState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    threadBirthState = $payload.threadBirthState
    duplexState = $payload.duplexState
    identityInheritanceDenied = $payload.identityInheritanceDenied
    ambientSharedIdentityDenied = $payload.ambientSharedIdentityDenied
    sourceFileCount = $payload.sourceFileCount
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[inter-worker-braid-handoff-packet] Bundle: {0}' -f $bundlePath)
$bundlePath
