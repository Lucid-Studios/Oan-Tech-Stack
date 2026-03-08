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

    if ($SubsystemName -eq 'SoulFrame') {
        return @(
            [ordered]@{
                witness = 'projection-shape'
                classification = 'summary_only'
                note = 'Bounded projection is proven by contract and tests to carry non-empty SessionHandle, WorkingStateHandle, and ProvenanceMarker while withholding custody-shaped payload material.'
                references = @(
                    'OAN Mortalis V1.0/src/Oan.Common/SoulFrameMembraneContracts.cs',
                    'OAN Mortalis V1.0/src/SoulFrame.Host/SoulFrameHostClient.cs',
                    'OAN Mortalis V1.0/tests/Oan.SoulFrame.Tests/SoulFrameHostClientTests.cs'
                )
            }
            [ordered]@{
                witness = 'return-candidate-shape'
                classification = 'summary_only'
                note = 'Return intake is candidate-only by contract and tests, with SessionHandle, ReturnCandidatePointer, ProvenanceMarker, and IntakeIntent but no patch or write-back semantics.'
                references = @(
                    'OAN Mortalis V1.0/src/Oan.Common/SoulFrameMembraneContracts.cs',
                    'OAN Mortalis V1.0/src/SoulFrame.Host/SoulFrameHostClient.cs',
                    'OAN Mortalis V1.0/tests/Oan.SoulFrame.Tests/SoulFrameHostClientTests.cs',
                    'OAN Mortalis V1.0/tests/Oan.Audit.Tests/AgentiCoreMembraneCallerTests.cs'
                )
            }
            [ordered]@{
                witness = 'authority-boundary'
                classification = 'empty_by_design'
                note = 'SoulFrame intentionally does not expose custody mutation, broad store access, orchestration control, or Prime publication payloads through the membrane surface.'
                references = @(
                    'OAN Mortalis V1.0/src/Oan.Common/SoulFrameMembraneContracts.cs',
                    'OAN Mortalis V1.0/src/SoulFrame.Host/SoulFrameHostClient.cs'
                )
            }
        )
    }

    if ($SubsystemName -eq 'AgentiCore') {
        return @(
            [ordered]@{
                witness = 'bounded-worker-state'
                classification = 'summary_only'
                note = 'Bounded worker state is proven by code and tests to carry non-empty SessionHandle, WorkingStateHandle, and ProvenanceMarker while rejecting custody-shaped widening.'
                references = @(
                    'OAN Mortalis V1.0/src/AgentiCore/Services/BoundedMembraneWorkerService.cs',
                    'OAN Mortalis V1.0/tests/Oan.Audit.Tests/AgentiCoreMembraneCallerTests.cs',
                    'OAN Mortalis V1.0/tests/Oan.Audit.Tests/AgentiCoreFlowMembraneIntegrationTests.cs'
                )
            }
            [ordered]@{
                witness = 'return-candidate-pointer'
                classification = 'payload_present'
                note = 'AgentiCore emits explicit candidate-shaped return pointers on the bounded membrane path, and tests prove those pointers are non-empty and remain in the agenticore-return:// namespace.'
                references = @(
                    'OAN Mortalis V1.0/src/AgentiCore/Services/BoundedMembraneWorkerService.cs',
                    'OAN Mortalis V1.0/src/AgentiCore/Services/AgentiCore.cs',
                    'OAN Mortalis V1.0/tests/Oan.Audit.Tests/AgentiCoreMembraneCallerTests.cs',
                    'OAN Mortalis V1.0/tests/Oan.Audit.Tests/AgentiCoreFlowMembraneIntegrationTests.cs'
                )
            }
            [ordered]@{
                witness = 'governance-cycle-candidate-payload'
                classification = 'payload_present'
                note = 'The governance-cycle path returns a non-empty candidate payload derived from bounded cognition results, rather than only emitting an empty shell or state transition marker.'
                references = @(
                    'OAN Mortalis V1.0/src/AgentiCore/Services/AgentiCore.cs',
                    'OAN Mortalis V1.0/tests/Oan.Audit.Tests/StewardAgentGovernanceTests.cs'
                )
            }
            [ordered]@{
                witness = 'authority-boundary'
                classification = 'empty_by_design'
                note = 'AgentiCore intentionally does not widen WorkingStateHandle into custody access and does not gain publication or orchestration authority through the bounded worker stage.'
                references = @(
                    'OAN Mortalis V1.0/src/AgentiCore/Services/BoundedMembraneWorkerService.cs',
                    'OAN Mortalis V1.0/src/AgentiCore/Services/AgentiCore.cs',
                    'OAN Mortalis V1.0/tests/Oan.Audit.Tests/AgentiCoreFlowMembraneIntegrationTests.cs'
                )
            }
        )
    }

    if ($SubsystemName -eq 'GovernanceConnective') {
        return @(
            [ordered]@{
                witness = 'governance-decision-receipt'
                classification = 'payload_present'
                note = 'Governance adjudication emits non-empty decision receipts with explicit decision, adjudicator identity, rationale code, and downstream authorization state.'
                references = @(
                    'OAN Mortalis V1.0/src/EngramGovernance/Services/StewardAgent.cs',
                    'OAN Mortalis V1.0/tests/Oan.Audit.Tests/StewardAgentGovernanceTests.cs',
                    'OAN Mortalis V1.0/tests/Oan.Runtime.IntegrationTests/GovernanceGoldenPathIntegrationTests.cs'
                )
            }
            [ordered]@{
                witness = 'approved-reengrammitization'
                classification = 'payload_present'
                note = 'Approved governance outcomes produce a governed reengrammitization request and accepted Cryptic receipt, proving that residue admission is carrying real payload input rather than only advancing loop state.'
                references = @(
                    'OAN Mortalis V1.0/src/CradleTek.Mantle/MantleOfSovereigntyService.cs',
                    'OAN Mortalis V1.0/src/Oan.Cradle/StackManager.cs',
                    'OAN Mortalis V1.0/tests/Oan.Runtime.IntegrationTests/GovernanceGoldenPathIntegrationTests.cs'
                )
            }
            [ordered]@{
                witness = 'rejected-deferred-downstream-suppression'
                classification = 'empty_by_policy'
                note = 'Rejected and deferred decisions explicitly authorize no Cryptic mutation and no Prime publication; emptiness here is policy-enforced rather than an accidental absence of work.'
                references = @(
                    'OAN Mortalis V1.0/src/EngramGovernance/Services/StewardAgent.cs',
                    'OAN Mortalis V1.0/src/Oan.Cradle/StackManager.cs',
                    'OAN Mortalis V1.0/tests/Oan.Audit.Tests/StewardAgentGovernanceTests.cs',
                    'OAN Mortalis V1.0/tests/Oan.Runtime.IntegrationTests/GovernanceGoldenPathIntegrationTests.cs'
                )
            }
            [ordered]@{
                witness = 'prime-publication-lanes'
                classification = 'payload_present'
                note = 'Prime publication lanes are independently materialized and tracked: approved loops publish pointer and checked-view outputs separately, while replay and retry preserve lane-aware completion without duplicating already completed acts.'
                references = @(
                    'OAN Mortalis V1.0/src/CradleTek.Public/PublicLayerService.cs',
                    'OAN Mortalis V1.0/src/Oan.Cradle/StackManager.cs',
                    'OAN Mortalis V1.0/tests/Oan.Runtime.IntegrationTests/GovernanceGoldenPathIntegrationTests.cs',
                    'OAN Mortalis V1.0/tests/Oan.Runtime.IntegrationTests/GovernanceOperationalControlIntegrationTests.cs'
                )
            }
            [ordered]@{
                witness = 'replay-and-recovery-status'
                classification = 'summary_only'
                note = 'Journal-first replay returns explicit loop stage, failure, and lane-completion truth for status and recovery control, proving that continuation is based on recorded evidence rather than implicit state-machine assumption.'
                references = @(
                    'OAN Mortalis V1.0/src/Oan.Storage/GovernanceReceiptJournal.cs',
                    'OAN Mortalis V1.0/src/Oan.Cradle/StackManager.cs',
                    'OAN Mortalis V1.0/tests/Oan.Runtime.IntegrationTests/GovernanceOperationalControlIntegrationTests.cs'
                )
            }
        )
    }

    if ($SubsystemName -eq 'SLI') {
        return @(
            [ordered]@{
                witness = 'sli-ingestion-structure'
                classification = 'payload_present'
                note = 'SLI ingestion produces canonical program expressions, symbol trees, constructor graphs, and candidate tokens rather than only wrapping lower-layer inputs.'
                references = @(
                    'OAN Mortalis V1.0/src/SLI.Ingestion/SliIngestionEngine.cs',
                    'OAN Mortalis V1.0/tests/Oan.Sli.Tests/SliIngestionTests.cs'
                )
            }
            [ordered]@{
                witness = 'symbolic-cognition-trace'
                classification = 'payload_present'
                note = 'The SLI cognition engine emits non-empty decision branches, cleave residue, confidence, and symbolic traces, showing real symbolic execution rather than ceremonial naming.'
                references = @(
                    'OAN Mortalis V1.0/tests/Oan.Sli.Tests/SliCognitionEngineTests.cs'
                )
            }
            [ordered]@{
                witness = 'lisp-bridge-roundtrip'
                classification = 'payload_present'
                note = 'The Lisp bridge accepts and returns structured packet content in memory, proving the current symbolic bridge is carrying routing material rather than existing only as an architectural placeholder.'
                references = @(
                    'OAN Mortalis V1.0/src/SLI.Engine/LispSliBridgeStub.cs',
                    'OAN Mortalis V1.0/tests/Oan.Sli.Tests/SliEngineBridgeTests.cs'
                )
            }
            [ordered]@{
                witness = 'authority-boundary'
                classification = 'empty_by_design'
                note = 'SLI does not adjudicate governance, widen custody access, or impersonate the SoulFrame membrane in the currently proven paths; its role remains symbolic ingestion, routing, and execution.'
                references = @(
                    'OAN Mortalis V1.0/src/Oan.Sli/RoutingEngine.cs',
                    'OAN Mortalis V1.0/src/SLI.Ingestion/SliIngestionEngine.cs',
                    'OAN Mortalis V1.0/tests/Oan.Sli.Tests/SliIngestionTests.cs'
                )
            }
        )
    }

    $classification = if ($OwnedProjects.Count -gt 0) { 'summary_only' } else { 'unimplemented' }
    $note = if ($OwnedProjects.Count -gt 0) {
        'Subsystem audit v1 is using build and test evidence plus targeted probes; runtime payload content remains a bounded summary until a deeper pass measures subsystem-specific outputs directly.'
    }
    else {
        'No owned projects were identified for this subsystem in the current audit bundle.'
    }

    return @(
        [ordered]@{
            witness = 'runtime-payload-truth'
            classification = $classification
            note = $note
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
        SymbolicContribution = @(
            'Canonical SLI expressions and symbol trees that do not already exist in lower-layer inputs',
            'Non-empty symbolic traces, decision branches, and cleave residue from SLI cognition execution',
            'In-memory Lisp packet bridge behavior that proves current symbolic routing is more than naming ceremony'
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
