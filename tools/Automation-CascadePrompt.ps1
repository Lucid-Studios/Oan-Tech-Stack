Set-StrictMode -Version Latest

function Get-AutomationCascadeOperatorPromptText {
    return @'
Proceed under bounded cascade authorization within the current scope of admissible operations. Continue execution without unnecessary interruption while remaining within defined authority, constraints, and prior standing.

Pause and request HITL intervention only when:

a decision exceeds current authorization or scope
required information or clarity falls below operational thresholds
a meaningful branch or irreversible action requires explicit operator intent

Otherwise, maintain continuity of execution and advance the system toward its defined objective, preserving lawful structure and traceable reasoning at each step.
'@.Trim()
}

function Get-BuildDispatchRootPromptText {
    return @'
You are OAN Build Dispatch for the active `OAN Mortalis V1.1.1` line.

Mission:
Act as the lawful root requester and admitting surface for active build needs.
Continuously reconcile the active `V1.1.1` build posture, publish bounded source-bucket work requests when needed, ingest only lawful bucket receipts, and advance the build toward executable completion without widening authority.

Authority:
- `OAN Mortalis V1.1.1` is the active build truth.
- `Documentation Repo` is the lawful source and first documentation-form compile surface.
- Source buckets are specialized work surfaces, not sovereign planners.
- Do not use `V1.0` policy or docs as active authority for new work.
- Do not mutate source buckets directly.

Reconcile first on every run:
- active carry-forward refinement posture
- versioned touchpoint matrix
- run-isolated build pathway state
- seeded governance state
- v111 enrichment pathway state
- companion-tool telemetry state
- master-thread orchestration state
- any existing `source-bucket-request-index`, `source-bucket-return-index`, `source-bucket-federation-status`, and `source-bucket-return-integration-status` surfaces

Current gate truth:
- Treat the carry-forward cleanup as complete.
- Treat the active gate as `V1.1.1` enrichment and seeded-governance admission.
- If seeded governance says `ready` but the run-isolated pathway still says `bring-seeded-governance-to-ready-state`, classify that as `clarify` and refresh the gate truth before publishing new downstream requests.

Primary duties:
1. Reconcile active `V1.1.1` build standing.
2. Determine whether the next lawful action is:
   - `clarify`
   - `frame-now`
   - `spec-now`
   - `implement-now`
   - `hold`
3. Publish bounded source-bucket work requests only when a real build need exists.
4. Target only the declared source buckets:
   - `IUTT SLI & Lisp`
   - `Latex Styles`
   - `Trivium Forum`
   - `Holographic Data Tool`
5. Ingest only lawful return receipts and classify their result.
6. Route admitted returns into:
   - `frame-now`
   - `spec-now`
   - `implement-now`
   - `hold`
7. Mutate active build surfaces only after lawful return and review standing exist.
8. Preserve the `seed-llm` pause seam and bounded actionable-surface target for `Oan.Runtime.Headless`.

Request law:
- Publish bounded requests using subject, predicate, and actions.
- Name the target bucket explicitly.
- Require triad receipts from the bucket:
  - doping header
  - receipt
  - notice
- Never treat raw repo drift from a bucket as build authority.

Return law:
- Accept only receipted returns.
- Classify bucket listener posture as:
  - `received`
  - `understood`
  - `admissible`
  - `actionable`
  - `withheld_or_escalated`
- Apply one governance action per return:
  - `admit`
  - `hold`
  - `narrow`
  - `defer`
  - `refuse`
  - `return`
  - `escalate`

Cadence duties:
- Continue hourly when lawful work exists.
- Emit concise hourly status when due.
- Refresh the six-hour `.audit` chain when due.
- Prepare the twenty-four-hour HITL digest when due.
- If nothing changed, preserve truthful standing and wait.

Stop and request HITL when:
- promotion, release, or executable admission crosses current standing
- seeded-governance or enrichment posture is materially ambiguous
- a bucket return would widen runtime, CME, or tool authority
- a classification ambiguity affects build admission
- a meaningful irreversible action is required

Completion rule:
Treat the objective as complete only when the current `V1.1.1` target component is buildable, tested, hygienic, executable in the intended OAN environment, and all remaining transition is human-governed admission.
'@.Trim()
}

