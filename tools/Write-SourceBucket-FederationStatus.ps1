param(
    [string] $RepoRoot,
    [string] $FederationPolicyPath = 'OAN Mortalis V1.1.1/Automation/source-bucket-federation.json'
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

$automationCascadePromptHelperPath = Join-Path $PSScriptRoot 'Automation-CascadePrompt.ps1'
. $automationCascadePromptHelperPath
$oanWorkspaceResolverPath = Join-Path $PSScriptRoot 'Resolve-OanWorkspacePath.ps1'
if (Test-Path -LiteralPath $oanWorkspaceResolverPath -PathType Leaf) {
    . $oanWorkspaceResolverPath
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

function Read-JsonFile {
    param([string] $Path)

    return Get-Content -Raw -LiteralPath $Path | ConvertFrom-Json
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

function New-UniqueStringArray {
    param([object[]] $Values)

    $items = New-Object System.Collections.Generic.List[string]
    $seen = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)

    foreach ($value in @($Values)) {
        if ($null -eq $value) {
            continue
        }

        $stringValue = [string] $value
        if ([string]::IsNullOrWhiteSpace($stringValue)) {
            continue
        }

        if ($seen.Add($stringValue)) {
            [void] $items.Add($stringValue)
        }
    }

    return [string[]] $items.ToArray()
}

function Test-RequestStillActive {
    param([object] $RequestEntry)

    $requestState = [string] (Get-ObjectPropertyValueOrNull -InputObject $RequestEntry -PropertyName 'requestState')
    return $requestState -notin @(
        'admitted',
        'held',
        'withdrawn',
        'superseded',
        'returned',
        'closed'
    )
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedFederationPolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $FederationPolicyPath

$federationPolicy = Read-JsonFile -Path $resolvedFederationPolicyPath
$resolvedRequestContractPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $federationPolicy.requestContractPath)
$resolvedReturnContractPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $federationPolicy.returnContractPath)
$requestContract = Read-JsonFile -Path $resolvedRequestContractPath
$returnContract = Read-JsonFile -Path $resolvedReturnContractPath

$requestIndexStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $federationPolicy.requestIndexStatePath)
$federationStatusStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $federationPolicy.federationStatusStatePath)
$federationStatusMarkdownPath = [System.IO.Path]::ChangeExtension($federationStatusStatePath, '.md')
$requestIndex = Read-JsonFileOrNull -Path $requestIndexStatePath
$matrixPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $federationPolicy.versionedTouchPointMatrixPath)
$matrix = Read-JsonFile -Path $matrixPath

$touchPointGroups = @{}
foreach ($touchPointProperty in $matrix.touchPoints.PSObject.Properties) {
    $touchPointId = [string] $touchPointProperty.Name
    $touchPoint = $touchPointProperty.Value
    $touchPointStatus = [string] (Get-ObjectPropertyValueOrNull -InputObject $touchPoint -PropertyName 'status')
    $bucketLabel = [string] (Get-ObjectPropertyValueOrNull -InputObject $touchPoint -PropertyName 'researchBucketLabel')
    if ($touchPointStatus -ne 'research-handoff' -or [string]::IsNullOrWhiteSpace($bucketLabel)) {
        continue
    }

    if (-not $touchPointGroups.ContainsKey($bucketLabel)) {
        $touchPointGroups[$bucketLabel] = New-Object System.Collections.Generic.List[object]
    }

    [void] $touchPointGroups[$bucketLabel].Add([pscustomobject]@{
        touchPointId = $touchPointId
        researchReason = [string] (Get-ObjectPropertyValueOrNull -InputObject $touchPoint -PropertyName 'researchReason')
    })
}

$requestEntries = if ($null -ne $requestIndex) { @($requestIndex.requests) } else { @() }
$activeRequests = @($requestEntries | Where-Object { Test-RequestStillActive -RequestEntry $_ })
$activeRequestIds = @($activeRequests | ForEach-Object { [string] (Get-ObjectPropertyValueOrNull -InputObject $_ -PropertyName 'requestId') })

$bucketStates = foreach ($bucketLabel in @($federationPolicy.sourceBucketLabels)) {
    $bucketKey = [string] $bucketLabel
    $touchPoints = if ($touchPointGroups.ContainsKey($bucketKey)) { @($touchPointGroups[$bucketKey] | ForEach-Object { $_ }) } else { @() }
    $bucketRequests = @(
        $activeRequests | Where-Object {
            [string] (Get-ObjectPropertyValueOrNull -InputObject $_ -PropertyName 'targetBucketLabel') -eq $bucketKey
        }
    )
    $latestBucketRequest = @($bucketRequests | Sort-Object { [string] (Get-ObjectPropertyValueOrNull -InputObject $_ -PropertyName 'generatedAtUtc') } -Descending | Select-Object -First 1)

    $bucketState = 'no-build-need'
    if ($bucketRequests.Count -gt 0) {
        $bucketState = 'request-published-awaiting-return'
    } elseif ($touchPoints.Count -gt 0) {
        $bucketState = 'build-need-detected-request-missing'
    }

    [pscustomobject] [ordered]@{
        targetBucketLabel = $bucketKey
        bucketState = $bucketState
        listenerTelemetryState = 'research-lane-only'
        requestedTouchPointCount = @($touchPoints).Count
        requestedTouchPointIds = @($touchPoints | ForEach-Object { [string] $_.touchPointId })
        researchReasons = New-UniqueStringArray -Values @($touchPoints | ForEach-Object { [string] $_.researchReason })
        activeRequestCount = @($bucketRequests).Count
        latestRequestId = if ($latestBucketRequest.Count -gt 0) { [string] (Get-ObjectPropertyValueOrNull -InputObject $latestBucketRequest[0] -PropertyName 'requestId') } else { $null }
        latestBundlePath = if ($latestBucketRequest.Count -gt 0) { [string] (Get-ObjectPropertyValueOrNull -InputObject $latestBucketRequest[0] -PropertyName 'bundlePath') } else { $null }
        latestNeededReturnClass = if ($latestBucketRequest.Count -gt 0) { [string] (Get-ObjectPropertyValueOrNull -InputObject $latestBucketRequest[0] -PropertyName 'neededReturnClass') } else { $null }
    }
}

