function Read-SourceBucketThreadJsonFileOrNull {
    param([string] $Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return $null
    }

    return Get-Content -Raw -LiteralPath $Path | ConvertFrom-Json
}

function Write-SourceBucketThreadJsonFile {
    param(
        [string] $Path,
        [object] $Value
    )

    $directory = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($directory)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }

    $Value | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $Path -Encoding utf8
}

function Get-SourceBucketStandingHash {
    param([object] $Value)

    $json = $Value | ConvertTo-Json -Depth 12 -Compress
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($json)
    $hasher = [System.Security.Cryptography.SHA256]::Create()
    try {
        $hashBytes = $hasher.ComputeHash($bytes)
        return ([System.BitConverter]::ToString($hashBytes)).Replace('-', '').ToLowerInvariant()
    }
    finally {
        $hasher.Dispose()
    }
}

function Resolve-SourceBucketThreadContinuityRecord {
    param(
        [string] $StatePath,
        [string] $BucketLabel,
        [string] $SubjectKey,
        [string] $ContinuityKey,
        [string] $DiscourseOffice,
        [string] $AppendixId,
        [string] $StandingHash,
        [bool] $ContradictionDetected = $false,
        [ValidateSet('reuse', 'successor', 'break')]
        [string] $ContinuityDisposition = 'reuse',
        [string] $SuccessorReason
    )

    $nowUtc = (Get-Date).ToUniversalTime().ToString('o')
    $state = Read-SourceBucketThreadJsonFileOrNull -Path $StatePath
    if ($null -eq $state) {
        $state = [ordered]@{
            schemaVersion = 1
            generatedAtUtc = $nowUtc
            records = @()
        }
    }

    $records = @($state.records)
    $currentRecord = @(
        $records |
        Where-Object {
            [string] $_.bucketLabel -eq $BucketLabel -and
            [string] $_.continuityKey -eq $ContinuityKey -and
            -not [bool] $_.sealed
        } |
        Select-Object -First 1
    )
    if ($currentRecord -is [System.Array]) {
        $currentRecord = if ($currentRecord.Count -gt 0) { $currentRecord[0] } else { $null }
    }

    $requiresSuccessor = $false
    if ($null -ne $currentRecord) {
        $requiresSuccessor = $ContradictionDetected -or
            $ContinuityDisposition -ne 'reuse' -or
            -not [string]::Equals([string] $currentRecord.subjectKey, $SubjectKey, [System.StringComparison]::Ordinal) -or
            -not [string]::Equals([string] $currentRecord.discourseOffice, $DiscourseOffice, [System.StringComparison]::Ordinal) -or
            [bool] $currentRecord.sealed
    }

    $action = 'reuse'
    if ($null -eq $currentRecord) {
        $action = 'opened'
        $threadId = '{0}::{1}' -f $ContinuityKey, ((Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ'))
        $currentRecord = [ordered]@{
            continuityKey = $ContinuityKey
            currentThreadId = $threadId
            lastAppendixId = $AppendixId
            lastStandingHash = $StandingHash
            sealed = $false
            successorThreadId = $null
            successorReason = $null
            bucketLabel = $BucketLabel
            discourseOffice = $DiscourseOffice
            subjectKey = $SubjectKey
            lastResolvedAtUtc = $nowUtc
        }
        $records += $currentRecord
    } elseif ($requiresSuccessor) {
        $action = if ($ContinuityDisposition -eq 'break') { 'break' } else { 'successor' }
        $newThreadId = '{0}::{1}' -f $ContinuityKey, ((Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ'))
        $currentRecord.sealed = $true
        $currentRecord.successorThreadId = $newThreadId
        $currentRecord.successorReason = if (-not [string]::IsNullOrWhiteSpace($SuccessorReason)) {
            $SuccessorReason
        } elseif ($ContradictionDetected) {
            'contradiction-requires-adjudication'
        } else {
            'continuity-lawful-successor'
        }
        $currentRecord.lastResolvedAtUtc = $nowUtc

        $records += [ordered]@{
            continuityKey = $ContinuityKey
            currentThreadId = $newThreadId
            lastAppendixId = $AppendixId
            lastStandingHash = $StandingHash
            sealed = $false
            successorThreadId = $null
            successorReason = $null
            bucketLabel = $BucketLabel
            discourseOffice = $DiscourseOffice
            subjectKey = $SubjectKey
            lastResolvedAtUtc = $nowUtc
        }

        $currentRecord = $records[-1]
    } else {
        $currentRecord.lastAppendixId = $AppendixId
        $currentRecord.lastStandingHash = $StandingHash
        $currentRecord.bucketLabel = $BucketLabel
        $currentRecord.discourseOffice = $DiscourseOffice
        $currentRecord.subjectKey = $SubjectKey
        $currentRecord.lastResolvedAtUtc = $nowUtc
    }

    $state.schemaVersion = 1
    $state.generatedAtUtc = $nowUtc
    $state.records = @($records)
    Write-SourceBucketThreadJsonFile -Path $StatePath -Value $state

    return [pscustomobject]@{
        action = $action
        continuityKey = $ContinuityKey
        threadId = [string] $currentRecord.currentThreadId
        statePath = $StatePath
        resolvedAtUtc = $nowUtc
    }
}
