Set-StrictMode -Version Latest

function Get-SeededGovernancePropertyValueOrNull {
    param(
        [object] $InputObject,
        [string[]] $PropertyNames
    )

    if ($null -eq $InputObject) {
        return $null
    }

    foreach ($propertyName in $PropertyNames) {
        $property = $InputObject.PSObject.Properties[$propertyName]
        if ($null -ne $property) {
            return $property.Value
        }
    }

    return $null
}

function Get-SeededGovernanceBuildAdmission {
    param(
        [object] $SeededGovernanceState,
        [object] $CyclePolicy
    )

    $disposition = [string] (Get-SeededGovernancePropertyValueOrNull -InputObject $SeededGovernanceState -PropertyNames @('disposition'))
    $dispositionReason = [string] (Get-SeededGovernancePropertyValueOrNull -InputObject $SeededGovernanceState -PropertyNames @('dispositionReason'))
    $readyState = [string] (Get-SeededGovernancePropertyValueOrNull -InputObject $SeededGovernanceState -PropertyNames @('readyState'))
    $buildAdmissionState = [string] (Get-SeededGovernancePropertyValueOrNull -InputObject $SeededGovernanceState -PropertyNames @('buildAdmissionState'))
    $buildAdmissionReason = [string] (Get-SeededGovernancePropertyValueOrNull -InputObject $SeededGovernanceState -PropertyNames @('buildAdmissionReason'))

    if (-not [string]::IsNullOrWhiteSpace($buildAdmissionState)) {
        $isAdmitted = $buildAdmissionState -eq 'admitted-local-bounded'
        return [ordered]@{
            disposition = $disposition
            dispositionReason = $dispositionReason
            readyState = $readyState
            buildAdmissionState = $buildAdmissionState
            buildAdmissionReason = $buildAdmissionReason
            buildAdmissionIsAdmitted = $isAdmitted
            buildAdmissionClarifyRequired = (-not $isAdmitted -and $readyState -eq 'ready')
        }
    }

    $policy = Get-SeededGovernancePropertyValueOrNull -InputObject $CyclePolicy -PropertyNames @('seededGovernancePolicy')
    $admittedReadyStates = @(
        Get-SeededGovernancePropertyValueOrNull -InputObject $policy -PropertyNames @('buildAdmittedReadyStates')
    )
    $admittedDispositions = @(
        Get-SeededGovernancePropertyValueOrNull -InputObject $policy -PropertyNames @('buildAdmittedDispositions')
    )
    $admittedDeferredReasons = @(
        Get-SeededGovernancePropertyValueOrNull -InputObject $policy -PropertyNames @('buildAdmittedDeferredReasons')
    )

    if ($admittedReadyStates.Count -eq 0) {
        $admittedReadyStates = @('ready')
    }

    if ($admittedDispositions.Count -eq 0) {
        $admittedDispositions = @('Accepted')
    }

    $readyForAdmission = $readyState -in $admittedReadyStates
    $acceptedByDisposition = $disposition -in $admittedDispositions
    $admittedDeferred = (
        $readyForAdmission -and
        $disposition -eq 'Deferred' -and
        $dispositionReason -in $admittedDeferredReasons
    )

    if ($readyForAdmission -and ($acceptedByDisposition -or $admittedDeferred)) {
        $buildAdmissionState = 'admitted-local-bounded'
        if ($acceptedByDisposition) {
            $buildAdmissionReason = 'seeded-governance-accepted-for-build'
        } else {
            $buildAdmissionReason = 'seeded-governance-ready-with-research-routed-preflight'
        }
    } elseif ($readyState -eq 'ready') {
        $buildAdmissionState = 'clarify-required'
        $buildAdmissionReason = 'seeded-governance-ready-but-build-admission-unresolved'
    } elseif ($null -eq $SeededGovernanceState) {
        $buildAdmissionState = 'missing'
        $buildAdmissionReason = 'seeded-governance-state-missing'
    } else {
        $buildAdmissionState = 'withheld'
        $buildAdmissionReason = 'seeded-governance-not-yet-build-admitted'
    }

    $isAdmitted = $buildAdmissionState -eq 'admitted-local-bounded'
    return [ordered]@{
        disposition = $disposition
        dispositionReason = $dispositionReason
        readyState = $readyState
        buildAdmissionState = $buildAdmissionState
        buildAdmissionReason = $buildAdmissionReason
        buildAdmissionIsAdmitted = $isAdmitted
        buildAdmissionClarifyRequired = (-not $isAdmitted -and $readyState -eq 'ready')
    }
}
