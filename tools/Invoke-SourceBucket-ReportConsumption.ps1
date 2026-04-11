param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',
    [string] $RepoRoot,
    [string] $CyclePolicyPath = 'OAN Mortalis V1.1.1/build/local-automation-cycle.json',
    [string] $ConsumptionPolicyPath = 'OAN Mortalis V1.1.1/build/source-bucket-report-consumption.json',
    [switch] $FullResearchMode,
    [switch] $SkipPruning
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

. (Join-Path $PSScriptRoot 'Resolve-SourceBucket-ThreadContinuity.ps1')
$discernmentAdmissionHelperPath = Join-Path $PSScriptRoot 'Discernment-Admission.ps1'
. $discernmentAdmissionHelperPath

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

function Write-MarkdownFile {
    param(
        [string] $Path,
        [string[]] $Lines
    )

    $directory = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($directory)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }

    Set-Content -LiteralPath $Path -Value $Lines -Encoding utf8
}

function Get-OptionalDateTimeUtc {
    param([object] $Value)

    if ($null -eq $Value) {
        return $null
    }

    $stringValue = [string] $Value
    if ([string]::IsNullOrWhiteSpace($stringValue)) {
        return $null
    }

    return [datetime]::Parse($stringValue, [System.Globalization.CultureInfo]::InvariantCulture, [System.Globalization.DateTimeStyles]::RoundtripKind).ToUniversalTime()
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

function ConvertTo-StringArray {
    param([object] $Value)

    if ($null -eq $Value) {
        return @()
    }

    if ($Value -is [string]) {
        return @($Value)
    }

    return @($Value | ForEach-Object {
            if ($_ -is [string]) {
                $_
            } elseif ($_.PSObject.Properties['summary']) {
                [string] $_.summary
            } else {
                [string] $_
            }
        } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
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

function Get-ValueBySelector {
    param(
        [object] $InputObject,
        [string] $Selector
    )

    if ($null -eq $InputObject -or [string]::IsNullOrWhiteSpace($Selector)) {
        return $null
    }

    $current = $InputObject
    foreach ($segment in @($Selector.Split('.'))) {
        if ($null -eq $current) {
            return $null
        }

        $segmentName = $segment
        $index = $null
        if ($segment -match '^(?<name>[^\[]+)\[(?<index>\d+)\]$') {
            $segmentName = $Matches['name']
            $index = [int] $Matches['index']
        }

        if ($current -is [System.Collections.IDictionary]) {
            if (-not $current.Contains($segmentName)) {
                return $null
            }
            $current = $current[$segmentName]
        } else {
            $property = $current.PSObject.Properties[$segmentName]
            if ($null -eq $property) {
                return $null
            }
            $current = $property.Value
        }

        if ($null -ne $index) {
            $items = @($current)
            if ($items.Count -le $index) {
                return $null
            }
            $current = $items[$index]
        }
    }

    return $current
}

function Get-FirstValueBySelectors {
    param(
        [object[]] $StateEntries,
        [string[]] $Selectors
    )

    foreach ($entry in @($StateEntries)) {
        foreach ($selector in @($Selectors)) {
            $value = Get-ValueBySelector -InputObject $entry.data -Selector $selector
            if ($null -eq $value) {
                continue
            }

            if ($value -is [string] -and -not [string]::IsNullOrWhiteSpace([string] $value)) {
                return $value
            }

            if ($value -isnot [string]) {
                return $value
            }
        }
    }

    return $null
}

function Get-OpenHoldSummaries {
    param([object[]] $StateEntries)

    $values = New-Object System.Collections.Generic.List[string]

    foreach ($entry in @($StateEntries)) {
        foreach ($selector in @('active_holds', 'activeHolds')) {
            foreach ($item in @(ConvertTo-StringArray -Value (Get-ValueBySelector -InputObject $entry.data -Selector $selector))) {
                if (-not $values.Contains($item)) {
                    $values.Add($item)
                }
            }
        }

        $unresolvedHolds = @(Get-ValueBySelector -InputObject $entry.data -Selector 'unresolvedHolds')
        foreach ($hold in $unresolvedHolds) {
            if ($null -eq $hold) {
                continue
            }

            $status = [string] (Get-ValueBySelector -InputObject $hold -Selector 'status')
            if (-not [string]::IsNullOrWhiteSpace($status) -and $status -ne 'open') {
                continue
            }

            $summary = [string] (Get-ValueBySelector -InputObject $hold -Selector 'summary')
            if (-not [string]::IsNullOrWhiteSpace($summary) -and -not $values.Contains($summary)) {
                $values.Add($summary)
            }
        }
    }

    return @($values)
}

function Get-GitWorktreeState {
    param([string] $Path)

    try {
        $gitDir = & git -C $Path rev-parse --show-toplevel 2>$null
        if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace(($gitDir | Select-Object -First 1))) {
            return 'not-a-git-repo'
        }

        $status = & git -C $Path status --porcelain 2>$null
        if ($LASTEXITCODE -ne 0) {
            return 'git-unavailable'
        }

        if (@($status).Count -gt 0) {
            return 'dirty'
        }

        return 'clean'
    }
    catch {
        return 'git-unavailable'
    }
}

function Get-NextHourlyAnchorUtc {
    param([int] $Minute)

    $localNow = Get-Date
    $candidateLocal = Get-Date -Year $localNow.Year -Month $localNow.Month -Day $localNow.Day -Hour $localNow.Hour -Minute $Minute -Second 0
    if ($candidateLocal -le $localNow) {
        $candidateLocal = $candidateLocal.AddHours(1)
    }

    return $candidateLocal.ToUniversalTime()
}

function Get-NextDailyAnchorUtc {
    param(
        [int] $Hour,
        [int] $Minute
    )

    $localNow = Get-Date
    $candidateLocal = Get-Date -Year $localNow.Year -Month $localNow.Month -Day $localNow.Day -Hour $Hour -Minute $Minute -Second 0
    if ($candidateLocal -le $localNow) {
        $candidateLocal = $candidateLocal.AddDays(1)
    }

    return $candidateLocal.ToUniversalTime()
}

function Test-ContradictionStateRequiresReview {
    param([string] $ContradictionState)

    return -not [string]::IsNullOrWhiteSpace($ContradictionState) -and
        -not [string]::Equals($ContradictionState, 'none', [System.StringComparison]::OrdinalIgnoreCase)
}

function Get-ResolvedStateEntries {
    param(
        [string] $BasePath,
        [string[]] $RelativePaths
    )

    $entries = @()
    foreach ($relativePath in @($RelativePaths)) {
        $resolvedPath = Resolve-PathFromRepo -BasePath $BasePath -CandidatePath $relativePath
        $data = Read-JsonFileOrNull -Path $resolvedPath
        if ($null -ne $data) {
            $entries += [pscustomobject]@{
                path = $resolvedPath
                data = $data
            }
        }
    }

    return @($entries)
}

function Get-RawAppendixItems {
    param(
        [string] $BucketRoot,
        [string[]] $RawReportRoots
    )

    $items = @()
    foreach ($root in @($RawReportRoots)) {
        $resolvedRoot = Resolve-PathFromRepo -BasePath $BucketRoot -CandidatePath $root
        if (-not (Test-Path -LiteralPath $resolvedRoot)) {
            continue
        }

        $items += @(Get-ChildItem -LiteralPath $resolvedRoot -Force | ForEach-Object {
                $name = [string] $_.Name
                $reportClass = if ($name -match 'watch' -or $resolvedRoot -match 'watch') {
                    'lane_watch'
                } else {
                    'working_post'
                }

                $continuityKey = if ($name -match 'watch') {
                    'lane-watch'
                } elseif ($name -match 'working-post') {
                    'working-post'
                } elseif ($resolvedRoot -match 'release-digests') {
                    'release-digest'
                } elseif ($resolvedRoot -match 'release-candidates') {
                    'release-candidate'
                } elseif ($resolvedRoot -match 'work-reports') {
                    'work-report'
                } else {
                    'general'
                }

                [pscustomobject]@{
                    path = $_.FullName
                    isDirectory = $_.PSIsContainer
                    lastWriteUtc = $_.LastWriteTimeUtc
                    reportClass = $reportClass
                    continuityKey = $continuityKey
                    tupleKey = '{0}|{1}' -f $reportClass, $continuityKey
                }
            })
    }

    return @($items | Sort-Object -Property lastWriteUtc -Descending)
}

function Get-BucketStatusSummary {
    param(
        [string] $BucketLabel,
        [string] $BucketRoot,
        [string] $SourceScope,
        [object[]] $StateEntries,
        [object[]] $RawAppendixItems,
        [object] $PreviousSummary
    )

    $statusValue = [string] (Get-FirstValueBySelectors -StateEntries $StateEntries -Selectors @('status', 'mission_state', 'cycle_status', 'standing'))
    $nextActionValue = [string] (Get-FirstValueBySelectors -StateEntries $StateEntries -Selectors @('nextLawfulAction', 'next_lawful_actions[0]', 'next_lawful_question'))
    $milestoneValue = [string] (Get-FirstValueBySelectors -StateEntries $StateEntries -Selectors @('activeMilestone', 'phase', 'current_milestone_posture', 'intakeClassification'))
    $blockerSet = @(Get-OpenHoldSummaries -StateEntries $StateEntries)
    $repoWorktreeState = Get-GitWorktreeState -Path $BucketRoot
    $contradictionState = if (@($blockerSet | Where-Object { $_ -match 'contradiction|mismatch|drift' }).Count -gt 0) {
        'under_review'
    } else {
        'none'
    }
    $summaryPayload = [ordered]@{
        bucketLabel = $BucketLabel
        sourceScope = $SourceScope
        status = $statusValue
        nextLawfulAction = $nextActionValue
        blockerSet = @($blockerSet)
        milestone = $milestoneValue
        repoWorktreeState = $repoWorktreeState
        contradictionState = $contradictionState
        rawAppendixCount = @($RawAppendixItems).Count
        authoritativeStatePaths = @($StateEntries | ForEach-Object { $_.path })
    }
    $standingHash = Get-SourceBucketStandingHash -Value $summaryPayload

    $changedFields = New-Object System.Collections.Generic.List[string]
    if ($null -eq $PreviousSummary) {
        $changedFields.Add('initial-observation') | Out-Null
    } else {
        foreach ($field in @('status', 'nextLawfulAction', 'milestone', 'repoWorktreeState', 'contradictionState')) {
            $currentValue = [string] $summaryPayload[$field]
            $previousValue = [string] (Get-ObjectPropertyValueOrNull -InputObject $PreviousSummary -PropertyName $field)
            if (-not [string]::Equals($currentValue, $previousValue, [System.StringComparison]::Ordinal)) {
                $changedFields.Add($field) | Out-Null
            }
        }

        $currentBlockers = @($summaryPayload.blockerSet | Sort-Object)
        $previousBlockers = @($PreviousSummary.blockerSet | Sort-Object)
        if ((($currentBlockers -join '|') -ne ($previousBlockers -join '|'))) {
            $changedFields.Add('blockerSet') | Out-Null
        }
    }

    $materialChange = $changedFields.Count -gt 0
    $stabilityWitnessCount = if ($null -ne $PreviousSummary -and [string] $PreviousSummary.standingHash -eq $standingHash) {
        [int] $PreviousSummary.stabilityWitnessCount + 1
    } else {
        1
    }

    return [pscustomobject]@{
        bucketLabel = $BucketLabel
        sourceScope = $SourceScope
        status = $statusValue
        nextLawfulAction = $nextActionValue
        blockerSet = @($blockerSet)
        milestone = $milestoneValue
        repoWorktreeState = $repoWorktreeState
        contradictionState = $contradictionState
        authoritativeStatePaths = @($StateEntries | ForEach-Object { $_.path })
        rawAppendixCount = @($RawAppendixItems).Count
        rawTupleCount = @($RawAppendixItems | Group-Object -Property tupleKey).Count
        changedFields = @($changedFields)
        materialChange = $materialChange
        stabilityWitnessCount = $stabilityWitnessCount
        standingHash = $standingHash
    }
}

function Get-OanRootSummary {
    param(
        [string] $RepoRootPath,
        [object] $CycleState,
        [object] $TaskingStatusState,
        [object] $MasterThreadState,
        [object] $FederationStatusState,
        [object] $ReturnIntegrationState,
        [object] $PreviousSummary
    )

    $blockers = New-Object System.Collections.Generic.List[string]
    $returnDiscernment = Get-DiscernmentAdmissionEnvelope -State $ReturnIntegrationState -DefaultRequestedStanding 'source-bucket-return-build-review'
    if ([string] $CycleState.lastKnownStatus -eq 'hitl-required') {
        $blockers.Add('root-posture-remains-hitl-required') | Out-Null
    }
    if ($null -ne $MasterThreadState -and -not [string]::IsNullOrWhiteSpace([string] $MasterThreadState.nextAction)) {
        $blockers.Add([string] $MasterThreadState.nextAction) | Out-Null
    }
    $admittedReturnCount = if ($null -ne $ReturnIntegrationState) { [int] $ReturnIntegrationState.admittedReturnCount } else { 0 }
    if ($null -ne $FederationStatusState -and [int] $FederationStatusState.activeRequestCount -gt 0 -and $admittedReturnCount -lt [int] $FederationStatusState.activeRequestCount) {
        if ($returnDiscernment.isRefused) {
            $blockers.Add([string] $returnDiscernment.reason) | Out-Null
        } elseif ($returnDiscernment.isHeld) {
            $blockers.Add([string] $returnDiscernment.reason) | Out-Null
        } else {
            $blockers.Add('lawful-source-bucket-returns-still-pending') | Out-Null
        }
    }

    $recommendedAction = $null
    if ($null -ne $TaskingStatusState) {
        if ($null -ne $TaskingStatusState.currentPosture -and -not [string]::IsNullOrWhiteSpace([string] $TaskingStatusState.currentPosture.recommendedAction)) {
            $recommendedAction = [string] $TaskingStatusState.currentPosture.recommendedAction
        } elseif (-not [string]::IsNullOrWhiteSpace([string] $TaskingStatusState.recommendedAction)) {
            $recommendedAction = [string] $TaskingStatusState.recommendedAction
        }
    }

    $activeTaskMapId = $null
    if ($null -ne $TaskingStatusState) {
        if ($null -ne $TaskingStatusState.longFormTasking -and -not [string]::IsNullOrWhiteSpace([string] $TaskingStatusState.longFormTasking.activeTaskMapId)) {
            $activeTaskMapId = [string] $TaskingStatusState.longFormTasking.activeTaskMapId
        } elseif (-not [string]::IsNullOrWhiteSpace([string] $TaskingStatusState.activeTaskMapId)) {
            $activeTaskMapId = [string] $TaskingStatusState.activeTaskMapId
        }
    }

    $summaryPayload = [ordered]@{
        bucketLabel = 'OAN Tech Stack'
        sourceScope = 'root_only'
        status = [string] $CycleState.lastKnownStatus
        nextLawfulAction = if ($null -ne $MasterThreadState -and -not [string]::IsNullOrWhiteSpace([string] $MasterThreadState.nextAction)) {
            [string] $MasterThreadState.nextAction
        } else {
            [string] $recommendedAction
        }
        blockerSet = @($blockers)
        milestone = [string] $activeTaskMapId
        repoWorktreeState = Get-GitWorktreeState -Path $RepoRootPath
        contradictionState = if ($returnDiscernment.categoryErrorDetected -or $returnDiscernment.promotionWithoutReceiptsDetected) { 'source-bucket-return-discernment-conflict' } else { 'none' }
        activeRequestCount = if ($null -ne $FederationStatusState) { [int] $FederationStatusState.activeRequestCount } else { 0 }
        admittedReturnCount = $admittedReturnCount
        returnDiscernmentAction = [string] $returnDiscernment.action
        returnStandingSurfaceClass = [string] $returnDiscernment.standingSurfaceClass
        returnPromotionReceiptState = [string] $returnDiscernment.promotionReceiptState
    }
    $standingHash = Get-SourceBucketStandingHash -Value $summaryPayload
    $changedFields = New-Object System.Collections.Generic.List[string]
    if ($null -eq $PreviousSummary) {
        $changedFields.Add('initial-observation') | Out-Null
    } else {
        foreach ($field in @('status', 'nextLawfulAction', 'milestone', 'repoWorktreeState', 'contradictionState', 'returnDiscernmentAction', 'returnStandingSurfaceClass', 'returnPromotionReceiptState')) {
            $currentValue = [string] $summaryPayload[$field]
            $previousValue = [string] (Get-ObjectPropertyValueOrNull -InputObject $PreviousSummary -PropertyName $field)
            if (-not [string]::Equals($currentValue, $previousValue, [System.StringComparison]::Ordinal)) {
                $changedFields.Add($field) | Out-Null
            }
        }
        if ((@($summaryPayload.blockerSet | Sort-Object) -join '|') -ne (@($PreviousSummary.blockerSet | Sort-Object) -join '|')) {
            $changedFields.Add('blockerSet') | Out-Null
        }
    }

    return [pscustomobject]@{
        bucketLabel = 'OAN Tech Stack'
        sourceScope = 'root_only'
        status = [string] $summaryPayload.status
        nextLawfulAction = [string] $summaryPayload.nextLawfulAction
        blockerSet = @($summaryPayload.blockerSet)
        milestone = [string] $summaryPayload.milestone
        repoWorktreeState = [string] $summaryPayload.repoWorktreeState
        contradictionState = [string] $summaryPayload.contradictionState
        changedFields = @($changedFields)
        materialChange = $changedFields.Count -gt 0
        stabilityWitnessCount = if ($null -ne $PreviousSummary -and [string] $PreviousSummary.standingHash -eq $standingHash) { [int] $PreviousSummary.stabilityWitnessCount + 1 } else { 1 }
        standingHash = $standingHash
        activeRequestCount = [int] $summaryPayload.activeRequestCount
        admittedReturnCount = [int] $summaryPayload.admittedReturnCount
        returnDiscernmentAction = [string] $summaryPayload.returnDiscernmentAction
        returnStandingSurfaceClass = [string] $summaryPayload.returnStandingSurfaceClass
        returnPromotionReceiptState = [string] $summaryPayload.returnPromotionReceiptState
        authoritativeStatePaths = @()
    }
}

function Get-CandidatePredicate {
    param(
        [string] $BucketLabel,
        [object] $Summary
    )

    switch ($BucketLabel) {
        'OAN Tech Stack' { 'OAN build-green but admission-withheld pending publish-clean master-thread state and lawful source-bucket returns' }
        'IUTT SLI & Lisp' { 'IUTT still inside GEL dependency boundary with downstream hold' }
        'Trivium Forum' { 'Trivium still in phase-4 completion with closure-stage hardening active and environment-bound verification holds' }
        'Latex Styles' { 'Latex Styles locally strong and feed-current but not build-requested' }
        'Holographic Data Tool' { 'HDT candidate-ready as validator/support without direct build need' }
        default { '{0} standing remains active for bounded carry-forward review' -f $BucketLabel }
    }
}

function Get-CandidateId {
    param([string] $BucketLabel)

    return ('candidate://{0}' -f ($BucketLabel.ToLowerInvariant().Replace(' ', '-').Replace('&', 'and')))
}

function Get-CandidateRecord {
    param(
        [string] $BucketLabel,
        [object] $Summary,
        [object] $PreviousCandidate,
        [bool] $IsFullResearchMode
    )

    $candidatePredicate = Get-CandidatePredicate -BucketLabel $BucketLabel -Summary $Summary
    $baseHash = Get-SourceBucketStandingHash -Value ([ordered]@{
            predicate = $candidatePredicate
            status = $Summary.status
            nextLawfulAction = $Summary.nextLawfulAction
            blockerSet = @($Summary.blockerSet)
            milestone = $Summary.milestone
            contradictionState = $Summary.contradictionState
        })

    $stabilityWitnessCount = if ($null -ne $PreviousCandidate -and [string] $PreviousCandidate.candidateHash -eq $baseHash) {
        [int] $PreviousCandidate.stabilityWitnessCount + 1
    } else {
        1
    }

    $fullResearchBoundaryWitnessed = $IsFullResearchMode -or ($null -ne $PreviousCandidate -and [bool] $PreviousCandidate.fullResearchBoundaryWitnessed)
    $requiresContradictionReview = Test-ContradictionStateRequiresReview -ContradictionState ([string] $Summary.contradictionState)
    $discernmentStatus = if ($requiresContradictionReview) {
        'withheld'
    } elseif ($stabilityWitnessCount -ge 2 -and @($Summary.blockerSet).Count -lt 6) {
        'stable_enough_for_review'
    } elseif ([string]::IsNullOrWhiteSpace([string] $Summary.status) -or [string]::IsNullOrWhiteSpace([string] $Summary.nextLawfulAction)) {
        'refinement_required'
    } else {
        'unreviewed'
    }

    $consumerStandingClass = if ($requiresContradictionReview) {
        'pinned_for_review'
    } elseif (-not [bool] $Summary.materialChange) {
        'consumed_no_material_change'
    } else {
        'carry_forward_candidate'
    }

    $formationStage = if ($stabilityWitnessCount -ge 2) { 'tracked_candidate' } else { 'proto_engram_candidate' }
    $carryForwardEligibility = if ($consumerStandingClass -eq 'carry_forward_candidate') { 'daily_summary_only' } else { 'none' }
    $enhancementState = if ($stabilityWitnessCount -ge 2) { 'strengthening' } else { 'fresh' }

    if ($discernmentStatus -eq 'stable_enough_for_review' -and
        $stabilityWitnessCount -ge 2 -and
        $fullResearchBoundaryWitnessed -and
        -not $requiresContradictionReview) {
        $consumerStandingClass = 'gel_research_candidate'
        $formationStage = 'tracked_candidate'
        $carryForwardEligibility = 'simulated_gel_only'
        $enhancementState = 'stable'
    }

    return [ordered]@{
        candidateId = Get-CandidateId -BucketLabel $BucketLabel
        candidatePredicate = $candidatePredicate
        sourceBuckets = @($BucketLabel)
        sourceSurfaces = @($Summary.authoritativeStatePaths | ForEach-Object { [string] $_ })
        sourceScope = [string] $Summary.sourceScope
        formationStage = $formationStage
        discernmentStatus = $discernmentStatus
        carryForwardEligibility = $carryForwardEligibility
        carryForwardReason = if ($Summary.materialChange) { 'standing changed materially and survived one bounded discernment pass' } else { 'no new material delta; candidate remains bounded carry-forward memory only' }
        enhancementState = $enhancementState
        contradictionState = [string] $Summary.contradictionState
        lastWitnessedAt = (Get-Date).ToUniversalTime().ToString('o')
        blockedBy = @($Summary.blockerSet)
        consumerStandingClass = $consumerStandingClass
        candidateHash = $baseHash
        stabilityWitnessCount = $stabilityWitnessCount
        fullResearchBoundaryWitnessed = $fullResearchBoundaryWitnessed
    }
}

function Invoke-RawAppendixCompaction {
    param(
        [object[]] $RawAppendixItems,
        [bool] $MaterialChange,
        [bool] $PinnedForReview,
        [int] $RawRetentionHours,
        [bool] $SkipPruningRequested
    )

    $removed = @()
    if ($SkipPruningRequested -or $PinnedForReview) {
        return [pscustomobject]@{
            removedPaths = @()
            keptCount = @($RawAppendixItems).Count
        }
    }

    $nowUtc = (Get-Date).ToUniversalTime()
    $latestByTuple = @{}
    foreach ($item in @($RawAppendixItems | Sort-Object -Property lastWriteUtc -Descending)) {
        if (-not $latestByTuple.ContainsKey($item.tupleKey)) {
            $latestByTuple[$item.tupleKey] = $item.path
        }
    }

    foreach ($item in @($RawAppendixItems)) {
        $ageHours = ($nowUtc - [datetime] $item.lastWriteUtc).TotalHours
        $isLatestForTuple = [string] $latestByTuple[$item.tupleKey] -eq [string] $item.path
        $shouldRemove = ($ageHours -gt $RawRetentionHours) -or ((-not $MaterialChange) -and -not $isLatestForTuple)
        if (-not $shouldRemove) {
            continue
        }

        try {
            Remove-Item -LiteralPath $item.path -Recurse -Force
            $removed += [string] $item.path
        }
        catch {
        }
    }

    return [pscustomobject]@{
        removedPaths = @($removed)
        keptCount = @($RawAppendixItems).Count - @($removed).Count
    }
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$cyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$consumptionPolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $ConsumptionPolicyPath
$cyclePolicy = Get-Content -Raw -LiteralPath $cyclePolicyPath | ConvertFrom-Json
$consumptionPolicy = Get-Content -Raw -LiteralPath $consumptionPolicyPath | ConvertFrom-Json

$cycleStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.statePath)
$taskingStatusPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath '.audit/state/local-automation-tasking-status.json'
$masterThreadStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.masterThreadOrchestrationStatePath)
$federationStatusPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.sourceBucketFederationStatusStatePath)
$returnIntegrationPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.sourceBucketReturnIntegrationStatusStatePath)

$currentStandingSummaryStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $consumptionPolicy.currentStandingSummaryStatePath)
$currentStandingSummaryMarkdownPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $consumptionPolicy.currentStandingSummaryMarkdownPath)
$currentCandidateGelItemsStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $consumptionPolicy.currentCandidateGelItemsStatePath)
$currentCandidateGelItemsMarkdownPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $consumptionPolicy.currentCandidateGelItemsMarkdownPath)
$threadContinuityStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $consumptionPolicy.threadContinuityStatePath)
$reportConsumptionStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $consumptionPolicy.reportConsumptionStatePath)
$reportConsumptionOutputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $consumptionPolicy.reportConsumptionOutputRoot)
$reportConsumptionDailyOutputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $consumptionPolicy.reportConsumptionDailyOutputRoot)

$previousStandingSummary = Read-JsonFileOrNull -Path $currentStandingSummaryStatePath
$previousCandidateItemsState = Read-JsonFileOrNull -Path $currentCandidateGelItemsStatePath
$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
$taskingStatusState = Read-JsonFileOrNull -Path $taskingStatusPath
$masterThreadState = Read-JsonFileOrNull -Path $masterThreadStatePath
$federationStatusState = Read-JsonFileOrNull -Path $federationStatusPath
$returnIntegrationState = Read-JsonFileOrNull -Path $returnIntegrationPath