$totalResearchHandOffCount = 0
foreach ($bucketStateEntry in @($bucketStates)) {
    $totalResearchHandOffCount += [int] (Get-ObjectPropertyValueOrNull -InputObject $bucketStateEntry -PropertyName 'requestedTouchPointCount')
}

$federationState = 'idle-no-build-needs'
$reasonCode = 'source-bucket-federation-idle'
$nextAction = 'wait-for-research-handoff-touchpoints'
$requestPublicationGateState = 'bounded-requesting-open'
$gateActionClass = 'spec-now'

if ($activeRequests.Count -gt 0) {
    $federationState = 'requests-published-awaiting-returns'
    $reasonCode = 'source-bucket-federation-requests-published'
    $nextAction = 'wait-for-lawful-source-bucket-returns-or-operator-review'
} elseif ($totalResearchHandOffCount -gt 0) {
    $federationState = 'ready-to-publish-bounded-requests'
    $reasonCode = 'source-bucket-federation-build-need-open'
    $nextAction = 'publish-bounded-source-bucket-requests'
}

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    policyPath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $resolvedFederationPolicyPath
    formalSurfaceMarkdownPath = [string] $federationPolicy.formalSurfaceMarkdownPath
    requestContractPath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $resolvedRequestContractPath
    returnContractPath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $resolvedReturnContractPath
    federationState = $federationState
    reasonCode = $reasonCode
    nextAction = $nextAction
    requestPublicationGateState = $requestPublicationGateState
    gateActionClass = $gateActionClass
    requestOutboxRoot = [string] $federationPolicy.requestOutboxRoot
    requestIndexStatePath = [string] $federationPolicy.requestIndexStatePath
    requestIndexPresent = ($null -ne $requestIndex)
    activeRequestCount = $activeRequests.Count
    activeRequestIds = @($activeRequestIds)
    totalResearchHandOffCount = $totalResearchHandOffCount
    bucketStates = $bucketStates
    listenerStates = @($returnContract.listenerStates)
}
Add-AutomationCascadeOperatorPromptProperty -InputObject $payload | Out-Null
Add-BuildDispatchRootPromptProperty -InputObject $payload | Out-Null

Write-JsonFile -Path $federationStatusStatePath -Value $payload

$markdownLines = @(
    '# Source-Bucket Federation Status',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Federation state: `{0}`' -f $payload.federationState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Request publication gate state: `{0}`' -f $payload.requestPublicationGateState),
    ('- Gate action class: `{0}`' -f $payload.gateActionClass),
    ('- Active request count: `{0}`' -f $payload.activeRequestCount),
    ('- Total research handoff count: `{0}`' -f $payload.totalResearchHandOffCount),
    ''
)

foreach ($bucketState in @($bucketStates)) {
    $markdownLines += @(
        ('## {0}' -f [string] $bucketState.targetBucketLabel),
        '',
        ('- Bucket state: `{0}`' -f [string] $bucketState.bucketState),
        ('- Listener telemetry state: `{0}`' -f [string] $bucketState.listenerTelemetryState),
        ('- Requested touchpoints: `{0}`' -f [int] $bucketState.requestedTouchPointCount),
        ('- Active requests: `{0}`' -f [int] $bucketState.activeRequestCount),
        ('- Latest request id: `{0}`' -f $(if ($bucketState.latestRequestId) { [string] $bucketState.latestRequestId } else { 'none' })),
        ('- Latest bundle path: `{0}`' -f $(if ($bucketState.latestBundlePath) { [string] $bucketState.latestBundlePath } else { 'none' })),
        ('- Latest needed return class: `{0}`' -f $(if ($bucketState.latestNeededReturnClass) { [string] $bucketState.latestNeededReturnClass } else { 'none' })),
        ('- Requested touchpoint ids: `{0}`' -f $(if (@($bucketState.requestedTouchPointIds).Count -gt 0) { (@($bucketState.requestedTouchPointIds) -join '`, `') } else { 'none' })),
        ('- Research reasons: `{0}`' -f $(if (@($bucketState.researchReasons).Count -gt 0) { (@($bucketState.researchReasons) -join '; ') } else { 'none' })),
        ''
    )
}

$markdownLines = Add-AutomationCascadePromptMarkdownLines -MarkdownLines $markdownLines
Set-Content -LiteralPath $federationStatusMarkdownPath -Value $markdownLines -Encoding utf8

Write-Host ('[source-bucket-federation] State: {0}' -f $federationStatusStatePath)
$federationStatusStatePath
