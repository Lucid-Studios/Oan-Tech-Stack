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

function Test-EnumMembership {
    param(
        [string] $Value,
        [object[]] $AllowedValues
    )

    return @($AllowedValues) -contains $Value
}

function Test-JsonPropertyPresent {
    param(
        [object] $InputObject,
        [string] $PropertyName
    )

    if ($null -eq $InputObject) {
        return $false
    }

    return $null -ne $InputObject.PSObject.Properties[$PropertyName]
}

function Get-TriadRefValue {
    param(
        [object] $TriadRefs,
        [string[]] $CandidateNames
    )

    foreach ($candidateName in @($CandidateNames)) {
        $value = Get-ObjectPropertyValueOrNull -InputObject $TriadRefs -PropertyName $candidateName
        if (-not [string]::IsNullOrWhiteSpace([string] $value)) {
            return [string] $value
        }
    }

    return $null
}

function Get-RequestLifecycleStateFromReturn {
    param(
        [string] $ReturnStanding,
        [string] $IntegrationDisposition
    )

    switch ($ReturnStanding) {
        'admitted-for-build-review' { return 'returned' }
        'held-or-escalated' { return 'held' }
        'invalid-return' { return 'held' }
        default { return 'published-awaiting-return' }
    }
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedFederationPolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $FederationPolicyPath

$federationPolicy = Read-JsonFile -Path $resolvedFederationPolicyPath
$resolvedRequestContractPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $federationPolicy.requestContractPath)
$resolvedReturnContractPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $federationPolicy.returnContractPath)
$requestContract = Read-JsonFile -Path $resolvedRequestContractPath
$returnContract = Read-JsonFile -Path $resolvedReturnContractPath

$returnInboxRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $federationPolicy.returnInboxRoot)
$returnIndexStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $federationPolicy.returnIndexStatePath)
$returnIntegrationStatusStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $federationPolicy.returnIntegrationStatusStatePath)
$returnIntegrationStatusMarkdownPath = [System.IO.Path]::ChangeExtension($returnIntegrationStatusStatePath, '.md')
$requestIndexStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $federationPolicy.requestIndexStatePath)

New-Item -ItemType Directory -Force -Path $returnInboxRoot | Out-Null

$requestIndex = Read-JsonFileOrNull -Path $requestIndexStatePath
$requestEntries = if ($null -ne $requestIndex) { @($requestIndex.requests) } else { @() }
$requestMap = @{}
foreach ($requestEntry in @($requestEntries)) {
    $requestId = [string] (Get-ObjectPropertyValueOrNull -InputObject $requestEntry -PropertyName 'requestId')
    if (-not [string]::IsNullOrWhiteSpace($requestId)) {
        $requestMap[$requestId] = $requestEntry
    }
}

$returnFiles = @()
if (Test-Path -LiteralPath $returnInboxRoot -PathType Container) {
    $returnFiles = @(Get-ChildItem -LiteralPath $returnInboxRoot -Recurse -File -Filter 'return.json')
}