$localNow = Get-Date
$utcNow = $localNow.ToUniversalTime()
$schedule = $consumptionPolicy.scheduling
$isFullResearchMode = [bool] $FullResearchMode -or ($localNow.Hour -eq [int] $schedule.fullResearchRunHourLocal -and $localNow.Minute -eq [int] $schedule.fullResearchRunMinuteLocal)
$bundleId = '{0}-{1}' -f $utcNow.ToString('yyyyMMddTHHmmssZ'), $(if ($isFullResearchMode) { 'full-research' } else { 'consumption' })
$bundleRoot = Join-Path $reportConsumptionOutputRoot $bundleId
$dailyBundleId = $localNow.ToString('yyyy-MM-dd')
$dailyBundleRoot = Join-Path $reportConsumptionDailyOutputRoot $dailyBundleId

$previousRootSummary = if ($null -ne $previousStandingSummary) { $previousStandingSummary.rootStanding } else { $null }
$rootSummary = Get-OanRootSummary -RepoRootPath $resolvedRepoRoot -CycleState $cycleState -TaskingStatusState $taskingStatusState -MasterThreadState $masterThreadState -FederationStatusState $federationStatusState -ReturnIntegrationState $returnIntegrationState -PreviousSummary $previousRootSummary

$bucketSummaries = @()
$candidateItems = @()
$deltaItems = @()
$compactionResults = @()