function Get-AutomationCascadeOperatorPromptPayload {
    $promptText = Get-AutomationCascadeOperatorPromptText

    return [ordered]@{
        schemaVersion = 1
        label = 'bounded-cascade-authorization'
        promptText = $promptText
        continuationDirective = 'continue-without-unnecessary-interruption'
        hitlPauseConditions = @(
            'a decision exceeds current authorization or scope'
            'required information or clarity falls below operational thresholds'
            'a meaningful branch or irreversible action requires explicit operator intent'
        )
        closingDirective = 'maintain continuity of execution and advance the system toward its defined objective, preserving lawful structure and traceable reasoning at each step'
    }
}

function Get-BuildDispatchRootPromptPayload {
    $promptText = Get-BuildDispatchRootPromptText

    return [ordered]@{
        schemaVersion = 1
        label = 'oan-build-dispatch-root-prompt'
        promptText = $promptText
        governingLine = 'OAN Mortalis V1.1.1'
        dispatcherRole = 'lawful-root-requester-and-admitting-surface'
        nextActionClasses = @(
            'clarify'
            'frame-now'
            'spec-now'
            'implement-now'
            'hold'
        )
        declaredSourceBuckets = @(
            'IUTT SLI & Lisp'
            'Latex Styles'
            'Trivium Forum'
            'Holographic Data Tool'
        )
        completionRule = 'Treat the objective as complete only when the current V1.1.1 target component is buildable, tested, hygienic, executable in the intended OAN environment, and all remaining transition is human-governed admission.'
    }
}

function Get-AutomationCascadeInputPropertyValueOrNull {
    param(
        [object] $InputObject,
        [string[]] $PropertyNames
    )

    if ($null -eq $InputObject) {
        return $null
    }

    foreach ($propertyName in @($PropertyNames)) {
        if ([string]::IsNullOrWhiteSpace($propertyName)) {
            continue
        }

        if ($InputObject -is [System.Collections.IDictionary] -and $InputObject.Contains($propertyName)) {
            return $InputObject[$propertyName]
        }

        $property = $InputObject.PSObject.Properties[$propertyName]
        if ($null -ne $property) {
            return $property.Value
        }
    }

    return $null
}

function Get-AutomationCascadeBooleanPropertyOrNull {
    param(
        [object] $InputObject,
        [string[]] $PropertyNames
    )

    $value = Get-AutomationCascadeInputPropertyValueOrNull -InputObject $InputObject -PropertyNames $PropertyNames
    if ($null -eq $value) {
        return $null
    }

    if ($value -is [bool]) {
        return [bool] $value
    }

    $stringValue = [string] $value
    if ([string]::IsNullOrWhiteSpace($stringValue)) {
        return $null
    }

    $parsed = $false
    if ([bool]::TryParse($stringValue, [ref] $parsed)) {
        return $parsed
    }

    return $null
}

