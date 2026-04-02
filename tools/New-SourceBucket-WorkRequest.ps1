param(
    [Parameter(Mandatory = $true)]
    [string] $TargetBucketLabel,
    [Parameter(Mandatory = $true)]
    [string] $BuildSurface,
    [Parameter(Mandatory = $true)]
    [string] $Subject,
    [Parameter(Mandatory = $true)]
    [string] $Predicate,
    [Parameter(Mandatory = $true)]
    [string[]] $Actions,
    [string] $NeededReturnClass = 'spec-now',
    [string[]] $EvidenceLinks,
    [string] $AdmissibilityClass = 'review_required',
    [string[]] $RequiredReceipts,
    [string] $HitlState = 'required_before_promotion',
    [string[]] $WithholdRules,
    [string[]] $SourceTouchPointIds,
    [string] $RequestKey,
    [string] $RepoRoot,
    [string] $RequestContractPath = 'OAN Mortalis V1.1.1/build/source-bucket-work-request-contract.json'
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

function New-Slug {
    param([string] $Value)

    $collapsed = [regex]::Replace($Value.ToLowerInvariant(), '[^a-z0-9]+', '-')
    return $collapsed.Trim('-')
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
$resolvedRequestContractPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $RequestContractPath
$requestContract = Read-JsonFile -Path $resolvedRequestContractPath
$resolvedFederationPolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $requestContract.sourceBucketFederationPolicyPath)
$federationPolicy = Read-JsonFile -Path $resolvedFederationPolicyPath

if (@($requestContract.targetBucketLabels) -notcontains $TargetBucketLabel) {
    throw ('Unknown target bucket label: {0}' -f $TargetBucketLabel)
}

if (@($requestContract.neededReturnClasses) -notcontains $NeededReturnClass) {
    throw ('Unknown needed return class: {0}' -f $NeededReturnClass)
}

if (@($requestContract.admissibilityClasses) -notcontains $AdmissibilityClass) {
    throw ('Unknown admissibility class: {0}' -f $AdmissibilityClass)
}

if (@($requestContract.hitlStates) -notcontains $HitlState) {
    throw ('Unknown HITL state: {0}' -f $HitlState)
}

$effectiveRequiredReceipts = if ($PSBoundParameters.ContainsKey('RequiredReceipts') -and @($RequiredReceipts).Count -gt 0) {
    New-UniqueStringArray -Values $RequiredReceipts
} else {
    New-UniqueStringArray -Values @($requestContract.requiredReceipts)
}

$expectedReceipts = New-UniqueStringArray -Values @($requestContract.requiredReceipts)
foreach ($receiptId in $expectedReceipts) {
    if ($effectiveRequiredReceipts -notcontains $receiptId) {
        throw ('Required receipt `{0}` is missing from the request.' -f $receiptId)
    }
}

$effectiveEvidenceLinks = New-UniqueStringArray -Values $EvidenceLinks
$effectiveWithholdRules = New-UniqueStringArray -Values $WithholdRules
$effectiveTouchPointIds = New-UniqueStringArray -Values $SourceTouchPointIds
$effectiveActions = New-UniqueStringArray -Values $Actions

if (@($effectiveActions).Count -eq 0) {
    throw 'At least one action is required for a source-bucket work request.'
}

if ([string]::IsNullOrWhiteSpace($RequestKey)) {
    $RequestKey = '{0}|{1}|{2}|{3}' -f $TargetBucketLabel, $BuildSurface, $NeededReturnClass, (@($effectiveTouchPointIds) -join ',')
}

$requestOutboxRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $federationPolicy.requestOutboxRoot)
$requestIndexStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $federationPolicy.requestIndexStatePath)
$existingIndex = Read-JsonFileOrNull -Path $requestIndexStatePath

$existingRequests = @()
if ($null -ne $existingIndex) {
    $existingRequestProperty = Get-ObjectPropertyValueOrNull -InputObject $existingIndex -PropertyName 'requests'
    if ($null -ne $existingRequestProperty) {
        $existingRequests = @($existingRequestProperty)
    }
}