$previousCandidatesById = @{}
if ($null -ne $previousCandidateItemsState) {
    foreach ($candidate in @($previousCandidateItemsState.items)) {
        $previousCandidatesById[[string] $candidate.candidateId] = $candidate
    }
}

foreach ($bucket in @($consumptionPolicy.sourceBuckets)) {
    $bucketRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $bucket.bucketRoot)
    $stateEntries = Get-ResolvedStateEntries -BasePath $bucketRoot -RelativePaths @($bucket.authoritativeStatePaths)
    $rawAppendixItems = Get-RawAppendixItems -BucketRoot $bucketRoot -RawReportRoots @($bucket.rawReportRoots)
    $previousSummary = $null
    if ($null -ne $previousStandingSummary) {
        $previousSummary = @($previousStandingSummary.bucketStandings | Where-Object { [string] $_.bucketLabel -eq [string] $bucket.bucketLabel } | Select-Object -First 1)
        if ($previousSummary -is [System.Array]) {
            $previousSummary = if ($previousSummary.Count -gt 0) { $previousSummary[0] } else { $null }
        }
    }

    $summary = Get-BucketStatusSummary -BucketLabel ([string] $bucket.bucketLabel) -BucketRoot $bucketRoot -SourceScope ([string] $bucket.sourceScope) -StateEntries $stateEntries -RawAppendixItems $rawAppendixItems -PreviousSummary $previousSummary
    $appendixId = 'standing-summary::{0}::{1}' -f ([string] $bucket.bucketLabel).ToLowerInvariant().Replace(' ', '-'), $bundleId
    $continuityResult = Resolve-SourceBucketThreadContinuityRecord `
        -StatePath $threadContinuityStatePath `
        -BucketLabel ([string] $bucket.bucketLabel) `
        -SubjectKey ('standing::{0}' -f [string] $bucket.bucketLabel) `
        -ContinuityKey ('source-bucket-standing::{0}' -f ([string] $bucket.bucketLabel).ToLowerInvariant().Replace(' ', '-')) `
        -DiscourseOffice 'report_consumption' `
        -AppendixId $appendixId `
        -StandingHash ([string] $summary.standingHash) `
        -ContradictionDetected (Test-ContradictionStateRequiresReview -ContradictionState ([string] $summary.contradictionState)) `
        -ContinuityDisposition 'reuse'

    $summary | Add-Member -NotePropertyName threadId -NotePropertyValue ([string] $continuityResult.threadId) -Force
    $summary | Add-Member -NotePropertyName continuityAction -NotePropertyValue ([string] $continuityResult.action) -Force
    $bucketSummaries += $summary

    if ($summary.materialChange) {
        $deltaItems += [ordered]@{
            bucketLabel = [string] $summary.bucketLabel
            changedFields = @($summary.changedFields)
            standingHash = [string] $summary.standingHash
            nextLawfulAction = [string] $summary.nextLawfulAction
        }
    }

    $candidate = Get-CandidateRecord -BucketLabel ([string] $bucket.bucketLabel) -Summary $summary -PreviousCandidate ($previousCandidatesById[(Get-CandidateId -BucketLabel ([string] $bucket.bucketLabel))]) -IsFullResearchMode $isFullResearchMode
    $candidateItems += $candidate

    $compactionResults += [ordered]@{
        bucketLabel = [string] $bucket.bucketLabel
        result = Invoke-RawAppendixCompaction `
            -RawAppendixItems $rawAppendixItems `
            -MaterialChange ([bool] $summary.materialChange) `
            -PinnedForReview ([string] $candidate.consumerStandingClass -eq 'pinned_for_review') `
            -RawRetentionHours ([int] $consumptionPolicy.retentionPolicy.rawRetentionHours) `
            -SkipPruningRequested ([bool] $SkipPruning)
    }
}