function Get-AutomationHitlVerificationAidPayload {
    param([object] $InputObject)

    $status = [string] (Get-AutomationCascadeInputPropertyValueOrNull -InputObject $InputObject -PropertyNames @(
        'currentStatus',
        'status',
        'lastKnownStatus',
        'currentAutomationPosture',
        'orchestrationState',
        'effectiveLifecycleState',
        'lifecycleState',
        'materializationState'
    ))
    if ([string]::IsNullOrWhiteSpace($status)) {
        $status = $null
    }

    $reasonCode = [string] (Get-AutomationCascadeInputPropertyValueOrNull -InputObject $InputObject -PropertyNames @(
        'reasonCode',
        'triggerReason',
        'hitl_reason',
        'supportReason'
    ))
    if ([string]::IsNullOrWhiteSpace($reasonCode)) {
        $reasonCode = $null
    }

    $nextAction = [string] (Get-AutomationCascadeInputPropertyValueOrNull -InputObject $InputObject -PropertyNames @(
        'nextAction',
        'next_lawful_action',
        'recommendedAction',
        'materializationNextAction'
    ))
    if ([string]::IsNullOrWhiteSpace($nextAction)) {
        $nextAction = $null
    }

    $actionClass = [string] (Get-AutomationCascadeInputPropertyValueOrNull -InputObject $InputObject -PropertyNames @(
        'actionClass',
        'currentAutomationActionClass'
    ))
    if ([string]::IsNullOrWhiteSpace($actionClass)) {
        $statusForActionClass = if ($null -ne $status) { $status } else { '' }
        switch ($statusForActionClass.ToLowerInvariant()) {
            'candidate-ready' { $actionClass = 'continue' }
            'clear-to-continue' { $actionClass = 'continue' }
            'hitl-required' { $actionClass = 'escalate' }
            'blocked' { $actionClass = 'suspend' }
            default { $actionClass = 'clarify' }
        }
    }

    $explicitHitlRequired = Get-AutomationCascadeBooleanPropertyOrNull -InputObject $InputObject -PropertyNames @(
        'hitl_required',
        'requiresImmediateHitl'
    )
    $requiredNow = if ($null -ne $explicitHitlRequired) {
        [bool] $explicitHitlRequired
    } else {
        $statusRequiresHitl = $status -eq 'hitl-required'
        $actionClassRequiresHitl = $actionClass -eq 'escalate'
        $textRequiresHitl = (
            ($null -ne $nextAction -and $nextAction.IndexOf('hitl', [System.StringComparison]::OrdinalIgnoreCase) -ge 0) -or
            ($null -ne $reasonCode -and $reasonCode.IndexOf('hitl', [System.StringComparison]::OrdinalIgnoreCase) -ge 0)
        )

        $statusRequiresHitl -or $actionClassRequiresHitl -or $textRequiresHitl
    }

    return [ordered]@{
        schemaVersion = 1
        label = 'hitl-verification-aid'
        purpose = 'operator-review-and-confirmation'
        aidState = if ($requiredNow) { 'required-now' } else { 'available-if-needed' }
        operatorExecutionRequired = $requiredNow
        executionLaw = 'automation may prepare the aid; operator reviews and executes the confirmation'
        triggerContext = [ordered]@{
            status = $status
            actionClass = $actionClass
            reasonCode = $reasonCode
            nextAction = $nextAction
        }
        reviewSequence = @(
            [ordered]@{
                id = 'received'
                prompt = 'Confirm the return has been received without auto-ratifying it.'
            }
            [ordered]@{
                id = 'understood'
                prompt = 'Confirm the intent, present state, and requested next move are understood.'
            }
            [ordered]@{
                id = 'admissible'
                prompt = 'Confirm the requested move is admissible under current standing, scope, and law.'
            }
            [ordered]@{
                id = 'actionable'
                prompt = 'Confirm the move is actionable now with available evidence, capability, and authority.'
            }
            [ordered]@{
                id = 'withheld_or_escalated'
                prompt = 'If any prior step fails, withhold, clarify, defer, refuse, or escalate instead of forcing continuation.'
            }
        )
        operatorOutcomes = @(
            'admit'
            'clarify'
            'defer'
            'refuse'
            'escalate'
        )
        completionRule = 'Only the operator may confirm direct HITL admission; automation may prepare but may not self-ratify.'
    }
}

function Add-AutomationCascadeOperatorPromptProperty {
    param(
        [Parameter(Mandatory = $true)]
        [object] $InputObject,
        [string] $PropertyName = 'operatorPrompt'
    )

    $existingProperty = $InputObject.PSObject.Properties[$PropertyName]
    if ($null -eq $existingProperty) {
        Add-Member -InputObject $InputObject -NotePropertyName $PropertyName -NotePropertyValue (Get-AutomationCascadeOperatorPromptPayload) -Force
    } else {
        $existingProperty.Value = Get-AutomationCascadeOperatorPromptPayload
    }

    $existingHitlProperty = $InputObject.PSObject.Properties['hitlVerificationAid']
    if ($null -eq $existingHitlProperty) {
        Add-Member -InputObject $InputObject -NotePropertyName 'hitlVerificationAid' -NotePropertyValue (Get-AutomationHitlVerificationAidPayload -InputObject $InputObject) -Force
    } else {
        $existingHitlProperty.Value = Get-AutomationHitlVerificationAidPayload -InputObject $InputObject
    }

    return $InputObject
}

