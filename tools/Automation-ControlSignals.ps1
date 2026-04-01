Set-StrictMode -Version Latest

function Get-AutomationActionClassFromStatus {
    param(
        [string] $Status,
        [string] $DefaultValue = 'clarify'
    )

    if ([string]::IsNullOrWhiteSpace($Status)) {
        return $DefaultValue
    }

    switch ($Status.ToLowerInvariant()) {
        'candidate-ready' { return 'continue' }
        'hitl-required' { return 'escalate' }
        'blocked' { return 'suspend' }
        default { return $DefaultValue }
    }
}

function Get-AutomationControlActionClasses {
    return @(
        'continue'
        'clarify'
        'suspend'
        'escalate'
        'return'
    )
}

function Get-AutomationControlContractBarriers {
    return @(
        'new_tool_creation'
        'new_tool_adoption_outside_current_qualification'
        'new_domain_entry'
        'governance_sensitive_change'
        'destructive_or_irreversible_action'
        'final_commit_or_release_decision'
    )
}

function New-AutomationDopingHeaderPayload {
    param(
        [Parameter(Mandatory = $true)]
        [string] $RunId,
        [Parameter(Mandatory = $true)]
        [string] $Lane,
        [string] $Standing = 'admitted',
        [string] $AuthorityClass = 'bounded-cascade',
        [Parameter(Mandatory = $true)]
        [string] $Objective,
        [string] $Phase,
        [string] $Milestone,
        [string[]] $Artifacts = @(),
        [string[]] $AuthorizedTools = @(),
        [string[]] $VerificationExpectations = @(),
        [string] $UpstreamRoot = 'OAN Tech Stack',
        [bool] $RequiresRootReconciliation = $true,
        [string[]] $ForwardConditioningNotes = @()
    )

    return [ordered]@{
        schema = 'oan.automation.doping-header.v0.1'
        kind = 'doping_header'
        run_id = $RunId
        lane = $Lane
        standing = $Standing
        authority_class = $AuthorityClass
        admitted_scope = [ordered]@{
            objective = $Objective
            phase = if ([string]::IsNullOrWhiteSpace($Phase)) { $null } else { $Phase }
            milestone = if ([string]::IsNullOrWhiteSpace($Milestone)) { $null } else { $Milestone }
            artifacts = @($Artifacts | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
        }
        authorized_tools = @($AuthorizedTools | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
        action_classes = @(Get-AutomationControlActionClasses)
        contract_barriers = @(Get-AutomationControlContractBarriers)
        verification_expectations = @($VerificationExpectations | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
        forward_conditioning = [ordered]@{
            upstream_root = $UpstreamRoot
            requires_root_reconciliation = $RequiresRootReconciliation
            notes = @($ForwardConditioningNotes | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
        }
    }
}

function New-AutomationReceiptPayload {
    param(
        [Parameter(Mandatory = $true)]
        [string] $ReceiptId,
        [Parameter(Mandatory = $true)]
        [string] $RunId,
        [Parameter(Mandatory = $true)]
        [string] $Lane,
        [Parameter(Mandatory = $true)]
        [string] $Summary,
        [Parameter(Mandatory = $true)]
        [string] $Status,
        [Parameter(Mandatory = $true)]
        [string] $StandingResult,
        [string[]] $ArtifactsTouched = @(),
        [Parameter(Mandatory = $true)]
        [object] $Verification,
        [Parameter(Mandatory = $true)]
        [string] $CarryForwardClass,
        [string[]] $NextLawfulActions = @(),
        [bool] $HitlRequired = $false,
        [string] $HitlReason = ''
    )

    return [ordered]@{
        schema = 'oan.automation.receipt.v0.1'
        kind = 'receipt'
        receipt_id = $ReceiptId
        run_id = $RunId
        lane = $Lane
        summary = $Summary
        status = $Status
        standing_result = $StandingResult
        artifacts_touched = @($ArtifactsTouched | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
        verification = $Verification
        carry_forward_class = $CarryForwardClass
        next_lawful_actions = @($NextLawfulActions | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
        hitl_required = $HitlRequired
        hitl_reason = if ($HitlRequired) { $HitlReason } else { '' }
    }
}

function New-AutomationNoticePayload {
    param(
        [Parameter(Mandatory = $true)]
        [string] $NoticeId,
        [Parameter(Mandatory = $true)]
        [string] $Lane,
        [Parameter(Mandatory = $true)]
        [string] $Type,
        [Parameter(Mandatory = $true)]
        [string] $Status,
        [Parameter(Mandatory = $true)]
        [string] $Summary,
        [string[]] $DependsOn = @(),
        [string[]] $EnablesWhenCleared = @(),
        [Parameter(Mandatory = $true)]
        [string] $NextLawfulAction,
        [bool] $HitlRequired = $false
    )

    return [ordered]@{
        schema = 'oan.automation.notice.v0.1'
        kind = 'notice'
        notice_id = $NoticeId
        lane = $Lane
        type = $Type
        status = $Status
        summary = $Summary
        depends_on = @($DependsOn | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
        enables_when_cleared = @($EnablesWhenCleared | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
        next_lawful_action = $NextLawfulAction
        hitl_required = $HitlRequired
    }
}