$rootCandidate = Get-CandidateRecord -BucketLabel 'OAN Tech Stack' -Summary $rootSummary -PreviousCandidate ($previousCandidatesById[(Get-CandidateId -BucketLabel 'OAN Tech Stack')]) -IsFullResearchMode $isFullResearchMode
$candidateItems = @($rootCandidate) + @($candidateItems)

$currentStandingSummaryPayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $utcNow.ToString('o')
    mode = if ($isFullResearchMode) { 'full_research' } else { 'hourly_consumption' }
    rootStanding = $rootSummary
    bucketStandings = @($bucketSummaries)
    deltaCount = @($deltaItems).Count
}

$candidateItemsPayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $utcNow.ToString('o')
    mode = if ($isFullResearchMode) { 'full_research' } else { 'hourly_consumption' }
    items = @($candidateItems)
}

$consumptionReceiptPayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $utcNow.ToString('o')
    runMode = if ($isFullResearchMode) { 'full_research' } else { 'hourly_consumption' }
    bundleId = $bundleId
    bucketCount = @($bucketSummaries).Count
    deltaCount = @($deltaItems).Count
    fullResearchBoundary = $isFullResearchMode
    compactionResults = @(
        $compactionResults |
        ForEach-Object {
            [ordered]@{
                bucketLabel = [string] $_.bucketLabel
                removedCount = @($_.result.removedPaths).Count
                keptCount = [int] $_.result.keptCount
            }
        }
    )
}

$deltaPayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $utcNow.ToString('o')
    deltas = @($deltaItems)
}

Write-JsonFile -Path $currentStandingSummaryStatePath -Value $currentStandingSummaryPayload
Write-JsonFile -Path $currentCandidateGelItemsStatePath -Value $candidateItemsPayload
Write-JsonFile -Path (Join-Path $bundleRoot 'consumption-receipt.json') -Value $consumptionReceiptPayload
Write-JsonFile -Path (Join-Path $bundleRoot 'delta-since-last-summary.json') -Value $deltaPayload

$standingMarkdown = @(
    '# Current Source-Bucket Standing Summary',
    '',
    ('- Generated at (UTC): `{0}`' -f $currentStandingSummaryPayload.generatedAtUtc),
    ('- Mode: `{0}`' -f $currentStandingSummaryPayload.mode),
    ('- Root posture: `{0}`' -f [string] $rootSummary.status),
    ('- Root next lawful action: `{0}`' -f [string] $rootSummary.nextLawfulAction),
    ('- Root repo worktree: `{0}`' -f [string] $rootSummary.repoWorktreeState),
    ('- Delta count: `{0}`' -f [string] $currentStandingSummaryPayload.deltaCount),
    ''
)
foreach ($summary in @($bucketSummaries)) {
    $standingMarkdown += @(
        ('## {0}' -f [string] $summary.bucketLabel),
        '',
        ('- Status: `{0}`' -f [string] $summary.status),
        ('- Next lawful action: `{0}`' -f [string] $summary.nextLawfulAction),
        ('- Milestone: `{0}`' -f [string] $summary.milestone),
        ('- Repo worktree: `{0}`' -f [string] $summary.repoWorktreeState),
        ('- Continuity thread: `{0}` ({1})' -f [string] $summary.threadId, [string] $summary.continuityAction),
        ('- Material change: `{0}`' -f [string] $summary.materialChange),
        ('- Changed fields: `{0}`' -f $(if (@($summary.changedFields).Count -gt 0) { @($summary.changedFields) -join ', ' } else { 'none' })),
        ('- Blockers: `{0}`' -f $(if (@($summary.blockerSet).Count -gt 0) { @($summary.blockerSet) -join ' | ' } else { 'none' })),
        ''
    )
}
Write-MarkdownFile -Path $currentStandingSummaryMarkdownPath -Lines $standingMarkdown