function Add-BuildDispatchRootPromptProperty {
    param(
        [Parameter(Mandatory = $true)]
        [object] $InputObject,
        [string] $PropertyName = 'rootDispatchPrompt'
    )

    $existingProperty = $InputObject.PSObject.Properties[$PropertyName]
    if ($null -eq $existingProperty) {
        Add-Member -InputObject $InputObject -NotePropertyName $PropertyName -NotePropertyValue (Get-BuildDispatchRootPromptPayload) -Force
    } else {
        $existingProperty.Value = Get-BuildDispatchRootPromptPayload
    }

    return $InputObject
}

function Add-AutomationHitlVerificationMarkdownLines {
    param(
        [Parameter(Mandatory = $true)]
        [object] $Lines
    )

    [void] $Lines.Add('')
    [void] $Lines.Add('## HITL Verification Aid')
    [void] $Lines.Add('')
    [void] $Lines.Add('- This aid is operator-executed, not automation-ratified.')
    [void] $Lines.Add('- Review sequence: `received -> understood -> admissible -> actionable -> withheld_or_escalated`.')
    [void] $Lines.Add('- Operator outcomes: `admit`, `clarify`, `defer`, `refuse`, `escalate`.')
    [void] $Lines.Add('- Completion rule: only the operator may confirm direct HITL admission.')
}

function Add-AutomationCascadePromptMarkdownLines {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [AllowEmptyCollection()]
        [string[]] $MarkdownLines
    )

    $lines = New-Object System.Collections.Generic.List[string]
    foreach ($line in @($MarkdownLines)) {
        [void] $lines.Add([string] $line)
    }

    $existingHeadingIndex = -1
    for ($index = 0; $index -lt $lines.Count; $index++) {
        if ($lines[$index] -eq '## Operator Prompt' -or $lines[$index] -eq '## HITL Verification Aid') {
            $existingHeadingIndex = $index
            break
        }
    }

    if ($existingHeadingIndex -ge 0) {
        while ($lines.Count -gt $existingHeadingIndex) {
            $lines.RemoveAt($lines.Count - 1)
        }
    }

    if ($lines.Count -gt 0 -and -not [string]::IsNullOrWhiteSpace($lines[$lines.Count - 1])) {
        [void] $lines.Add('')
    }

    [void] $lines.Add('## Operator Prompt')
    [void] $lines.Add('')
    [void] $lines.Add('```text')
    foreach ($line in (Get-AutomationCascadeOperatorPromptText -split "`r?`n")) {
        [void] $lines.Add($line)
    }
    [void] $lines.Add('```')
    Add-AutomationHitlVerificationMarkdownLines -Lines $lines

    return [string[]] $lines.ToArray()
}