$returnEntries = foreach ($returnFile in @($returnFiles)) {
    $returnPayload = Read-JsonFile -Path $returnFile.FullName
    $missingFields = New-Object System.Collections.Generic.List[string]

    foreach ($requiredField in @($returnContract.requiredFields)) {
        if (-not (Test-JsonPropertyPresent -InputObject $returnPayload -PropertyName ([string] $requiredField))) {
            [void] $missingFields.Add([string] $requiredField)
        }
    }

    $returnId = [string] (Get-ObjectPropertyValueOrNull -InputObject $returnPayload -PropertyName 'returnId')
    $requestId = [string] (Get-ObjectPropertyValueOrNull -InputObject $returnPayload -PropertyName 'requestId')
    $targetBucketLabel = [string] (Get-ObjectPropertyValueOrNull -InputObject $returnPayload -PropertyName 'targetBucketLabel')
    $listenerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $returnPayload -PropertyName 'listenerState')
    $workingState = [string] (Get-ObjectPropertyValueOrNull -InputObject $returnPayload -PropertyName 'workingState')
    $governanceAction = [string] (Get-ObjectPropertyValueOrNull -InputObject $returnPayload -PropertyName 'governanceAction')
    $workClass = [string] (Get-ObjectPropertyValueOrNull -InputObject $returnPayload -PropertyName 'workClass')
    $returnClass = [string] (Get-ObjectPropertyValueOrNull -InputObject $returnPayload -PropertyName 'returnClass')
    $standingResult = [string] (Get-ObjectPropertyValueOrNull -InputObject $returnPayload -PropertyName 'standingResult')
    $hitlRequired = [bool] (Get-ObjectPropertyValueOrNull -InputObject $returnPayload -PropertyName 'hitlRequired')
    $neededFollowup = New-UniqueStringArray -Values @((Get-ObjectPropertyValueOrNull -InputObject $returnPayload -PropertyName 'neededFollowup'))
    $triadRefs = Get-ObjectPropertyValueOrNull -InputObject $returnPayload -PropertyName 'triadRefs'
    $artifactsTouched = New-UniqueStringArray -Values @((Get-ObjectPropertyValueOrNull -InputObject $returnPayload -PropertyName 'artifactsTouched'))
    $verificationOutcomes = @((Get-ObjectPropertyValueOrNull -InputObject $returnPayload -PropertyName 'verificationOutcomes'))

    $validationErrors = New-Object System.Collections.Generic.List[string]
    foreach ($missingField in @($missingFields)) {
        [void] $validationErrors.Add(('missing-required-field:{0}' -f $missingField))
    }

    if (-not (Test-EnumMembership -Value $targetBucketLabel -AllowedValues @($returnContract.targetBucketLabels))) {
        [void] $validationErrors.Add('invalid-target-bucket-label')
    }
    if (-not (Test-EnumMembership -Value $listenerState -AllowedValues @($returnContract.listenerStates))) {
        [void] $validationErrors.Add('invalid-listener-state')
    }
    if (-not (Test-EnumMembership -Value $workingState -AllowedValues @($returnContract.workingStates))) {
        [void] $validationErrors.Add('invalid-working-state')
    }
    if (-not (Test-EnumMembership -Value $governanceAction -AllowedValues @($returnContract.governanceActions))) {
        [void] $validationErrors.Add('invalid-governance-action')
    }
    if (-not (Test-EnumMembership -Value $workClass -AllowedValues @($returnContract.workClasses))) {
        [void] $validationErrors.Add('invalid-work-class')
    }
    if (-not (Test-EnumMembership -Value $returnClass -AllowedValues @($returnContract.returnClasses))) {
        [void] $validationErrors.Add('invalid-return-class')
    }

    $matchedRequest = $null
    $requestMatchState = 'missing'
    if (-not [string]::IsNullOrWhiteSpace($requestId) -and $requestMap.ContainsKey($requestId)) {
        $matchedRequest = $requestMap[$requestId]
        $requestMatchState = 'matched'
    } else {
        [void] $validationErrors.Add('request-id-not-found-in-build-index')
    }

    if ($null -ne $matchedRequest) {
        $matchedBucketLabel = [string] (Get-ObjectPropertyValueOrNull -InputObject $matchedRequest -PropertyName 'targetBucketLabel')
        if ($matchedBucketLabel -ne $targetBucketLabel) {
            $requestMatchState = 'bucket-mismatch'
            [void] $validationErrors.Add('target-bucket-mismatch-with-request')
        }

        $neededReturnClass = [string] (Get-ObjectPropertyValueOrNull -InputObject $matchedRequest -PropertyName 'neededReturnClass')
        if (-not [string]::IsNullOrWhiteSpace($neededReturnClass) -and $neededReturnClass -ne $returnClass) {
            [void] $validationErrors.Add('return-class-mismatch-with-request')
        }
    }

    if ($governanceAction -eq 'admit' -and $workingState -ne 'completed') {
        [void] $validationErrors.Add('admit-return-not-completed')
    }

    $dopingHeaderRef = Get-TriadRefValue -TriadRefs $triadRefs -CandidateNames @('dopingHeader', 'doping_header', 'dopingHeaderPath', 'doping_header_path')
    $receiptRef = Get-TriadRefValue -TriadRefs $triadRefs -CandidateNames @('receipt', 'receiptPath', 'receipt_path')
    $noticeRef = Get-TriadRefValue -TriadRefs $triadRefs -CandidateNames @('notice', 'noticePath', 'notice_path')
    $triadComplete = (
        -not [string]::IsNullOrWhiteSpace($dopingHeaderRef) -and
        -not [string]::IsNullOrWhiteSpace($receiptRef) -and
        -not [string]::IsNullOrWhiteSpace($noticeRef)
    )
    if (-not $triadComplete) {
        [void] $validationErrors.Add('triad-refs-incomplete')
    }

    $integrationDisposition = 'hold'
    $returnStanding = 'received-awaiting-review'

    if ($validationErrors.Count -gt 0) {
        $integrationDisposition = 'hold'
        $returnStanding = 'invalid-return'
    } elseif ($hitlRequired -or $returnClass -eq 'hold' -or $listenerState -eq 'withheld_or_escalated' -or $governanceAction -in @('hold', 'narrow', 'defer', 'refuse', 'return', 'escalate')) {
        $integrationDisposition = 'hold'
        $returnStanding = 'held-or-escalated'
    } elseif ($governanceAction -eq 'admit' -and $listenerState -in @('admissible', 'actionable') -and $triadComplete) {
        $integrationDisposition = $returnClass
        $returnStanding = 'admitted-for-build-review'
    }

    $definedTerms = if (@($validationErrors | Where-Object { $_ -like 'missing-required-field:*' -or $_ -like 'invalid-*' }).Count -eq 0) { 'pass' } else { 'fail' }
    $contextualScope = if ($requestMatchState -eq 'matched') { 'pass' } else { 'fail' }
    $evidenceSufficiency = if ($triadComplete -and ($workingState -eq 'completed' -or $governanceAction -ne 'admit')) { 'pass' } else { 'fail' }
    $nonContradiction = if (@($validationErrors | Where-Object { $_ -in @('return-class-mismatch-with-request', 'admit-return-not-completed') }).Count -eq 0) { 'pass' } else { 'fail' }
    $surfaceAppropriateness = if ($governanceAction -ne 'admit' -or $listenerState -in @('admissible', 'actionable')) { 'pass' } else { 'fail' }
    $promotionWarrant = if ($returnStanding -eq 'admitted-for-build-review') { 'pass' } else { 'fail' }
    $categoryErrorDetected = $contextualScope -eq 'fail' -or $surfaceAppropriateness -eq 'fail'
    $promotionWithoutReceiptsDetected = $governanceAction -eq 'admit' -and -not $triadComplete
    $receiptsSufficientForPromotion = $evidenceSufficiency -eq 'pass' -and $promotionWarrant -eq 'pass'
    $promotionReceiptState = if ($returnStanding -eq 'invalid-return') { 'invalid' } elseif ($receiptsSufficientForPromotion) { 'sufficient' } else { 'insufficient-for-closure' }
    $discernmentAction = 'remain-provisional'
    $standingSurfaceClass = 'rhetoric-bearing'
    $discernmentReason = 'source-bucket-return-remains-provisional'

    if ($returnStanding -eq 'invalid-return') {
        $discernmentAction = 'refuse'
        $standingSurfaceClass = 'refusal-surface'
        $discernmentReason = if ($categoryErrorDetected) { 'source-bucket-return-category-error-detected' } else { 'source-bucket-return-contract-invalid' }
    } elseif ($returnStanding -eq 'held-or-escalated') {
        $discernmentAction = 'hold'
        $standingSurfaceClass = 'refusal-surface'
        $discernmentReason = if ($promotionWithoutReceiptsDetected) { 'source-bucket-return-promotion-without-triad-receipts' } else { 'source-bucket-return-held-pending-lawful-admission' }
    } elseif ($returnStanding -eq 'admitted-for-build-review') {
        $discernmentAction = 'admit'
        $standingSurfaceClass = 'closure-bearing'
        $discernmentReason = 'source-bucket-return-ready-for-build-review'
    }

    [pscustomobject] [ordered]@{
        returnId = $returnId
        requestId = $requestId
        targetBucketLabel = $targetBucketLabel
        generatedAtUtc = [string] (Get-ObjectPropertyValueOrNull -InputObject $returnPayload -PropertyName 'generatedAtUtc')
        listenerState = $listenerState
        workingState = $workingState
        governanceAction = $governanceAction
        returnClass = $returnClass
        workClass = $workClass
        standingResult = $standingResult
        hitlRequired = $hitlRequired
        requestMatchState = $requestMatchState
        triadComplete = $triadComplete
        requestedStanding = 'source-bucket-return-build-review'
        discernmentAction = $discernmentAction
        standingSurfaceClass = $standingSurfaceClass
        promotionReceiptState = $promotionReceiptState
        receiptsSufficientForPromotion = $receiptsSufficientForPromotion
        categoryErrorDetected = $categoryErrorDetected
        promotionWithoutReceiptsDetected = $promotionWithoutReceiptsDetected
        discernmentReason = $discernmentReason
        discernmentEvaluation = [pscustomobject]@{
            definedTerms = $definedTerms
            contextualScope = $contextualScope
            evidenceSufficiency = $evidenceSufficiency
            nonContradiction = $nonContradiction
            surfaceAppropriateness = $surfaceAppropriateness
            promotionWarrant = $promotionWarrant
        }
        integrationDisposition = $integrationDisposition
        returnStanding = $returnStanding
        validationErrors = New-UniqueStringArray -Values @($validationErrors)
        artifactsTouched = @($artifactsTouched)
        verificationOutcomeCount = @($verificationOutcomes).Count
        neededFollowup = @($neededFollowup)
        bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath (Split-Path -Parent $returnFile.FullName)
        returnJsonPath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $returnFile.FullName
        triadRefs = [pscustomobject]@{
            dopingHeader = $dopingHeaderRef
            receipt = $receiptRef
            notice = $noticeRef
        }
    }
}

