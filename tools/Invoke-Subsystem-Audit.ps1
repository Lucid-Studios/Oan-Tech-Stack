param(
    [ValidateSet('CradleTek', 'SoulFrame', 'AgentiCore', 'GovernanceConnective', 'SLI', 'All')]
    [string[]] $Subsystem = @('All'),

    [ValidateSet("Debug", "Release")]
    [string] $Configuration = "Release",

    [string] $AuditBundlePath,

    [string] $OutputRoot = ".audit/runs"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Write-JsonFile {
    param([string] $Path, [object] $Value)

    $directory = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($directory)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }

    $Value | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $Path -Encoding utf8
}

function Add-NdjsonLine {
    param([string] $Path, [object] $Value)

    $Value | ConvertTo-Json -Depth 12 -Compress | Add-Content -LiteralPath $Path -Encoding utf8
}

function Invoke-AuditCommand {
    param(
        [string] $Executable,
        [string[]] $Arguments,
        [string] $LogPath
    )

    $startedAt = Get-Date
    $captured = @()
    $exitCode = 0

    try {
        $captured = & $Executable @Arguments 2>&1
        $exitCode = $LASTEXITCODE
        if ($null -eq $exitCode) {
            $exitCode = 0
        }
    }
    catch {
        $captured = @($_.Exception.ToString())
        $exitCode = 1
    }

    $endedAt = Get-Date
    Set-Content -LiteralPath $LogPath -Value ($captured | ForEach-Object { "$_" }) -Encoding utf8

    return [ordered]@{
        startedAtUtc = $startedAt.ToUniversalTime().ToString("o")
        completedAtUtc = $endedAt.ToUniversalTime().ToString("o")
        durationMs = [int][Math]::Round(($endedAt - $startedAt).TotalMilliseconds)
        exitCode = $exitCode
        succeeded = ($exitCode -eq 0)
        logPath = $LogPath
    }
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

function Get-LatestAuditBundlePath {
    param([string] $BundleRoot)

    if (-not (Test-Path -LiteralPath $BundleRoot -PathType Container)) {
        return $null
    }

    $latest = Get-ChildItem -LiteralPath $BundleRoot -Directory | Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1
    if ($null -eq $latest) {
        return $null
    }

    return $latest.FullName
}

$repoRoot = (Resolve-Path -LiteralPath (Split-Path -Parent $MyInvocation.MyCommand.Path)).Path | Split-Path -Parent
$bundleRoot = Join-Path $repoRoot $OutputRoot

if ([string]::IsNullOrWhiteSpace($AuditBundlePath)) {
    $AuditBundlePath = Get-LatestAuditBundlePath -BundleRoot $bundleRoot
}
elseif (-not [System.IO.Path]::IsPathRooted($AuditBundlePath)) {
    $AuditBundlePath = Join-Path $repoRoot $AuditBundlePath
}

if (-not (Test-Path -LiteralPath $AuditBundlePath -PathType Container)) {
    throw "Audit bundle path not found. Run tools/Invoke-Build-Audit.ps1 first or supply -AuditBundlePath."
}

$projectsPath = Join-Path $AuditBundlePath 'projects.json'
$testsPath = Join-Path $AuditBundlePath 'tests.json'
if (-not (Test-Path -LiteralPath $projectsPath -PathType Leaf)) {
    throw "Audit bundle is missing projects.json."
}

$projectEntries = @((Get-Content -Raw -LiteralPath $projectsPath | ConvertFrom-Json) | ForEach-Object { $_ })
$testEntries = @()
if (Test-Path -LiteralPath $testsPath -PathType Leaf) {
    $testEntries = @((Get-Content -Raw -LiteralPath $testsPath | ConvertFrom-Json) | ForEach-Object { $_ })
}

$requestedSubsystems = if ($Subsystem -contains 'All') {
    @('CradleTek', 'SoulFrame', 'AgentiCore', 'GovernanceConnective', 'SLI')
}
else {
    $Subsystem
}

$subsystemDefinitions = @{
    CradleTek = @{
        Prefixes = @('OAN Mortalis V1.0/src/CradleTek.', 'OAN Mortalis V1.0/src/Oan.Cradle/', 'OAN Mortalis V1.0/src/Oan.Runtime.Headless/')
        Tests = @('Oan.Runtime.IntegrationTests')
        Ownership = @('Application and swarm orchestration fabric', 'Loop composition root', 'Control-plane routing and local recovery entrypoints')
        Inputs = @('Governance loop contracts', 'Journal-first status and recovery evidence', 'Store registry service resolution')
        Outputs = @('Runtime loop execution', 'Status views', 'Resume and retry coordination')
        Placement = 'CradleTek remains the application fabric and should not absorb custody, membrane, or publication law.'
    }
    SoulFrame = @{
        Prefixes = @('OAN Mortalis V1.0/src/SoulFrame.', 'OAN Mortalis V1.0/src/Oan.SoulFrame/')
        Tests = @('Oan.SoulFrame.Tests')
        Ownership = @('Self-state membrane', 'Bounded projection and candidate intake', 'Identity-safe mediation')
        Inputs = @('Cryptic-side governed source state', 'Bounded cognition return candidates')
        Outputs = @('Mitigated self-state projections', 'Return intake candidates', 'Membrane-scoped telemetry')
        Placement = 'SoulFrame remains a membrane, not a general execution or storage blob.'
    }
    AgentiCore = @{
        Prefixes = @('OAN Mortalis V1.0/src/AgentiCore', 'OAN Mortalis V1.0/src/Oan.AgentiCore/')
        Tests = @('Oan.Audit.Tests')
        Ownership = @('Bounded worker cognition', 'Local working-state use', 'Candidate-shaped return emission')
        Inputs = @('SoulFrame-bounded handles', 'Local bounded cognition requests')
        Outputs = @('Candidate return material', 'Working-state observations')
        Placement = 'AgentiCore should grow by composition around the bounded worker rather than by widening its authority.'
    }
    GovernanceConnective = @{
        Prefixes = @('OAN Mortalis V1.0/src/EngramGovernance/', 'OAN Mortalis V1.0/src/Oan.Storage/', 'OAN Mortalis V1.0/src/CradleTek.Mantle/', 'OAN Mortalis V1.0/src/CradleTek.Public/', 'OAN Mortalis V1.0/src/Data.Cryptic/')
        Tests = @('Oan.Runtime.IntegrationTests', 'Oan.Audit.Tests')
        Ownership = @('Governance adjudication', 'Receipt journaling', 'Cryptic re-engrammitization gate', 'Prime derivative publication lanes')
        Inputs = @('Return candidate review requests', 'Approved governance decisions', 'Journal replay evidence')
        Outputs = @('Decision receipts', 'Downstream act receipts', 'Governed Prime publication outputs')
        Placement = 'This connective layer should remain explicit and payload-proven rather than dissolve back into GEL or orchestration convenience.'
    }
    SLI = @{
        Prefixes = @('OAN Mortalis V1.0/src/Oan.Sli/', 'OAN Mortalis V1.0/src/SLI.')
        Tests = @('Oan.Sli.Tests')
        Ownership = @('Symbolic routing and ingestion', 'Deterministic symbolic execution', 'Lisp and parser surfaces')
        Inputs = @('Core runtime requests', 'Symbolic payloads', 'Deterministic harness inputs')
        Outputs = @('Symbolic execution results', 'Routing decisions', 'Ingestion artifacts')
        Placement = 'SLI should attach to proven runtime paths and not compensate for unclear lower-layer truth.'
    }
}

$results = New-Object System.Collections.Generic.List[object]
foreach ($subsystemName in $requestedSubsystems) {
    $definition = $subsystemDefinitions[$subsystemName]
    $subsystemDir = Join-Path $AuditBundlePath ('subsystems/{0}' -f $subsystemName)
    $logsDir = Join-Path $subsystemDir 'logs'
    New-Item -ItemType Directory -Force -Path $logsDir | Out-Null

    $eventsPath = Join-Path $subsystemDir 'events.ndjson'
    $summaryPath = Join-Path $subsystemDir 'summary.md'
    $payloadsPath = Join-Path $subsystemDir 'payloads.json'

    Add-NdjsonLine -Path $eventsPath -Value ([ordered]@{
        eventType = 'SUBSYSTEM_AUDIT_STARTED'
        subsystem = $subsystemName
        timestampUtc = (Get-Date).ToUniversalTime().ToString('o')
        auditBundle = Get-RelativePathString -BasePath $repoRoot -TargetPath $AuditBundlePath
    })

    $ownedProjects = @(
        $projectEntries | Where-Object {
            $path = [string] $_.projectPath
            $matches = $false
            foreach ($prefix in $definition.Prefixes) {
                if ($path.StartsWith($prefix, [System.StringComparison]::Ordinal)) {
                    $matches = $true
                    break
                }
            }
            $matches
        } | ForEach-Object {
            [pscustomobject]@{
                projectName = [string] $_.projectName
                projectPath = [string] $_.projectPath
                kind = [string] $_.kind
                result = [string] $_.result
                outputClassification = [string] $_.outputClassification
            }
        }
    )

    $associatedTests = @(
        $testEntries | Where-Object { $definition.Tests -contains [string] $_.projectName } | ForEach-Object {
            [pscustomobject]@{
                projectName = [string] $_.projectName
                projectPath = [string] $_.projectPath
            }
        }
    )

    $runtimePayloadClassification = if ($ownedProjects.Count -gt 0) { 'summary_only' } else { 'unimplemented' }
    $runtimePayloadNote = if ($ownedProjects.Count -gt 0) {
        'Subsystem audit v1 is using build and test evidence plus targeted probes; runtime payload content remains a bounded summary until a deeper pass measures subsystem-specific outputs directly.'
    }
    else {
        'No owned projects were identified for this subsystem in the current audit bundle.'
    }

    $probeResults = New-Object System.Collections.Generic.List[object]
    foreach ($testName in $definition.Tests) {
        $testProbe = $associatedTests | Where-Object projectName -eq $testName | Select-Object -First 1
        if ($null -eq $testProbe) {
            continue
        }

        $testProjectPath = Join-Path $repoRoot $testProbe.projectPath.Replace('/', '\')
        $logPath = Join-Path $logsDir ("probe-{0}.log" -f $testName)
        $probe = Invoke-AuditCommand -Executable 'dotnet' -Arguments @('test', $testProjectPath, '-c', $Configuration, '--no-build', '-v', 'minimal') -LogPath $logPath
        $probeResults.Add([ordered]@{
            projectName = $testName
            result = if ($probe.succeeded) { 'succeeded' } else { 'failed' }
            durationMs = $probe.durationMs
            exitCode = $probe.exitCode
            logPath = Get-RelativePathString -BasePath $subsystemDir -TargetPath $logPath
        }) | Out-Null

        if (-not $probe.succeeded) {
            throw "Subsystem audit probe failed for '$subsystemName' via '$testName'."
        }
    }

    $payloads = @(
        [ordered]@{
            subsystem = $subsystemName
            classification = $runtimePayloadClassification
            note = $runtimePayloadNote
            references = @('summary.md')
        }
    )

    Write-JsonFile -Path $payloadsPath -Value $payloads

    $summaryLines = @(
        '# Subsystem Audit Summary',
        '',
        ('- Subsystem: `{0}`' -f $subsystemName),
        ('- Bundle: `{0}`' -f (Get-RelativePathString -BasePath $repoRoot -TargetPath $AuditBundlePath)),
        '',
        '## Ownership',
        ''
    )
    foreach ($item in $definition.Ownership) { $summaryLines += ('- {0}' -f $item) }
    $summaryLines += @('', '## Inputs', '')
    foreach ($item in $definition.Inputs) { $summaryLines += ('- {0}' -f $item) }
    $summaryLines += @('', '## Outputs', '')
    foreach ($item in $definition.Outputs) { $summaryLines += ('- {0}' -f $item) }
    $summaryLines += @(
        '',
        '## Payload Truth',
        '',
        ('- Classification: `{0}`' -f $runtimePayloadClassification),
        ('- Note: {0}' -f $runtimePayloadNote),
        '',
        '## Placement',
        '',
        ('- {0}' -f $definition.Placement),
        '',
        '## Owned Projects',
        '',
        '| Project | Kind | Build Result | Output Classification |',
        '| --- | --- | --- | --- |'
    )
    foreach ($project in $ownedProjects) {
        $summaryLines += ('| {0} | {1} | {2} | {3} |' -f $project.projectName, $project.kind, $project.result, $project.outputClassification)
    }

    if ($probeResults.Count -gt 0) {
        $summaryLines += @(
            '',
            '## Test Probes',
            '',
            '| Project | Result | Duration (ms) |',
            '| --- | --- | ---: |'
        )
        foreach ($probe in $probeResults) {
            $summaryLines += ('| {0} | {1} | {2} |' -f $probe.projectName, $probe.result, $probe.durationMs)
        }
    }

    Set-Content -LiteralPath $summaryPath -Value $summaryLines -Encoding utf8

    Add-NdjsonLine -Path $eventsPath -Value ([ordered]@{
        eventType = 'SUBSYSTEM_AUDIT_COMPLETED'
        subsystem = $subsystemName
        timestampUtc = (Get-Date).ToUniversalTime().ToString('o')
        ownedProjectCount = $ownedProjects.Count
        testProbeCount = $probeResults.Count
        payloadClassification = $runtimePayloadClassification
    })

    $results.Add([ordered]@{
        subsystem = $subsystemName
        ownedProjectCount = $ownedProjects.Count
        testProbeCount = $probeResults.Count
        summaryPath = Get-RelativePathString -BasePath $repoRoot -TargetPath $summaryPath
        payloadClassification = $runtimePayloadClassification
    }) | Out-Null
}

$resultsPath = Join-Path $AuditBundlePath 'subsystems/results.json'
Write-JsonFile -Path $resultsPath -Value $results
Write-Host ("[audit] Subsystem summaries written under {0}" -f (Join-Path $AuditBundlePath 'subsystems'))
$resultsPath
