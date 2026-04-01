param(
    [string] $RepoRoot,
    [string] $CyclePolicyPath = 'OAN Mortalis V1.1.1/build/local-automation-cycle.json'
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

function Resolve-CompanionToolRootPath {
    param(
        [string] $BasePath,
        [string] $ToolRootName
    )

    if ([string]::IsNullOrWhiteSpace($ToolRootName)) {
        return $null
    }

    $resolvedBasePath = [System.IO.Path]::GetFullPath($BasePath)
    $workspaceParent = Split-Path -Parent $resolvedBasePath
    return [System.IO.Path]::GetFullPath((Join-Path $workspaceParent $ToolRootName))
}

function Get-LogicalExternalPath {
    param(
        [string] $ToolRootPath,
        [string] $ToolLabel,
        [object] $CandidatePath
    )

    if ([string]::IsNullOrWhiteSpace($ToolRootPath) -or [string]::IsNullOrWhiteSpace([string] $CandidatePath)) {
        return $null
    }

    try {
        $resolvedCandidate = [System.IO.Path]::GetFullPath([string] $CandidatePath)
    }
    catch {
        return [string] $CandidatePath
    }

    if (-not $resolvedCandidate.StartsWith($ToolRootPath, [System.StringComparison]::OrdinalIgnoreCase)) {
        return [string] $CandidatePath
    }

    $relativePath = Get-RelativePathString -BasePath $ToolRootPath -TargetPath $resolvedCandidate
    if ([string]::IsNullOrWhiteSpace($relativePath)) {
        return $ToolLabel
    }

    return ('{0}/{1}' -f $ToolLabel, $relativePath.Replace('\', '/'))
}

function Get-CompanionToolTelemetrySummary {
    param(
        [string] $ResolvedRootPath,
        [string] $ToolLabel,
        [string] $SurfaceClass
    )

    $repoPresent = Test-Path -LiteralPath $ResolvedRootPath -PathType Container
    $auditRootPath = if ($repoPresent) { Join-Path $ResolvedRootPath '.audit' } else { $null }
    $auditRootPresent = if ($null -ne $auditRootPath) { Test-Path -LiteralPath $auditRootPath -PathType Container } else { $false }
    $cycleStatePath = if ($null -ne $auditRootPath) { Join-Path $auditRootPath 'state\local-automation-cycle.json' } else { $null }
    $taskingStatusPath = if ($null -ne $auditRootPath) { Join-Path $auditRootPath 'state\local-automation-tasking-status.json' } else { $null }
    $orchestrationStatePath = if ($null -ne $auditRootPath) { Join-Path $auditRootPath 'state\master-thread-orchestration-status.json' } else { $null }

    $cycleState = if ($null -ne $cycleStatePath) { Read-JsonFileOrNull -Path $cycleStatePath } else { $null }
    $taskingStatusState = if ($null -ne $taskingStatusPath) { Read-JsonFileOrNull -Path $taskingStatusPath } else { $null }
    $orchestrationState = if ($null -ne $orchestrationStatePath) { Read-JsonFileOrNull -Path $orchestrationStatePath } else { $null }

    $docsPresent = if ($repoPresent) { Test-Path -LiteralPath (Join-Path $ResolvedRootPath 'docs') -PathType Container } else { $false }
    $packagePresent = if ($repoPresent) { Test-Path -LiteralPath (Join-Path $ResolvedRootPath 'package.json') -PathType Leaf } else { $false }
    $conferenceSurfacePresent = if ($repoPresent) { Test-Path -LiteralPath (Join-Path $ResolvedRootPath 'apps\web') -PathType Container } else { $false }

    $toolState = 'tool-root-missing'
    $reasonCode = 'companion-tool-root-missing'
    $nextAction = 'attach-tool-root-before-telemetry-ingress'

    if ($repoPresent -and $null -ne $cycleState) {
        $toolState = 'telemetry-present'
        $reasonCode = 'companion-tool-local-automation-cycle-present'
        $nextAction = [string] (Get-ObjectPropertyValueOrNull -InputObject $cycleState -PropertyName 'recommendedAction')
        if ([string]::IsNullOrWhiteSpace($nextAction)) {
            $nextAction = 'continue-mechanically'
        }
    } elseif ($repoPresent -and $auditRootPresent) {
        $toolState = 'awaiting-cycle-state'
        $reasonCode = 'companion-tool-audit-root-present-cycle-state-missing'
        $nextAction = 'emit-local-automation-cycle'
    } elseif ($repoPresent) {
        $toolState = 'awaiting-audit-lane'
        $reasonCode = 'companion-tool-repo-present-audit-lane-missing'
        $nextAction = 'emit-bounded-audit-telemetry'
    }

    return [ordered]@{
        toolLabel = $ToolLabel
        surfaceClass = $SurfaceClass
        repoPresent = $repoPresent
        auditRootPresent = $auditRootPresent
        telemetryAvailable = ($null -ne $cycleState)
        toolState = $toolState
        reasonCode = $reasonCode
        nextAction = $nextAction
        cycleStatus = [string] (Get-ObjectPropertyValueOrNull -InputObject $cycleState -PropertyName 'status')
        recommendedAction = [string] (Get-ObjectPropertyValueOrNull -InputObject $cycleState -PropertyName 'recommendedAction')
        developmentPosture = [string] (Get-ObjectPropertyValueOrNull -InputObject $cycleState -PropertyName 'developmentPosture')
        lastBundleId = [string] (Get-ObjectPropertyValueOrNull -InputObject $cycleState -PropertyName 'lastBundleId')
        lastDigestBundleId = [string] (Get-ObjectPropertyValueOrNull -InputObject $cycleState -PropertyName 'lastDigestBundleId')
        lastBundlePath = Get-LogicalExternalPath -ToolRootPath $ResolvedRootPath -ToolLabel $ToolLabel -CandidatePath (Get-ObjectPropertyValueOrNull -InputObject $cycleState -PropertyName 'lastBundlePath')
        lastDigestBundlePath = Get-LogicalExternalPath -ToolRootPath $ResolvedRootPath -ToolLabel $ToolLabel -CandidatePath (Get-ObjectPropertyValueOrNull -InputObject $cycleState -PropertyName 'lastDigestBundlePath')
        taskingStatusPresent = ($null -ne $taskingStatusState)
        activeTaskMapId = [string] (Get-ObjectPropertyValueOrNull -InputObject $taskingStatusState -PropertyName 'activeTaskMapId')
        masterThreadStatePresent = ($null -ne $orchestrationState)
        masterThreadState = [string] (Get-ObjectPropertyValueOrNull -InputObject $orchestrationState -PropertyName 'supportState')
        masterThreadPublishReady = [string] (Get-ObjectPropertyValueOrNull -InputObject $orchestrationState -PropertyName 'publishReady')
        docsPresent = $docsPresent
        packagePresent = $packagePresent
        conferenceSurfacePresent = $conferenceSurfacePresent
    }
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$cyclePolicy = Get-Content -Raw -LiteralPath $resolvedCyclePolicyPath | ConvertFrom-Json

$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.companionToolTelemetryOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.companionToolTelemetryStatePath)

$holographicDataToolRootPath = Resolve-CompanionToolRootPath -BasePath $resolvedRepoRoot -ToolRootName ([string] $cyclePolicy.optionalHopngToolRoot)
$triviumForumRootPath = Resolve-CompanionToolRootPath -BasePath $resolvedRepoRoot -ToolRootName ([string] $cyclePolicy.optionalTriviumForumToolRoot)

$holographicDataToolTelemetry = Get-CompanionToolTelemetrySummary -ResolvedRootPath $holographicDataToolRootPath -ToolLabel 'Holographic Data Tool' -SurfaceClass 'optional-bounded-support-tool'
$triviumForumTelemetry = Get-CompanionToolTelemetrySummary -ResolvedRootPath $triviumForumRootPath -ToolLabel 'Trivium Forum' -SurfaceClass 'optional-bounded-conference-tool'

$companionToolTelemetryState = 'awaiting-companion-tool-roots'
$reasonCode = 'companion-tool-roots-missing'
$nextAction = 'attach-companion-tool-roots-before-telemetry-ingress'

if ($holographicDataToolTelemetry.repoPresent -or $triviumForumTelemetry.repoPresent) {
    if ($holographicDataToolTelemetry.telemetryAvailable -and $triviumForumTelemetry.telemetryAvailable) {
        $companionToolTelemetryState = 'companion-tool-telemetry-present'
        $reasonCode = 'companion-tool-telemetry-present'
        $nextAction = 'continue-v111-enrichment-with-companion-telemetry'
    } elseif ($holographicDataToolTelemetry.telemetryAvailable -or $triviumForumTelemetry.telemetryAvailable) {
        $companionToolTelemetryState = 'partial-companion-tool-telemetry'
        if ($holographicDataToolTelemetry.telemetryAvailable) {
            $reasonCode = 'hdt-telemetry-present-trivium-telemetry-pending'
        } else {
            $reasonCode = 'trivium-telemetry-present-hdt-telemetry-pending'
        }

        $nextAction = 'continue-v111-enrichment-and-admit-pending-tool-telemetry-when-emitted'
    } else {
        $companionToolTelemetryState = 'companion-tools-present-awaiting-telemetry'
        $reasonCode = 'companion-tools-present-audit-telemetry-pending'
        $nextAction = 'emit-bounded-companion-tool-audit-telemetry'
    }
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKeyParts = @()
foreach ($bundleId in @(
    [string] $holographicDataToolTelemetry.lastBundleId,
    [string] $triviumForumTelemetry.lastBundleId
)) {
    if (-not [string]::IsNullOrWhiteSpace($bundleId)) {
        $bundleKeyParts += $bundleId
    }
}

$bundleKey = if ($bundleKeyParts.Count -gt 0) { $bundleKeyParts[0] } else { 'no-companion-bundle' }
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'companion-tool-telemetry.json'
$bundleMarkdownPath = Join-Path $bundlePath 'companion-tool-telemetry.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    companionToolTelemetryState = $companionToolTelemetryState
    reasonCode = $reasonCode
    nextAction = $nextAction
    optionalHopngState = 'optional-bounded'
    companionTelemetryPosture = 'admitted-optional-bounded'
    holographicDataTool = $holographicDataToolTelemetry
    triviumForum = $triviumForumTelemetry
}
Add-AutomationCascadeOperatorPromptProperty -InputObject $payload | Out-Null

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Companion Tool Telemetry',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Companion telemetry state: `{0}`' -f $payload.companionToolTelemetryState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Companion telemetry posture: `{0}`' -f $payload.companionTelemetryPosture),
    ('- Optional `.hopng` state: `{0}`' -f $payload.optionalHopngState),
    '',
    '## Holographic Data Tool',
    '',
    ('- Tool state: `{0}`' -f $payload.holographicDataTool.toolState),
    ('- Reason code: `{0}`' -f $payload.holographicDataTool.reasonCode),
    ('- Next action: `{0}`' -f $payload.holographicDataTool.nextAction),
    ('- Cycle status: `{0}`' -f $(if ($payload.holographicDataTool.cycleStatus) { $payload.holographicDataTool.cycleStatus } else { 'missing' })),
    ('- Development posture: `{0}`' -f $(if ($payload.holographicDataTool.developmentPosture) { $payload.holographicDataTool.developmentPosture } else { 'missing' })),
    ('- Last bundle id: `{0}`' -f $(if ($payload.holographicDataTool.lastBundleId) { $payload.holographicDataTool.lastBundleId } else { 'missing' })),
    ('- Last digest bundle id: `{0}`' -f $(if ($payload.holographicDataTool.lastDigestBundleId) { $payload.holographicDataTool.lastDigestBundleId } else { 'missing' })),
    ('- Last bundle path: `{0}`' -f $(if ($payload.holographicDataTool.lastBundlePath) { $payload.holographicDataTool.lastBundlePath } else { 'missing' })),
    ('- Tasking status present: `{0}`' -f [bool] $payload.holographicDataTool.taskingStatusPresent),
    ('- Master-thread state present: `{0}`' -f [bool] $payload.holographicDataTool.masterThreadStatePresent),
    '',
    '## Trivium Forum',
    '',
    ('- Tool state: `{0}`' -f $payload.triviumForum.toolState),
    ('- Reason code: `{0}`' -f $payload.triviumForum.reasonCode),
    ('- Next action: `{0}`' -f $payload.triviumForum.nextAction),
    ('- Repo present: `{0}`' -f [bool] $payload.triviumForum.repoPresent),
    ('- Audit root present: `{0}`' -f [bool] $payload.triviumForum.auditRootPresent),
    ('- Conference surface present: `{0}`' -f [bool] $payload.triviumForum.conferenceSurfacePresent),
    ('- Docs present: `{0}`' -f [bool] $payload.triviumForum.docsPresent),
    ('- Package present: `{0}`' -f [bool] $payload.triviumForum.packagePresent)
)
$markdownLines = Add-AutomationCascadePromptMarkdownLines -MarkdownLines $markdownLines
Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    companionToolTelemetryState = $payload.companionToolTelemetryState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    companionTelemetryPosture = $payload.companionTelemetryPosture
    optionalHopngState = $payload.optionalHopngState
    holographicDataToolTelemetryState = $payload.holographicDataTool.toolState
    triviumForumTelemetryState = $payload.triviumForum.toolState
    holographicDataToolCycleStatus = $payload.holographicDataTool.cycleStatus
    triviumForumConferenceSurfacePresent = $payload.triviumForum.conferenceSurfacePresent
}
Add-AutomationCascadeOperatorPromptProperty -InputObject $statePayload | Out-Null

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[companion-tool-telemetry] Bundle: {0}' -f $bundlePath)
$bundlePath