$existingActiveRequest = $existingRequests |
    Where-Object {
        ([string] (Get-ObjectPropertyValueOrNull -InputObject $_ -PropertyName 'requestKey') -eq $RequestKey) -and
        (Test-RequestStillActive -RequestEntry $_)
    } |
    Sort-Object { [string] (Get-ObjectPropertyValueOrNull -InputObject $_ -PropertyName 'generatedAtUtc') } -Descending |
    Select-Object -First 1

if ($null -ne $existingActiveRequest) {
    $existingBundleRelativePath = [string] (Get-ObjectPropertyValueOrNull -InputObject $existingActiveRequest -PropertyName 'bundlePath')
    if (-not [string]::IsNullOrWhiteSpace($existingBundleRelativePath)) {
        $existingBundlePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $existingBundleRelativePath
        if (Test-Path -LiteralPath $existingBundlePath -PathType Container) {
            Write-Host ('[source-bucket-work-request] Existing: {0}' -f $existingBundlePath)
            $existingBundlePath
            return
        }
    }
}

$bucketSlug = New-Slug -Value $TargetBucketLabel
$bucketRequestCount = @(
    $existingRequests | Where-Object {
        [string] (Get-ObjectPropertyValueOrNull -InputObject $_ -PropertyName 'targetBucketLabel') -eq $TargetBucketLabel
    }
).Count
$sequenceNumber = $bucketRequestCount + 1
$dateStamp = (Get-Date).ToUniversalTime().ToString('yyyy-MM-dd')
$requestIdLeaf = '{0}-{1}' -f $bucketSlug, $sequenceNumber.ToString('000')
$requestId = 'source-bucket-request://{0}/{1}' -f $dateStamp, $requestIdLeaf
$bundleDirectoryName = '{0}-{1}' -f $dateStamp, $requestIdLeaf
$bundlePath = Join-Path $requestOutboxRoot $bundleDirectoryName
$requestJsonPath = Join-Path $bundlePath 'request.json'
$requestMarkdownPath = Join-Path $bundlePath 'request.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    requestId = $requestId
    requestKey = $RequestKey
    targetBucketLabel = $TargetBucketLabel
    buildSurface = $BuildSurface
    subject = $Subject
    predicate = $Predicate
    actions = @($effectiveActions)
    neededReturnClass = $NeededReturnClass
    evidenceLinks = @($effectiveEvidenceLinks)
    admissibilityClass = $AdmissibilityClass
    requiredReceipts = @($effectiveRequiredReceipts)
    hitlState = $HitlState
    withholdRules = @($effectiveWithholdRules)
    requestState = 'published-awaiting-return'
    sourceTouchPointIds = @($effectiveTouchPointIds)
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    requestContractPath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $resolvedRequestContractPath
    federationPolicyPath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $resolvedFederationPolicyPath
    completionRule = [string] $requestContract.completionRule
}
Add-AutomationCascadeOperatorPromptProperty -InputObject $payload | Out-Null
Add-BuildDispatchRootPromptProperty -InputObject $payload | Out-Null

Write-JsonFile -Path $requestJsonPath -Value $payload