$updatedRequestEntries = foreach ($requestEntry in @($requestEntries)) {
    $requestId = [string] (Get-ObjectPropertyValueOrNull -InputObject $requestEntry -PropertyName 'requestId')
    $matchingReturns = @(
        $returnEntries |
            Where-Object { [string] $_.requestId -eq $requestId } |
            Sort-Object { [string] $_.generatedAtUtc } -Descending
    )
    $latestReturn = @($matchingReturns | Select-Object -First 1)
    $latestReturnEntry = if ($latestReturn.Count -gt 0) { $latestReturn[0] } else { $null }

    $updatedRequestEntry = [ordered]@{
        requestId = $requestId
        requestKey = [string] (Get-ObjectPropertyValueOrNull -InputObject $requestEntry -PropertyName 'requestKey')
        generatedAtUtc = [string] (Get-ObjectPropertyValueOrNull -InputObject $requestEntry -PropertyName 'generatedAtUtc')
        targetBucketLabel = [string] (Get-ObjectPropertyValueOrNull -InputObject $requestEntry -PropertyName 'targetBucketLabel')
        buildSurface = [string] (Get-ObjectPropertyValueOrNull -InputObject $requestEntry -PropertyName 'buildSurface')
        subject = [string] (Get-ObjectPropertyValueOrNull -InputObject $requestEntry -PropertyName 'subject')
        predicate = [string] (Get-ObjectPropertyValueOrNull -InputObject $requestEntry -PropertyName 'predicate')
        actions = @((Get-ObjectPropertyValueOrNull -InputObject $requestEntry -PropertyName 'actions'))
        neededReturnClass = [string] (Get-ObjectPropertyValueOrNull -InputObject $requestEntry -PropertyName 'neededReturnClass')
        admissibilityClass = [string] (Get-ObjectPropertyValueOrNull -InputObject $requestEntry -PropertyName 'admissibilityClass')
        hitlState = [string] (Get-ObjectPropertyValueOrNull -InputObject $requestEntry -PropertyName 'hitlState')
        requestState = [string] (Get-ObjectPropertyValueOrNull -InputObject $requestEntry -PropertyName 'requestState')
        sourceTouchPointIds = @((Get-ObjectPropertyValueOrNull -InputObject $requestEntry -PropertyName 'sourceTouchPointIds'))
        bundlePath = [string] (Get-ObjectPropertyValueOrNull -InputObject $requestEntry -PropertyName 'bundlePath')
    }

    if ($null -ne $latestReturnEntry) {
        $updatedRequestEntry.requestState = Get-RequestLifecycleStateFromReturn -ReturnStanding ([string] $latestReturnEntry.returnStanding) -IntegrationDisposition ([string] $latestReturnEntry.integrationDisposition)
        $updatedRequestEntry.lastReturnId = [string] $latestReturnEntry.returnId
        $updatedRequestEntry.lastReturnGeneratedAtUtc = [string] $latestReturnEntry.generatedAtUtc
        $updatedRequestEntry.lastReturnClass = [string] $latestReturnEntry.returnClass
        $updatedRequestEntry.lastReturnStanding = [string] $latestReturnEntry.returnStanding
        $updatedRequestEntry.lastIntegrationDisposition = [string] $latestReturnEntry.integrationDisposition
        $updatedRequestEntry.lastReturnBundlePath = [string] $latestReturnEntry.bundlePath
    }

    [pscustomobject] $updatedRequestEntry
}

