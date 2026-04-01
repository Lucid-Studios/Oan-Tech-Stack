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

function Get-SubsystemPayloadWitnesses {
    param(
        [string] $SubsystemName,
        [object[]] $OwnedProjects
    )

    $classification = if ($OwnedProjects.Count -gt 0) { 'summary_only' } else { 'unimplemented' }
    $ownedProjectReferences = @($OwnedProjects | ForEach-Object { [string] $_.projectPath } | Select-Object -First 8)
    if ($ownedProjectReferences.Count -eq 0) {
        $ownedProjectReferences = @('summary.md')
    }

    $runtimeNotes = @{
        CradleTek = 'CradleTek truth is currently summarized from the active V1.1.1 build lane and subsystem probes. This lane covers custody, host, runtime, mantle, and memory ownership without treating the summary as direct runtime authority.'
        SoulFrame = 'SoulFrame truth is currently summarized from the active V1.1.1 build lane and subsystem probes. This lane covers bootstrap and membrane ownership without widening the subsystem into custody, publication, or orchestration authority.'
        AgentiCore = 'AgentiCore truth is currently summarized from the active V1.1.1 build lane and subsystem probes. This lane covers chambered cognition and governed utility without flattening those surfaces into general repo authority.'
        GovernanceConnective = 'Governance connective truth is currently summarized from the active V1.1.1 build lane and subsystem probes. This lane covers Sanctuary governance, first-run shaping, hosted-seed mediation, trace persistence, and GEL-facing connective law as build-facing evidence rather than free narrative.'
        SLI = 'SLI truth is currently summarized from the active V1.1.1 build lane and subsystem probes. This lane covers ingestion, hosted Lisp, symbolic execution, and the HITL-to-SLI bridge without treating symbolic surfaces as self-authorizing governance.'
    }

    $ownedProjectNote = if ($OwnedProjects.Count -gt 0) {
        'Owned projects were identified for this subsystem in the active V1.1.1 solution and are being summarized directly from the current build tree.'
    } else {
        'No owned projects were identified for this subsystem in the active build audit bundle.'
    }

    $runtimeNote = if ($runtimeNotes.ContainsKey($SubsystemName)) {
        [string] $runtimeNotes[$SubsystemName]
    } else {
        'Subsystem audit v1 is using build and test evidence plus targeted probes; runtime payload content remains a bounded summary until a deeper subsystem-specific evidence pass is introduced.'
    }

    return @(
        [ordered]@{
            witness = 'owned-project-surface'
            classification = $classification
            note = $ownedProjectNote
            references = $ownedProjectReferences
        }
        [ordered]@{
            witness = 'runtime-payload-truth'
            classification = $classification
            note = $runtimeNote
            references = @('summary.md')
        }
    )
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
        Prefixes = @('OAN Mortalis V1.1.1/src/TechStack/CradleTek/', 'OAN Mortalis V1.1.1/src/Sanctuary/Oan.Runtime.Headless/', 'OAN Mortalis V1.1.1/src/Sanctuary/Oan.Runtime.Materialization/')
        Tests = @('Oan.Runtime.IntegrationTests')
        Ownership = @('Runtime host and orchestration fabric', 'Custody, mantle, and memory seams', 'Materialized return and audit surfaces')
        Inputs = @('Sanctuary governance decisions', 'Bounded ingress and hosted-seed outputs', 'Active solution build evidence')
        Outputs = @('Headless runtime execution', 'Operational contexts and return shaping', 'Host-visible evidence surfaces')
        Placement = 'CradleTek remains the governed host substrate and should not absorb doctrine, publication, or free-floating documentation authority.'
    }
    SoulFrame = @{
        Prefixes = @('OAN Mortalis V1.1.1/src/TechStack/SoulFrame/')
        Tests = @('Oan.Audit.Tests')
        Ownership = @('Bootstrap and membrane identity surfaces', 'Bounded projection and return-intake mediation', 'Stewardship-facing continuity seams')
        Inputs = @('Sanctuary legality and bootstrap context', 'CradleTek and AgentiCore receipts')
        Outputs = @('Mediated membrane posture', 'Bootstrap-bearing continuity surfaces', 'Bounded outward return posture')
        Placement = 'SoulFrame remains a stewardship membrane and should not be widened into a general host, storage, or publication sink.'
    }
    AgentiCore = @{
        Prefixes = @('OAN Mortalis V1.1.1/src/TechStack/AgentiCore/')
        Tests = @('Oan.Audit.Tests')
        Ownership = @('Chambered cognition execution', 'Governed utility and working-state use', 'Candidate-bearing return posture')
        Inputs = @('SoulFrame memory and ingress context', 'Hosted-seed and symbolic floor decisions')
        Outputs = @('Cognition-stage receipts', 'Candidate-bearing runtime posture', 'Governed utility surfaces')
        Placement = 'AgentiCore should grow by bounded cognition and utility seams rather than by widening into custody, documentation, or publication authority.'
    }
    GovernanceConnective = @{
        Prefixes = @('OAN Mortalis V1.1.1/src/Sanctuary/Oan.Common/', 'OAN Mortalis V1.1.1/src/Sanctuary/Oan.Nexus.Control/', 'OAN Mortalis V1.1.1/src/Sanctuary/Oan.PrimeCryptic.Services/', 'OAN Mortalis V1.1.1/src/Sanctuary/Oan.State.Modulation/', 'OAN Mortalis V1.1.1/src/Sanctuary/Oan.Trace.Persistence/', 'OAN Mortalis V1.1.1/src/Sanctuary/Oan.HostedLlm/', 'OAN Mortalis V1.1.1/src/Sanctuary/Oan.FirstRun/', 'OAN Mortalis V1.1.1/src/TechStack/GEL/')
        Tests = @('Oan.Runtime.IntegrationTests', 'Oan.Audit.Tests')
        Ownership = @('Sanctuary governance and nexus decisions', 'First-run and hosted-seed connective law', 'Trace persistence and GEL-facing connective surfaces')
        Inputs = @('Executable runtime receipts', 'Ingress and readiness evidence', 'Governed first-run and hosted-seed state')
        Outputs = @('Governance-readable decisions', 'Traceable connective receipts', 'Build-facing law and telemetry surfaces')
        Placement = 'This connective layer should remain explicit and build-facing rather than collapsing into documentation prose or host convenience.'
    }
    SLI = @{
        Prefixes = @('OAN Mortalis V1.1.1/src/Sanctuary/SLI.')
        Tests = @()
        Ownership = @('Symbolic routing and ingestion', 'Hosted Lisp and symbolic execution', 'The HITL-to-SLI bridge surface')
        Inputs = @('Governed ingress and hosted-seed packets', 'Symbolic runtime requests', 'Deterministic harness inputs')
        Outputs = @('Symbolic execution receipts', 'Routing decisions', 'Ingress and hosted Lisp evidence')
        Placement = 'SLI should attach to proven runtime paths and bridge law, not compensate for unclear lower-layer truth.'
        SymbolicContribution = @(
            'Canonical symbolic ingress and execution surfaces in the active Sanctuary lane',
            'Hosted Lisp receipts and symbolic-floor evidence in the current build line',
            'HITL-to-SLI bridge law that keeps ingress authority distinct from interior continuity authority'
        )
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

    $payloads = @(@(
        Get-SubsystemPayloadWitnesses -SubsystemName $subsystemName -OwnedProjects $ownedProjects
    ) | ForEach-Object {
        [pscustomobject][ordered]@{
            subsystem = $subsystemName
            witness = $_.witness
            classification = $_.classification
            note = $_.note
            references = @($_.references)
        }
    })

    Write-JsonFile -Path $payloadsPath -Value $payloads
    foreach ($payload in $payloads) {
        Add-NdjsonLine -Path $eventsPath -Value ([ordered]@{
            eventType = 'PAYLOAD_WITNESS_CAPTURED'
            subsystem = $subsystemName
            timestampUtc = (Get-Date).ToUniversalTime().ToString('o')
            witness = $payload.witness
            classification = $payload.classification
        })
    }

    $primaryPayloadClassification = if ($payloads.Count -gt 0) {
        ($payloads | Select-Object -First 1).classification
    }
    else {
        'unimplemented'
    }
    $primaryPayloadNote = if ($payloads.Count -gt 0) {
        ($payloads | Select-Object -First 1).note
    }
    else {
        'No payload witness records were emitted.'
    }

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
        ('- Primary classification: `{0}`' -f $primaryPayloadClassification),
        ('- Note: {0}' -f $primaryPayloadNote),
        '',
        '| Witness | Classification | Note |',
        '| --- | --- | --- |'
    )
    foreach ($payload in $payloads) {
        $summaryLines += ('| {0} | {1} | {2} |' -f $payload.witness, $payload.classification, $payload.note)
    }
    $summaryLines += @(
        '',
        '## Placement',
        '',
        ('- {0}' -f $definition.Placement)
    )
    if ($definition.ContainsKey('SymbolicContribution')) {
        $summaryLines += @('', '## Symbolic Contribution', '')
        foreach ($item in $definition.SymbolicContribution) { $summaryLines += ('- {0}' -f $item) }
    }
    $summaryLines += @(
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
        payloadClassification = $primaryPayloadClassification
    })

    $results.Add([ordered]@{
        subsystem = $subsystemName
        ownedProjectCount = $ownedProjects.Count
        testProbeCount = $probeResults.Count
        summaryPath = Get-RelativePathString -BasePath $repoRoot -TargetPath $summaryPath
        payloadClassification = $primaryPayloadClassification
    }) | Out-Null
}

$resultsPath = Join-Path $AuditBundlePath 'subsystems/results.json'
Write-JsonFile -Path $resultsPath -Value $results
Write-Host ("[audit] Subsystem summaries written under {0}" -f (Join-Path $AuditBundlePath 'subsystems'))
$resultsPath