$markdownLines = @(
    '# Source-Bucket Work Request',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Request id: `{0}`' -f $payload.requestId),
    ('- Target bucket: `{0}`' -f $payload.targetBucketLabel),
    ('- Build surface: `{0}`' -f $payload.buildSurface),
    ('- Request state: `{0}`' -f $payload.requestState),
    ('- Needed return class: `{0}`' -f $payload.neededReturnClass),
    ('- Admissibility class: `{0}`' -f $payload.admissibilityClass),
    ('- HITL state: `{0}`' -f $payload.hitlState),
    ('- Required receipts: `{0}`' -f ((@($payload.requiredReceipts) -join '`, `'))),
    ('- Subject: `{0}`' -f $payload.subject),
    ('- Predicate: `{0}`' -f $payload.predicate),
    ('- Actions: `{0}`' -f ((@($payload.actions) -join '; '))),
    ('- Evidence links: `{0}`' -f $(if (@($payload.evidenceLinks).Count -gt 0) { (@($payload.evidenceLinks) -join '`, `') } else { 'none' })),
    ('- Source touchpoints: `{0}`' -f $(if (@($payload.sourceTouchPointIds).Count -gt 0) { (@($payload.sourceTouchPointIds) -join '`, `') } else { 'none' })),
    ('- Withhold rules: `{0}`' -f $(if (@($payload.withholdRules).Count -gt 0) { (@($payload.withholdRules) -join '; ') } else { 'none' }))
)
$markdownLines = Add-AutomationCascadePromptMarkdownLines -MarkdownLines $markdownLines
Set-Content -LiteralPath $requestMarkdownPath -Value $markdownLines -Encoding utf8

$requestEntries = @($existingRequests)
$requestEntries += [pscustomobject]@{
    requestId = $payload.requestId
    requestKey = $payload.requestKey
    generatedAtUtc = $payload.generatedAtUtc
    targetBucketLabel = $payload.targetBucketLabel
    buildSurface = $payload.buildSurface
    subject = $payload.subject
    predicate = $payload.predicate
    actions = @($payload.actions)
    neededReturnClass = $payload.neededReturnClass
    admissibilityClass = $payload.admissibilityClass
    hitlState = $payload.hitlState
    requestState = $payload.requestState
    sourceTouchPointIds = @($payload.sourceTouchPointIds)
    bundlePath = $payload.bundlePath
}

$bucketSummaries = foreach ($bucketLabel in @($requestContract.targetBucketLabels)) {
    $bucketRequests = @(
        $requestEntries | Where-Object {
            [string] (Get-ObjectPropertyValueOrNull -InputObject $_ -PropertyName 'targetBucketLabel') -eq $bucketLabel
        }
    )
    $activeBucketRequests = @($bucketRequests | Where-Object { Test-RequestStillActive -RequestEntry $_ })
    $latestBucketRequest = @($bucketRequests | Sort-Object { [string] (Get-ObjectPropertyValueOrNull -InputObject $_ -PropertyName 'generatedAtUtc') } -Descending | Select-Object -First 1)

    [ordered]@{
        targetBucketLabel = $bucketLabel
        totalRequestCount = $bucketRequests.Count
        activeRequestCount = $activeBucketRequests.Count
        latestRequestId = if ($latestBucketRequest.Count -gt 0) { [string] (Get-ObjectPropertyValueOrNull -InputObject $latestBucketRequest[0] -PropertyName 'requestId') } else { $null }
        latestBundlePath = if ($latestBucketRequest.Count -gt 0) { [string] (Get-ObjectPropertyValueOrNull -InputObject $latestBucketRequest[0] -PropertyName 'bundlePath') } else { $null }
    }
}

$indexPayload = [pscustomobject]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    policyPath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $resolvedFederationPolicyPath
    requestContractPath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $resolvedRequestContractPath
    lastRequestId = $payload.requestId
    lastRequestBundle = $payload.bundlePath
    activeRequestCount = @($requestEntries | Where-Object { Test-RequestStillActive -RequestEntry $_ }).Count
    publishedRequestCount = $requestEntries.Count
    activeRequestIds = @(
        $requestEntries |
            Where-Object { Test-RequestStillActive -RequestEntry $_ } |
            ForEach-Object { [string] (Get-ObjectPropertyValueOrNull -InputObject $_ -PropertyName 'requestId') }
    )
    bucketSummaries = $bucketSummaries
    requests = @($requestEntries)
}
Add-AutomationCascadeOperatorPromptProperty -InputObject $indexPayload | Out-Null
Add-BuildDispatchRootPromptProperty -InputObject $indexPayload | Out-Null

Write-JsonFile -Path $requestIndexStatePath -Value $indexPayload
Write-Host ('[source-bucket-work-request] Request: {0}' -f $bundlePath)
$bundlePath