$bucketStates = foreach ($bucketLabel in @($returnContract.targetBucketLabels)) {
    $bucketReturns = @($returnEntries | Where-Object { [string] $_.targetBucketLabel -eq [string] $bucketLabel })
    $admittedBucketReturns = @($bucketReturns | Where-Object { [string] $_.returnStanding -eq 'admitted-for-build-review' })
    $heldBucketReturns = @($bucketReturns | Where-Object { [string] $_.integrationDisposition -eq 'hold' })
    $latestBucketReturn = @($bucketReturns | Sort-Object { [string] $_.generatedAtUtc } -Descending | Select-Object -First 1)

    $bucketState = 'awaiting-return'
    if (@($admittedBucketReturns).Count -gt 0) {
        $bucketState = 'return-ready-for-build-review'
    } elseif (@($heldBucketReturns).Count -gt 0) {
        $bucketState = 'return-held-or-escalated'
    } elseif (@($bucketReturns).Count -gt 0) {
        $bucketState = 'return-received-awaiting-review'
    }

    [pscustomobject] [ordered]@{
        targetBucketLabel = [string] $bucketLabel
        bucketState = $bucketState
        returnCount = @($bucketReturns).Count
        admittedReturnCount = @($admittedBucketReturns).Count
        heldReturnCount = @($heldBucketReturns).Count
        latestReturnId = if (@($latestBucketReturn).Count -gt 0) { [string] $latestBucketReturn[0].returnId } else { $null }
        latestReturnClass = if (@($latestBucketReturn).Count -gt 0) { [string] $latestBucketReturn[0].returnClass } else { $null }
        latestBundlePath = if (@($latestBucketReturn).Count -gt 0) { [string] $latestBucketReturn[0].bundlePath } else { $null }
    }
}

