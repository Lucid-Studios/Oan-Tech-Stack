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

function Get-ChildScriptOutputTail {
    param([object[]] $Output)

    return @($Output | ForEach-Object { "$_".Trim() } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -Last 1)
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$statusScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-SourceBucket-ReturnIntegrationStatus.ps1'
$statusOutput = & $statusScriptPath -RepoRoot $resolvedRepoRoot -FederationPolicyPath $FederationPolicyPath
$statusPath = Get-ChildScriptOutputTail -Output $statusOutput
if (-not [string]::IsNullOrWhiteSpace($statusPath)) {
    Write-Host ('[source-bucket-return-cycle] Status: {0}' -f $statusPath)
}

$statusPath