function Resolve-AutomationCascadeArtifactPath {
    param(
        [Parameter(Mandatory = $true)]
        [string] $RepoRoot,
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $CandidatePath
    )

    if ([string]::IsNullOrWhiteSpace($CandidatePath)) {
        return $null
    }

    $resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
    $resolvedAuditRoot = [System.IO.Path]::GetFullPath((Join-Path $resolvedRepoRoot '.audit'))
    try {
        $resolvedCandidate = if ([System.IO.Path]::IsPathRooted($CandidatePath)) {
            [System.IO.Path]::GetFullPath($CandidatePath)
        } else {
            [System.IO.Path]::GetFullPath((Join-Path $resolvedRepoRoot $CandidatePath))
        }
    }
    catch {
        return $null
    }

    if (-not (Test-Path -LiteralPath $resolvedCandidate)) {
        return $null
    }

    if (-not $resolvedCandidate.StartsWith($resolvedAuditRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        return $null
    }

    return $resolvedCandidate
}

function Get-AutomationCascadeArtifactFilePaths {
    param(
        [Parameter(Mandatory = $true)]
        [string] $RepoRoot,
        [Parameter(Mandatory = $true)]
        [object] $Value
    )

    $resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
    $results = New-Object System.Collections.Generic.List[string]
    $seen = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)

    function Add-ResolvedPath {
        param([string] $Path)

        if ([string]::IsNullOrWhiteSpace($Path)) {
            return
        }

        if ($seen.Add($Path)) {
            [void] $results.Add($Path)
        }
    }

    function Visit-Value {
        param([object] $InputValue)

        if ($null -eq $InputValue) {
            return
        }

        if ($InputValue -is [string]) {
            $resolved = Resolve-AutomationCascadeArtifactPath -RepoRoot $resolvedRepoRoot -CandidatePath $InputValue
            if ([string]::IsNullOrWhiteSpace($resolved)) {
                return
            }

            if (Test-Path -LiteralPath $resolved -PathType Container) {
                foreach ($file in Get-ChildItem -LiteralPath $resolved -Recurse -File -Include *.json,*.md) {
                    Add-ResolvedPath -Path $file.FullName
                }

                return
            }

            if ($resolved.EndsWith('.json', [System.StringComparison]::OrdinalIgnoreCase) -or
                $resolved.EndsWith('.md', [System.StringComparison]::OrdinalIgnoreCase)) {
                Add-ResolvedPath -Path $resolved
            }

            return
        }

        if ($InputValue -is [System.Collections.IDictionary]) {
            foreach ($entry in $InputValue.GetEnumerator()) {
                Visit-Value -InputValue $entry.Value
            }

            return
        }

        if (($InputValue -is [System.Collections.IEnumerable]) -and
            -not ($InputValue -is [string]) -and
            -not ($InputValue -is [System.Collections.IDictionary]) -and
            -not ($InputValue.PSObject.TypeNames -contains 'System.Management.Automation.PSCustomObject')) {
            foreach ($item in $InputValue) {
                Visit-Value -InputValue $item
            }

            return
        }

        foreach ($property in $InputValue.PSObject.Properties) {
            Visit-Value -InputValue $property.Value
        }
    }

    Visit-Value -InputValue $Value
    return [string[]] $results.ToArray()
}

function Set-AutomationCascadePromptOnJsonArtifact {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return $false
    }

    $content = Get-Content -Raw -LiteralPath $Path | ConvertFrom-Json
    if ($null -eq $content) {
        return $false
    }

    Add-AutomationCascadeOperatorPromptProperty -InputObject $content | Out-Null
    $content | ConvertTo-Json -Depth 32 | Set-Content -LiteralPath $Path -Encoding utf8
    return $true
}

function Set-AutomationCascadePromptOnMarkdownArtifact {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return $false
    }

    $existingLines = @()
    if (Test-Path -LiteralPath $Path -PathType Leaf) {
        $existingLines = @(Get-Content -LiteralPath $Path)
    }

    $updatedLines = Add-AutomationCascadePromptMarkdownLines -MarkdownLines $existingLines
    Set-Content -LiteralPath $Path -Value $updatedLines -Encoding utf8
    return $true
}

function Set-AutomationCascadePromptOnArtifacts {
    param(
        [Parameter(Mandatory = $true)]
        [string] $RepoRoot,
        [Parameter(Mandatory = $true)]
        [object] $Value
    )

    foreach ($artifactPath in @(Get-AutomationCascadeArtifactFilePaths -RepoRoot $RepoRoot -Value $Value)) {
        if ($artifactPath.EndsWith('.json', [System.StringComparison]::OrdinalIgnoreCase)) {
            [void] (Set-AutomationCascadePromptOnJsonArtifact -Path $artifactPath)
        } elseif ($artifactPath.EndsWith('.md', [System.StringComparison]::OrdinalIgnoreCase)) {
            [void] (Set-AutomationCascadePromptOnMarkdownArtifact -Path $artifactPath)
        }
    }
}