$totalReturnCount = @($returnEntries).Count
$admittedReturnCount = @($returnEntries | Where-Object { [string] $_.returnStanding -eq 'admitted-for-build-review' }).Count
$heldReturnCount = @($returnEntries | Where-Object { [string] $_.integrationDisposition -eq 'hold' }).Count
$hitlRequiredReturnCount = @($returnEntries | Where-Object { [bool] $_.hitlRequired }).Count
$invalidReturnCount = @($returnEntries | Where-Object { [string] $_.returnStanding -eq 'invalid-return' }).Count
$refusedReturnCount = @($returnEntries | Where-Object { [string] $_.discernmentAction -eq 'refuse' }).Count
$provisionalReturnCount = @($returnEntries | Where-Object { [string] $_.discernmentAction -eq 'remain-provisional' }).Count
$categoryErrorCount = @($returnEntries | Where-Object { [bool] $_.categoryErrorDetected }).Count
$promotionWithoutReceiptsCount = @($returnEntries | Where-Object { [bool] $_.promotionWithoutReceiptsDetected }).Count

$integrationState = 'awaiting-source-bucket-returns'
$reasonCode = 'source-bucket-return-intake-awaiting-returns'
$nextAction = 'wait-for-lawful-source-bucket-returns-or-operator-review'

