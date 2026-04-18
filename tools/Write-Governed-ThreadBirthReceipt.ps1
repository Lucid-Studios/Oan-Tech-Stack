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
$identityInvariantThreadRootStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.identityInvariantThreadRootStatePath)
$nexusSingularPortalFacadeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.nexusSingularPortalFacadeStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.governedThreadBirthReceiptOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.governedThreadBirthReceiptStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the governed thread-birth receipt writer can run.'
}

$identityInvariantThreadRootState = Read-JsonFileOrNull -Path $identityInvariantThreadRootStatePath
$nexusSingularPortalFacadeState = Read-JsonFileOrNull -Path $nexusSingularPortalFacadeStatePath

$threadRootState = [string] (Get-ObjectPropertyValueOrNull -InputObject $identityInvariantThreadRootState -PropertyName 'threadRootState')
$nexusPortalState = [string] (Get-ObjectPropertyValueOrNull -InputObject $nexusSingularPortalFacadeState -PropertyName 'portalState')

$sourceFiles = Resolve-OanWorkspaceTouchPointFamily -BasePath $resolvedRepoRoot -FamilyName 'worker-thread-birth' -CyclePolicy $cyclePolicy
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$governanceSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$threadBirthProjectionBound = $contractsSource.IndexOf('CreateGovernedThreadBirthReceipt', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateGovernedThreadBirth', [System.StringComparison]::Ordinal) -ge 0
$triadicWitnessLawPresent = $governanceSource.IndexOf('InternalGoverningCmeOffice.Mother', [System.StringComparison]::Ordinal) -ge 0 -and
    $governanceSource.IndexOf('InternalGoverningCmeOffice.Father', [System.StringComparison]::Ordinal) -ge 0 -and
    $governanceSource.IndexOf('InternalGoverningCmeOffice.Steward', [System.StringComparison]::Ordinal) -ge 0

$receiptState = 'awaiting-identity-thread-root'
$reasonCode = 'governed-thread-birth-receipt-awaiting-identity-thread-root'
$nextAction = 'emit-identity-invariant-thread-root'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $receiptState = 'blocked'
    $reasonCode = 'governed-thread-birth-receipt-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($threadRootState -ne 'identity-thread-root-ready') {
    $receiptState = 'awaiting-identity-thread-root'
    $reasonCode = 'governed-thread-birth-receipt-thread-root-not-ready'
    $nextAction = if ($null -ne $identityInvariantThreadRootState) { [string] $identityInvariantThreadRootState.nextAction } else { 'emit-identity-invariant-thread-root' }
} elseif ($nexusPortalState -ne 'portal-facade-ready') {
    $receiptState = 'awaiting-singular-portal'
    $reasonCode = 'governed-thread-birth-receipt-portal-not-ready'
    $nextAction = if ($null -ne $nexusSingularPortalFacadeState) { [string] $nexusSingularPortalFacadeState.nextAction } else { 'emit-nexus-singular-portal-facade' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $threadBirthProjectionBound -or -not $serviceBindingBound -or -not $triadicWitnessLawPresent) {
    $receiptState = 'awaiting-thread-birth-binding'
    $reasonCode = 'governed-thread-birth-receipt-source-missing'
    $nextAction = 'bind-governed-thread-birth-surface'
} else {
    $receiptState = 'thread-birth-ready'
    $reasonCode = 'governed-thread-birth-triadic-bound'
    $nextAction = 'emit-inter-worker-braid-handoff-packet'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'governed-thread-birth-receipt.json'
$bundleMarkdownPath = Join-Path $bundlePath 'governed-thread-birth-receipt.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    receiptState = $receiptState
    reasonCode = $reasonCode
    nextAction = $nextAction
    threadRootState = $threadRootState
    nexusPortalState = $nexusPortalState
    triadicWitnessRequired = $true
    witnessedOffices = @('Steward', 'Father', 'Mother')
    movementBeginsGoverned = $true
    threadBirthProjectionBound = $threadBirthProjectionBound
    serviceBindingBound = $serviceBindingBound
    triadicWitnessLawPresent = $triadicWitnessLawPresent
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
    '# Governed Thread Birth Receipt',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Receipt state: `{0}`' -f $payload.receiptState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Thread-root state: `{0}`' -f $(if ($payload.threadRootState) { $payload.threadRootState } else { 'missing' })),
    ('- Nexus portal state: `{0}`' -f $(if ($payload.nexusPortalState) { $payload.nexusPortalState } else { 'missing' })),
    ('- Triadic witness required: `{0}`' -f [bool] $payload.triadicWitnessRequired),
    ('- Witnessed offices: `{0}`' -f ($payload.witnessedOffices -join '`, `')),
    ('- Movement begins governed: `{0}`' -f [bool] $payload.movementBeginsGoverned),
    ('- Thread-birth projection bound: `{0}`' -f [bool] $payload.threadBirthProjectionBound),
    ('- Service binding bound: `{0}`' -f [bool] $payload.serviceBindingBound),
    ('- Triadic witness law present: `{0}`' -f [bool] $payload.triadicWitnessLawPresent),
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
    receiptState = $payload.receiptState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    threadRootState = $payload.threadRootState
    nexusPortalState = $payload.nexusPortalState
    witnessedOfficeCount = @($payload.witnessedOffices).Count
    threadBirthProjectionBound = $payload.threadBirthProjectionBound
    serviceBindingBound = $payload.serviceBindingBound
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[governed-thread-birth-receipt] Bundle: {0}' -f $bundlePath)
$bundlePath