$candidateMarkdown = @(
    '# Current Candidate GEL Items',
    '',
    ('- Generated at (UTC): `{0}`' -f $candidateItemsPayload.generatedAtUtc),
    ('- Mode: `{0}`' -f $candidateItemsPayload.mode),
    ''
)
foreach ($candidate in @($candidateItems)) {
    $candidateMarkdown += @(
        ('## {0}' -f [string] $candidate.candidateId),
        '',
        ('- Predicate: `{0}`' -f [string] $candidate.candidatePredicate),
        ('- Source scope: `{0}`' -f [string] $candidate.sourceScope),
        ('- Formation stage: `{0}`' -f [string] $candidate.formationStage),
        ('- Discernment status: `{0}`' -f [string] $candidate.discernmentStatus),
        ('- Carry-forward eligibility: `{0}`' -f [string] $candidate.carryForwardEligibility),
        ('- Consumer standing: `{0}`' -f [string] $candidate.consumerStandingClass),
        ('- Enhancement state: `{0}`' -f [string] $candidate.enhancementState),
        ('- Stability witness count: `{0}`' -f [string] $candidate.stabilityWitnessCount),
        ('- Blocked by: `{0}`' -f $(if (@($candidate.blockedBy).Count -gt 0) { @($candidate.blockedBy) -join ' | ' } else { 'none' })),
        ''
    )
}
Write-MarkdownFile -Path $currentCandidateGelItemsMarkdownPath -Lines $candidateMarkdown