if ($admittedReturnCount -gt 0) {
    $integrationState = 'receipted-returns-ready-for-build-review'
    $reasonCode = 'source-bucket-return-intake-admitted-return-ready'
    $nextAction = 'review-admitted-source-bucket-return-and-route-into-build'
} elseif ($invalidReturnCount -gt 0) {
    $integrationState = 'invalid-returns-require-review'
    $reasonCode = 'source-bucket-return-intake-invalid-return'
    $nextAction = 'review-invalid-source-bucket-return'
} elseif ($heldReturnCount -gt 0 -and $totalReturnCount -gt 0) {
    $integrationState = 'returns-held-or-escalated'
    $reasonCode = 'source-bucket-return-intake-held'
    $nextAction = 'preserve-held-return-state-until-operator-review'
} elseif ($totalReturnCount -gt 0) {
    $integrationState = 'returns-received-awaiting-build-review'
    $reasonCode = 'source-bucket-return-intake-awaiting-review'
    $nextAction = 'review-received-source-bucket-return'
}

$requestedStanding = 'source-bucket-return-build-review'
$discernmentAction = 'remain-provisional'
$standingSurfaceClass = 'rhetoric-bearing'
$promotionReceiptState = 'insufficient-for-closure'
$receiptsSufficientForPromotion = $false
$categoryErrorDetected = $categoryErrorCount -gt 0
$promotionWithoutReceiptsDetected = $promotionWithoutReceiptsCount -gt 0
$discernmentReason = 'source-bucket-return-intake-awaiting-lawful-returns'
$discernmentDefinedTerms = if ($invalidReturnCount -gt 0) { 'fail' } else { 'pass' }
$discernmentContextualScope = if ($categoryErrorCount -gt 0) { 'fail' } else { 'pass' }
$discernmentEvidenceSufficiency = 'fail'
$discernmentNonContradiction = if ($invalidReturnCount -gt 0) { 'fail' } else { 'pass' }
$discernmentSurfaceAppropriateness = if ($categoryErrorCount -gt 0) { 'fail' } else { 'pass' }
$discernmentPromotionWarrant = 'fail'

if ($admittedReturnCount -gt 0) {
    $discernmentAction = 'admit'
    $standingSurfaceClass = 'closure-bearing'
    $promotionReceiptState = 'sufficient'
    $receiptsSufficientForPromotion = $true
    $discernmentReason = 'source-bucket-return-intake-admitted-return-ready'
    $discernmentEvidenceSufficiency = 'pass'
    $discernmentPromotionWarrant = 'pass'
} elseif ($invalidReturnCount -gt 0) {
    $discernmentAction = 'refuse'
    $standingSurfaceClass = 'refusal-surface'
    $promotionReceiptState = 'invalid'
    $discernmentReason = 'source-bucket-return-intake-category-or-contract-invalid'
} elseif ($heldReturnCount -gt 0 -and $totalReturnCount -gt 0) {
    $discernmentAction = 'hold'
    $standingSurfaceClass = 'refusal-surface'
    $discernmentReason = if ($promotionWithoutReceiptsDetected) { 'source-bucket-return-intake-promotion-without-receipts' } else { 'source-bucket-return-intake-held-pending-review' }
}

$bucketRequestSummaries = foreach ($bucketLabel in @($requestContract.targetBucketLabels)) {
    $bucketRequests = @(
        $updatedRequestEntries | Where-Object {
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
        latestRequestState = if ($latestBucketRequest.Count -gt 0) { [string] (Get-ObjectPropertyValueOrNull -InputObject $latestBucketRequest[0] -PropertyName 'requestState') } else { $null }
        latestBundlePath = if ($latestBucketRequest.Count -gt 0) { [string] (Get-ObjectPropertyValueOrNull -InputObject $latestBucketRequest[0] -PropertyName 'bundlePath') } else { $null }
    }
}

$updatedRequestIndexPayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    policyPath = if ($null -ne $requestIndex) { [string] (Get-ObjectPropertyValueOrNull -InputObject $requestIndex -PropertyName 'policyPath') } else { Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $resolvedFederationPolicyPath }
    requestContractPath = if ($null -ne $requestIndex) { [string] (Get-ObjectPropertyValueOrNull -InputObject $requestIndex -PropertyName 'requestContractPath') } else { Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $resolvedRequestContractPath }
    lastRequestId = [string] (Get-ObjectPropertyValueOrNull -InputObject $requestIndex -PropertyName 'lastRequestId')
    lastRequestBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $requestIndex -PropertyName 'lastRequestBundle')
    activeRequestCount = @($updatedRequestEntries | Where-Object { Test-RequestStillActive -RequestEntry $_ }).Count
    publishedRequestCount = @($updatedRequestEntries).Count
    activeRequestIds = @(
        $updatedRequestEntries |
            Where-Object { Test-RequestStillActive -RequestEntry $_ } |
            ForEach-Object { [string] (Get-ObjectPropertyValueOrNull -InputObject $_ -PropertyName 'requestId') }
    )
    bucketSummaries = $bucketRequestSummaries
    requests = @($updatedRequestEntries)
}
Add-AutomationCascadeOperatorPromptProperty -InputObject $updatedRequestIndexPayload | Out-Null
Add-BuildDispatchRootPromptProperty -InputObject $updatedRequestIndexPayload | Out-Null

$returnIndexPayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    policyPath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $resolvedFederationPolicyPath
    returnContractPath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $resolvedReturnContractPath
    requestIndexStatePath = [string] $federationPolicy.requestIndexStatePath
    returnInboxRoot = [string] $federationPolicy.returnInboxRoot
    returnCount = $totalReturnCount
    admittedReturnCount = $admittedReturnCount
    heldReturnCount = $heldReturnCount
    invalidReturnCount = $invalidReturnCount
    returns = @($returnEntries)
}
Add-AutomationCascadeOperatorPromptProperty -InputObject $returnIndexPayload | Out-Null
Add-BuildDispatchRootPromptProperty -InputObject $returnIndexPayload | Out-Null

$statusPayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $returnIndexPayload.generatedAtUtc
    policyPath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $resolvedFederationPolicyPath
    formalSurfaceMarkdownPath = [string] $federationPolicy.formalSurfaceMarkdownPath
    requestContractPath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $resolvedRequestContractPath
    returnContractPath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $resolvedReturnContractPath
    returnInboxRoot = [string] $federationPolicy.returnInboxRoot
    requestIndexStatePath = [string] $federationPolicy.requestIndexStatePath
    returnIndexStatePath = [string] $federationPolicy.returnIndexStatePath
    integrationState = $integrationState
    reasonCode = $reasonCode
    nextAction = $nextAction
    totalReturnCount = $totalReturnCount
    admittedReturnCount = $admittedReturnCount
    heldReturnCount = $heldReturnCount
    invalidReturnCount = $invalidReturnCount
    refusedReturnCount = $refusedReturnCount
    provisionalReturnCount = $provisionalReturnCount
    hitlRequiredReturnCount = $hitlRequiredReturnCount
    categoryErrorCount = $categoryErrorCount
    promotionWithoutReceiptsCount = $promotionWithoutReceiptsCount
    requestedStanding = $requestedStanding
    discernmentAction = $discernmentAction
    standingSurfaceClass = $standingSurfaceClass
    promotionReceiptState = $promotionReceiptState
    receiptsSufficientForPromotion = $receiptsSufficientForPromotion
    categoryErrorDetected = $categoryErrorDetected
    promotionWithoutReceiptsDetected = $promotionWithoutReceiptsDetected
    discernmentReason = $discernmentReason
    discernmentEvaluation = [ordered]@{
        definedTerms = $discernmentDefinedTerms
        contextualScope = $discernmentContextualScope
        evidenceSufficiency = $discernmentEvidenceSufficiency
        nonContradiction = $discernmentNonContradiction
        surfaceAppropriateness = $discernmentSurfaceAppropriateness
        promotionWarrant = $discernmentPromotionWarrant
    }
    bucketStates = $bucketStates
    listenerStates = @($returnContract.listenerStates)
    governanceActions = @($returnContract.governanceActions)
    returnClasses = @($returnContract.returnClasses)
    activeRequestCount = $updatedRequestIndexPayload.activeRequestCount
}
Add-AutomationCascadeOperatorPromptProperty -InputObject $statusPayload | Out-Null
Add-BuildDispatchRootPromptProperty -InputObject $statusPayload | Out-Null

