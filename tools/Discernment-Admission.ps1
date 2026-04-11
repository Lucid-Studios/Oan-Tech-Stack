function Get-DiscernmentObjectPropertyValueOrNull {
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

function ConvertTo-DiscernmentBoolean {
    param([object] $Value)

    if ($null -eq $Value) {
        return $false
    }

    if ($Value -is [bool]) {
        return [bool] $Value
    }

    $parsed = $false
    if ([bool]::TryParse([string] $Value, [ref] $parsed)) {
        return $parsed
    }

    return $false
}

function Get-DiscernmentAdmissionEnvelope {
    param(
        [object] $State,
        [string] $DefaultRequestedStanding = 'unspecified-standing'
    )

    $requestedStanding = [string] (Get-DiscernmentObjectPropertyValueOrNull -InputObject $State -PropertyName 'requestedStanding')
    if ([string]::IsNullOrWhiteSpace($requestedStanding)) {
        $requestedStanding = $DefaultRequestedStanding
    }

    $action = [string] (Get-DiscernmentObjectPropertyValueOrNull -InputObject $State -PropertyName 'discernmentAction')
    if ([string]::IsNullOrWhiteSpace($action)) {
        $action = 'remain-provisional'
    }

    $standingSurfaceClass = [string] (Get-DiscernmentObjectPropertyValueOrNull -InputObject $State -PropertyName 'standingSurfaceClass')
    if ([string]::IsNullOrWhiteSpace($standingSurfaceClass)) {
        $standingSurfaceClass = 'rhetoric-bearing'
    }

    $promotionReceiptState = [string] (Get-DiscernmentObjectPropertyValueOrNull -InputObject $State -PropertyName 'promotionReceiptState')
    if ([string]::IsNullOrWhiteSpace($promotionReceiptState)) {
        $promotionReceiptState = 'insufficient-for-closure'
    }

    $receiptsSufficientForPromotion = ConvertTo-DiscernmentBoolean -Value (Get-DiscernmentObjectPropertyValueOrNull -InputObject $State -PropertyName 'receiptsSufficientForPromotion')
    $categoryErrorDetected = ConvertTo-DiscernmentBoolean -Value (Get-DiscernmentObjectPropertyValueOrNull -InputObject $State -PropertyName 'categoryErrorDetected')
    $promotionWithoutReceiptsDetected = ConvertTo-DiscernmentBoolean -Value (Get-DiscernmentObjectPropertyValueOrNull -InputObject $State -PropertyName 'promotionWithoutReceiptsDetected')

    $reason = [string] (Get-DiscernmentObjectPropertyValueOrNull -InputObject $State -PropertyName 'discernmentReason')
    if ([string]::IsNullOrWhiteSpace($reason)) {
        $reason = [string] (Get-DiscernmentObjectPropertyValueOrNull -InputObject $State -PropertyName 'reasonCode')
    }
    if ([string]::IsNullOrWhiteSpace($reason)) {
        $reason = 'discernment-state-unclassified'
    }

    $nextAction = [string] (Get-DiscernmentObjectPropertyValueOrNull -InputObject $State -PropertyName 'nextAction')
    $evaluation = Get-DiscernmentObjectPropertyValueOrNull -InputObject $State -PropertyName 'discernmentEvaluation'

    $isClosureBearing = $standingSurfaceClass -eq 'closure-bearing'
    $isAdmitted = $action -eq 'admit' -and
        $isClosureBearing -and
        $promotionReceiptState -eq 'sufficient' -and
        $receiptsSufficientForPromotion
    $isHeld = $action -eq 'hold'
    $isRefused = $action -eq 'refuse'
    $isProvisional = $action -eq 'remain-provisional'

    return [pscustomobject] [ordered]@{
        statePresent = $null -ne $State
        requestedStanding = $requestedStanding
        action = $action
        standingSurfaceClass = $standingSurfaceClass
        promotionReceiptState = $promotionReceiptState
        receiptsSufficientForPromotion = $receiptsSufficientForPromotion
        categoryErrorDetected = $categoryErrorDetected
        promotionWithoutReceiptsDetected = $promotionWithoutReceiptsDetected
        reason = $reason
        nextAction = $nextAction
        evaluation = $evaluation
        isClosureBearing = $isClosureBearing
        isAdmitted = $isAdmitted
        isHeld = $isHeld
        isRefused = $isRefused
        isProvisional = $isProvisional
        blocksDownstream = $isHeld -or $isRefused
        awaitsPromotion = -not $isAdmitted -and -not ($isHeld -or $isRefused)
    }
}
