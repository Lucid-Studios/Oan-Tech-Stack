param(
    [string] $RepoRoot,
    [Parameter(Mandatory = $true)]
    [string] $ManifestPath,
    [string] $DigestBundlePath,
    [string] $CyclePolicyPath = 'OAN Mortalis V1.1.1/Automation/local-automation-cycle.json',
    [string] $TaskStatusPath = '.audit/state/local-automation-tasking-status.json'
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

    $Value | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $Path -Encoding utf8
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedManifestPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $ManifestPath
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$resolvedTaskStatusPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $TaskStatusPath
$cyclePolicy = Get-Content -Raw -LiteralPath $resolvedCyclePolicyPath | ConvertFrom-Json
$blockedEscalationOutputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.blockedEscalationOutputRoot)
$blockedEscalationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.blockedEscalationStatePath)

$manifest = Get-Content -Raw -LiteralPath $resolvedManifestPath | ConvertFrom-Json
if ([string] $manifest.status -ne [string] $cyclePolicy.blockedStatus) {
    throw 'Blocked escalation bundles may only be created for blocked manifests.'
}

$digestJson = $null
$resolvedDigestBundlePath = $null
if (-not [string]::IsNullOrWhiteSpace($DigestBundlePath)) {
    $resolvedDigestBundlePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $DigestBundlePath
    $digestJson = Read-JsonFileOrNull -Path (Join-Path $resolvedDigestBundlePath 'release-candidate-digest.json')
}

$taskStatus = Read-JsonFileOrNull -Path $resolvedTaskStatusPath
$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$commitSha = [string] $manifest.repo.commitSha
$shortSha = if ($commitSha.Length -gt 8) { $commitSha.Substring(0, 8) } else { $commitSha }
$bundleId = '{0}-{1}' -f $timestamp, $shortSha
$bundlePath = Join-Path $blockedEscalationOutputRoot $bundleId
$bundleJsonPath = Join-Path $bundlePath 'blocked-escalation-bundle.json'
$bundleMarkdownPath = Join-Path $bundlePath 'blocked-escalation-bundle.md'

$bundlePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    status = [string] $manifest.status
    repo = $manifest.repo
    versionDecision = $manifest.versionDecision
    gatesTriggered = @($manifest.gatesTriggered | ForEach-Object { [string] $_ })
    buildAudit = $manifest.buildAudit
    subsystemAudit = $manifest.subsystemAudit
    sourceManifestPath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $resolvedManifestPath
    sourceDigestBundlePath = if ($null -ne $resolvedDigestBundlePath) { Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $resolvedDigestBundlePath } else { $null }
    taskStatusPath = if (Test-Path -LiteralPath $resolvedTaskStatusPath -PathType Leaf) { Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $resolvedTaskStatusPath } else { $null }
    recommendedAction = if ($null -ne $digestJson) { [string] $digestJson.recommendedAction } else { 'review-required-blocked' }
}
Add-AutomationCascadeOperatorPromptProperty -InputObject $bundlePayload | Out-Null

Write-JsonFile -Path $bundleJsonPath -Value $bundlePayload

$markdownLines = @(
    '# Blocked Escalation Bundle',
    '',
    ('- Generated at (UTC): `{0}`' -f $bundlePayload.generatedAtUtc),
    ('- Status: `{0}`' -f $bundlePayload.status),
    ('- Branch: `{0}`' -f [string] $bundlePayload.repo.branch),
    ('- Commit: `{0}`' -f [string] $bundlePayload.repo.commitSha),
    ('- Recommended action: `{0}`' -f [string] $bundlePayload.recommendedAction),
    ('- Source manifest: `{0}`' -f [string] $bundlePayload.sourceManifestPath)
)

if ($bundlePayload.sourceDigestBundlePath) {
    $markdownLines += ('- Source digest bundle: `{0}`' -f [string] $bundlePayload.sourceDigestBundlePath)
}

if ($bundlePayload.gatesTriggered.Count -gt 0) {
    $markdownLines += @(
        '',
        '## Gates',
        ''
    )

    foreach ($gate in $bundlePayload.gatesTriggered) {
        $markdownLines += ('- `{0}`' -f $gate)
    }
}

$markdownLines = Add-AutomationCascadePromptMarkdownLines -MarkdownLines $markdownLines
Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $bundlePayload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    bundleJsonPath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundleJsonPath
    sourceManifestPath = $bundlePayload.sourceManifestPath
    recommendedAction = $bundlePayload.recommendedAction
}
Add-AutomationCascadeOperatorPromptProperty -InputObject $statePayload | Out-Null

Write-JsonFile -Path $blockedEscalationStatePath -Value $statePayload
Write-Host ('[blocked-escalation-bundle] Bundle: {0}' -f $bundlePath)
$bundlePath