Write-JsonFile -Path $requestIndexStatePath -Value $updatedRequestIndexPayload
Write-JsonFile -Path $returnIndexStatePath -Value $returnIndexPayload
Write-JsonFile -Path $returnIntegrationStatusStatePath -Value $statusPayload

$markdownLines = @(
    '# Source-Bucket Return Integration Status',
    '',
    ('- Generated at (UTC): `{0}`' -f $statusPayload.generatedAtUtc),
    ('- Integration state: `{0}`' -f $statusPayload.integrationState),
    ('- Reason code: `{0}`' -f $statusPayload.reasonCode),
    ('- Next action: `{0}`' -f $statusPayload.nextAction),
    ('- Total return count: `{0}`' -f $statusPayload.totalReturnCount),
    ('- Admitted return count: `{0}`' -f $statusPayload.admittedReturnCount),
    ('- Held return count: `{0}`' -f $statusPayload.heldReturnCount),
    ('- Invalid return count: `{0}`' -f $statusPayload.invalidReturnCount),
    ('- Refused return count: `{0}`' -f $statusPayload.refusedReturnCount),
    ('- Provisional return count: `{0}`' -f $statusPayload.provisionalReturnCount),
    ('- HITL-required return count: `{0}`' -f $statusPayload.hitlRequiredReturnCount),
    ('- Category error count: `{0}`' -f $statusPayload.categoryErrorCount),
    ('- Promotion without receipts count: `{0}`' -f $statusPayload.promotionWithoutReceiptsCount),
    ('- Requested standing: `{0}`' -f $statusPayload.requestedStanding),
    ('- Discernment action: `{0}`' -f $statusPayload.discernmentAction),
    ('- Standing surface class: `{0}`' -f $statusPayload.standingSurfaceClass),
    ('- Promotion receipt state: `{0}`' -f $statusPayload.promotionReceiptState),
    ('- Receipts sufficient for promotion: `{0}`' -f [bool] $statusPayload.receiptsSufficientForPromotion),
    ('- Category error detected: `{0}`' -f [bool] $statusPayload.categoryErrorDetected),
    ('- Promotion without receipts detected: `{0}`' -f [bool] $statusPayload.promotionWithoutReceiptsDetected),
    ('- Discernment reason: `{0}`' -f $statusPayload.discernmentReason),
    ('- Discernment evaluation: `definedTerms={0}; contextualScope={1}; evidenceSufficiency={2}; nonContradiction={3}; surfaceAppropriateness={4}; promotionWarrant={5}`' -f
        $statusPayload.discernmentEvaluation.definedTerms,
        $statusPayload.discernmentEvaluation.contextualScope,
        $statusPayload.discernmentEvaluation.evidenceSufficiency,
        $statusPayload.discernmentEvaluation.nonContradiction,
        $statusPayload.discernmentEvaluation.surfaceAppropriateness,
        $statusPayload.discernmentEvaluation.promotionWarrant),
    ''
)

foreach ($bucketState in @($bucketStates)) {
    $markdownLines += @(
        ('## {0}' -f [string] $bucketState.targetBucketLabel),
        '',
        ('- Bucket state: `{0}`' -f [string] $bucketState.bucketState),
        ('- Return count: `{0}`' -f [int] $bucketState.returnCount),
        ('- Admitted return count: `{0}`' -f [int] $bucketState.admittedReturnCount),
        ('- Held return count: `{0}`' -f [int] $bucketState.heldReturnCount),
        ('- Latest return id: `{0}`' -f $(if ($bucketState.latestReturnId) { [string] $bucketState.latestReturnId } else { 'none' })),
        ('- Latest return class: `{0}`' -f $(if ($bucketState.latestReturnClass) { [string] $bucketState.latestReturnClass } else { 'none' })),
        ('- Latest bundle path: `{0}`' -f $(if ($bucketState.latestBundlePath) { [string] $bucketState.latestBundlePath } else { 'none' })),
        ''
    )
}

$markdownLines = Add-AutomationCascadePromptMarkdownLines -MarkdownLines $markdownLines
Set-Content -LiteralPath $returnIntegrationStatusMarkdownPath -Value $markdownLines -Encoding utf8

Write-Host ('[source-bucket-return-intake] State: {0}' -f $returnIntegrationStatusStatePath)
$returnIntegrationStatusStatePath