if ($isFullResearchMode) {
    Write-JsonFile -Path (Join-Path $dailyBundleRoot 'bucket-standing-summary.json') -Value $currentStandingSummaryPayload
    Write-MarkdownFile -Path (Join-Path $dailyBundleRoot 'bucket-standing-summary.md') -Lines $standingMarkdown
    Write-JsonFile -Path (Join-Path $dailyBundleRoot 'candidate-gel-items.json') -Value $candidateItemsPayload
    Write-MarkdownFile -Path (Join-Path $dailyBundleRoot 'candidate-gel-items.md') -Lines $candidateMarkdown

    $reviewPacketLines = @(
        '# Simulated GEL Review Packet',
        '',
        ('- Generated at (UTC): `{0}`' -f $utcNow.ToString('o')),
        ('- Closed review window: `12:00 PM -> 12:00 PM` local'),
        ('- Root posture: `{0}`' -f [string] $rootSummary.status),
        ('- Root next lawful action: `{0}`' -f [string] $rootSummary.nextLawfulAction),
        ('- Candidate count: `{0}`' -f [string] @($candidateItems).Count),
        ''
    )
    foreach ($candidate in @($candidateItems)) {
        $reviewPacketLines += @(
            ('## {0}' -f [string] $candidate.candidateId),
            '',
            ('- Predicate: `{0}`' -f [string] $candidate.candidatePredicate),
            ('- Standing: `{0}`' -f [string] $candidate.consumerStandingClass),
            ('- Discernment: `{0}`' -f [string] $candidate.discernmentStatus),
            ('- Carry-forward reason: `{0}`' -f [string] $candidate.carryForwardReason),
            ('- Blocked by: `{0}`' -f $(if (@($candidate.blockedBy).Count -gt 0) { @($candidate.blockedBy) -join ' | ' } else { 'none' })),
            ''
        )
    }
    Write-MarkdownFile -Path (Join-Path $dailyBundleRoot ([string] $consumptionPolicy.dailyReviewPacketMarkdownName)) -Lines $reviewPacketLines
}

$reportConsumptionStatePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $utcNow.ToString('o')
    runMode = if ($isFullResearchMode) { 'full_research' } else { 'hourly_consumption' }
    bundleId = $bundleId
    bundlePath = $bundleRoot
    dailyBundlePath = if ($isFullResearchMode) { $dailyBundleRoot } else { $null }
    lastRunUtc = $utcNow.ToString('o')
    nextRunUtc = (Get-NextHourlyAnchorUtc -Minute ([int] $schedule.reportConsumptionMinute)).ToString('o')
    nextFullResearchRunUtc = (Get-NextDailyAnchorUtc -Hour ([int] $schedule.fullResearchRunHourLocal) -Minute ([int] $schedule.fullResearchRunMinuteLocal)).ToString('o')
    deltaCount = @($deltaItems).Count
    fullResearchBoundary = $isFullResearchMode
    currentStandingSummaryStatePath = $currentStandingSummaryStatePath
    currentCandidateGelItemsStatePath = $currentCandidateGelItemsStatePath
}
Write-JsonFile -Path $reportConsumptionStatePath -Value $reportConsumptionStatePayload

Write-Host ('[source-bucket-report-consumption] State: {0}' -f $reportConsumptionStatePath)
$reportConsumptionStatePath
