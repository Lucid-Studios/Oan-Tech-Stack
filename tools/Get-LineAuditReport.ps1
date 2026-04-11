param(
    [string] $LineRoot = "OAN Mortalis V1.1.1",

    [ValidateSet("Markdown", "Json")]
    [string] $Format = "Markdown"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-FileTextOrNull {
    param([string] $Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return $null
    }

    return Get-Content -LiteralPath $Path -Raw
}

function Get-JsonObjectOrNull {
    param([string] $Path)

    $text = Get-FileTextOrNull -Path $Path
    if ([string]::IsNullOrWhiteSpace($text)) {
        return $null
    }

    try {
        return $text | ConvertFrom-Json
    }
    catch {
        return $null
    }
}

function Test-TextContains {
    param(
        [AllowNull()]
        [string] $Text,
        [string] $Value,
        [System.StringComparison] $Comparison = [System.StringComparison]::Ordinal
    )

    if ([string]::IsNullOrEmpty($Text)) {
        return $false
    }

    return $Text.IndexOf($Value, $Comparison) -ge 0
}

function Get-RelativePathString {
    param(
        [string] $BasePath,
        [string] $TargetPath
    )

    $resolvedBase = [System.IO.Path]::GetFullPath($BasePath)
    $resolvedTarget = [System.IO.Path]::GetFullPath($TargetPath)
    $baseUri = New-Object System.Uri(($resolvedBase.TrimEnd('\') + '\'))
    $targetUri = New-Object System.Uri($resolvedTarget)
    return [System.Uri]::UnescapeDataString($baseUri.MakeRelativeUri($targetUri).ToString()).Replace('/', '\')
}

function Get-MarkdownSectionBody {
    param(
        [string] $Text,
        [string] $Heading
    )

    if ([string]::IsNullOrWhiteSpace($Text)) {
        return $null
    }

    $escapedHeading = [regex]::Escape($Heading)
    $match = [regex]::Match(
        $Text,
        "(?ms)^$escapedHeading\r?\n(?<body>.*?)(?=^##\s+|\z)"
    )

    if (-not $match.Success) {
        return $null
    }

    return $match.Groups["body"].Value.Trim()
}

function Convert-SectionBodyToSingleLine {
    param([string] $Text)

    if ([string]::IsNullOrWhiteSpace($Text)) {
        return $null
    }

    $lines = @(
        $Text -split "(`r`n|`n|`r)" |
            ForEach-Object { $_.Trim() } |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    )

    if ($lines.Count -eq 0) {
        return $null
    }

    return [string]::Join(" ", $lines)
}

function Get-MarkdownBacktickedBulletValues {
    param(
        [string] $Text,
        [string] $Heading
    )

    $sectionBody = Get-MarkdownSectionBody -Text $Text -Heading $Heading
    if ([string]::IsNullOrWhiteSpace($sectionBody)) {
        return @()
    }

    return @(
        [regex]::Matches($sectionBody, '(?m)^\s*-\s+`(?<value>[^`]+)`') |
            ForEach-Object { $_.Groups["value"].Value.Trim() } |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    )
}

function Get-MarkdownArrowSequenceValues {
    param(
        [string] $Text,
        [string] $Heading,
        [string] $AxisName
    )

    $sectionBody = Get-MarkdownSectionBody -Text $Text -Heading $Heading
    if ([string]::IsNullOrWhiteSpace($sectionBody)) {
        return @()
    }

    $axisPattern = [regex]::Escape($AxisName)
    $match = [regex]::Match(
        $sectionBody,
        ('(?ms)^\s*-\s+`{0}`\s*\r?\n\s+`(?<values>[^`]+)`' -f $axisPattern)
    )

    if (-not $match.Success) {
        return @()
    }

    return @(
        $match.Groups["values"].Value -split '\s*->\s*' |
            ForEach-Object { $_.Trim() } |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    )
}

function Get-SolutionRelativePath {
    param(
        [string] $LineRootPath,
        [object] $LineManifest
    )

    if ($null -ne $LineManifest) {
        $solutionPathProperty = $LineManifest.PSObject.Properties["solutionPath"]
        if ($null -ne $solutionPathProperty -and -not [string]::IsNullOrWhiteSpace([string] $solutionPathProperty.Value)) {
            return [string] $solutionPathProperty.Value
        }
    }

    foreach ($candidate in @("Oan.sln", "San.sln")) {
        if (Test-Path -LiteralPath (Join-Path $LineRootPath $candidate) -PathType Leaf) {
            return $candidate
        }
    }

    $solution = Get-ChildItem -LiteralPath $LineRootPath -Filter *.sln -File -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($null -ne $solution) {
        return $solution.Name
    }

    return $null
}

function Get-TestCountsFromSource {
    param([string] $LineRootPath)

    $testsRoot = Join-Path $LineRootPath "tests"
    if (-not (Test-Path -LiteralPath $testsRoot -PathType Container)) {
        return [pscustomobject]@{
            audit = 0
            integration = 0
        }
    }

    $files = [System.IO.Directory]::EnumerateFiles($testsRoot, "*.cs", [System.IO.SearchOption]::AllDirectories) |
        Where-Object { $_ -notmatch '\\bin\\' -and $_ -notmatch '\\obj\\' }

    $auditCount = 0
    $integrationCount = 0

    foreach ($file in $files) {
        $text = Get-Content -LiteralPath $file -Raw
        $count = [regex]::Matches($text, '^\s*\[(Fact|Theory)\]', [System.Text.RegularExpressions.RegexOptions]::Multiline).Count

        if ((Test-TextContains -Text $file -Value "Audit.Tests" -Comparison ([System.StringComparison]::OrdinalIgnoreCase))) {
            $auditCount += $count
            continue
        }

        if ((Test-TextContains -Text $file -Value "IntegrationTests" -Comparison ([System.StringComparison]::OrdinalIgnoreCase))) {
            $integrationCount += $count
        }
    }

    return [pscustomobject]@{
        audit = $auditCount
        integration = $integrationCount
    }
}

function Invoke-ReadOnlyPowershellScript {
    param([string] $ScriptPath)

    if (-not (Test-Path -LiteralPath $ScriptPath -PathType Leaf)) {
        return [pscustomobject]@{
            status = "undeclared"
            output = @()
        }
    }

    try {
        $output = & powershell -ExecutionPolicy Bypass -File $ScriptPath 2>&1
        $exitCode = $LASTEXITCODE
    }
    catch {
        $output = @($_.Exception.Message)
        $exitCode = 1
    }

    return [pscustomobject]@{
        status = if ($exitCode -eq 0) { "current-pass" } else { "current-fail" }
        output = @($output | ForEach-Object { "$_" })
    }
}

function Get-GitDiffCheckNoise {
    param(
        [string] $RepoRoot,
        [string] $LineRoot
    )

    try {
        $output = & git -C $RepoRoot diff --check -- $LineRoot 2>&1
        $exitCode = $LASTEXITCODE
    }
    catch {
        $output = @($_.Exception.Message)
        $exitCode = 1
    }

    $lines = @(
        $output |
            ForEach-Object { "$_".Trim() } |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    )

    $nonWarningLines = @($lines | Where-Object { $_ -notlike "warning:*" })

    return [pscustomobject]@{
        status = if ($lines.Count -eq 0) { "clean" } elseif ($nonWarningLines.Count -eq 0) { "known-noise-present" } else { "needs-review" }
        exitCode = $exitCode
        lines = $lines
    }
}

function Get-TelemetryTaxonomyObject {
    param([AllowNull()][string] $TaxonomyText)

    if ([string]::IsNullOrWhiteSpace($TaxonomyText)) {
        return [pscustomobject]@{
            semanticClasses = "undeclared"
            packageClasses = "undeclared"
            authorityClasses = "undeclared"
            continuityClasses = "undeclared"
            retentionClasses = "undeclared"
            domainGroupoid = "undeclared"
            splineGroupoid = "undeclared"
        }
    }

    $semanticClasses = @(Get-MarkdownBacktickedBulletValues -Text $TaxonomyText -Heading "## Semantic Carrier Classes")
    $packageClasses = @(Get-MarkdownBacktickedBulletValues -Text $TaxonomyText -Heading "## Packaging Classes")
    $domainGroupoid = @(Get-MarkdownArrowSequenceValues -Text $TaxonomyText -Heading "## Telemetry Groupoids" -AxisName "domain")
    $splineGroupoid = @(Get-MarkdownArrowSequenceValues -Text $TaxonomyText -Heading "## Telemetry Groupoids" -AxisName "spline")
    $authorityClasses = @(Get-MarkdownArrowSequenceValues -Text $TaxonomyText -Heading "## Telemetry Groupoids" -AxisName "authorityClass")
    $continuityClasses = @(Get-MarkdownArrowSequenceValues -Text $TaxonomyText -Heading "## Telemetry Groupoids" -AxisName "continuityClass")
    $retentionClasses = @(Get-MarkdownArrowSequenceValues -Text $TaxonomyText -Heading "## Telemetry Groupoids" -AxisName "retentionClass")

    return [pscustomobject]@{
        semanticClasses = if ($semanticClasses.Count -gt 0) { $semanticClasses } else { "undeclared" }
        packageClasses = if ($packageClasses.Count -gt 0) { $packageClasses } else { "undeclared" }
        authorityClasses = if ($authorityClasses.Count -gt 0) { $authorityClasses } else { "undeclared" }
        continuityClasses = if ($continuityClasses.Count -gt 0) { $continuityClasses } else { "undeclared" }
        retentionClasses = if ($retentionClasses.Count -gt 0) { $retentionClasses } else { "undeclared" }
        domainGroupoid = if ($domainGroupoid.Count -gt 0) { $domainGroupoid } else { "undeclared" }
        splineGroupoid = if ($splineGroupoid.Count -gt 0) { $splineGroupoid } else { "undeclared" }
    }
}

function Get-LineTelemetryInventoryDescriptors {
    param([string] $LineName)

    if ([string]::Equals($LineName, "OAN Mortalis V1.1.1", [System.StringComparison]::OrdinalIgnoreCase)) {
        return @(
            [ordered]@{
                surfaceName = "companion-tool-telemetry-last-run"
                domain = "Sanctuary"
                spline = "Run"
                semanticClass = "standing_surface"
                authorityClass = "evidence"
                continuityClass = "line"
                retentionClass = "rolling_state"
                packageClass = "state_surface"
                declaredBy = "docs\COMPANION_TOOL_TELEMETRY_LANE.md"
                stateSurfacePath = ".audit\state\local-automation-companion-tool-telemetry-last-run.json"
                runSurfacePath = ".audit\runs\companion-tool-telemetry\"
                notes = @("bounded audit evidence", "companion telemetry does not widen runtime authority")
            }
            [ordered]@{
                surfaceName = "source-bucket-federation-status"
                domain = "Sanctuary"
                spline = "Build"
                semanticClass = "standing_surface"
                authorityClass = "evidence"
                continuityClass = "line"
                retentionClass = "rolling_state"
                packageClass = "state_surface"
                declaredBy = "docs\SOURCE_BUCKET_FEDERATION_LANE.md"
                stateSurfacePath = ".audit\state\source-bucket-federation-status.json"
                notes = @("line-local federation control-plane standing")
            }
            [ordered]@{
                surfaceName = "source-bucket-request-index"
                domain = "Sanctuary"
                spline = "Build"
                semanticClass = "standing_surface"
                authorityClass = "evidence"
                continuityClass = "line"
                retentionClass = "rolling_state"
                packageClass = "state_surface"
                declaredBy = "docs\SOURCE_BUCKET_FEDERATION_LANE.md"
                stateSurfacePath = ".audit\state\source-bucket-request-index.json"
                notes = @("bounded request publication index")
            }
            [ordered]@{
                surfaceName = "source-bucket-return-index"
                domain = "Sanctuary"
                spline = "Build"
                semanticClass = "standing_surface"
                authorityClass = "evidence"
                continuityClass = "line"
                retentionClass = "rolling_state"
                packageClass = "state_surface"
                declaredBy = "docs\SOURCE_BUCKET_FEDERATION_LANE.md"
                stateSurfacePath = ".audit\state\source-bucket-return-index.json"
                notes = @("bounded return intake index")
            }
            [ordered]@{
                surfaceName = "source-bucket-return-integration-status"
                domain = "Sanctuary"
                spline = "Build"
                semanticClass = "standing_surface"
                authorityClass = "evidence"
                continuityClass = "line"
                retentionClass = "rolling_state"
                packageClass = "state_surface"
                declaredBy = "docs\SOURCE_BUCKET_FEDERATION_LANE.md"
                stateSurfacePath = ".audit\state\source-bucket-return-integration-status.json"
                summarySurfacePath = ".audit\state\source-bucket-return-integration-status.md"
                notes = @("listener and integration standing")
            }
            [ordered]@{
                surfaceName = "source-bucket-report-consumption-summary"
                domain = "Sanctuary"
                spline = "Rest"
                semanticClass = "summary_digest"
                authorityClass = "evidence"
                continuityClass = "line"
                retentionClass = "rolling_state"
                packageClass = "state_surface"
                declaredBy = "docs\SOURCE_BUCKET_REPORT_CONSUMPTION_LANE.md"
                stateSurfacePath = ".audit\state\current-source-bucket-standing-summary.json"
                summarySurfacePath = ".audit\state\current-source-bucket-standing-summary.md"
                notes = @("consumed standing summary", "derived read-only condensation")
            }
            [ordered]@{
                surfaceName = "source-bucket-candidate-gel-items"
                domain = "Sanctuary"
                spline = "Rest"
                semanticClass = "candidate_packet"
                authorityClass = "candidate"
                continuityClass = "line"
                retentionClass = "pinned_review"
                packageClass = "state_surface"
                declaredBy = "docs\SOURCE_BUCKET_REPORT_CONSUMPTION_LANE.md"
                stateSurfacePath = ".audit\state\current-candidate-gel-items.json"
                summarySurfacePath = ".audit\state\current-candidate-gel-items.md"
                notes = @("candidate-only carry-forward material", "no direct shortcut to GEL candidate admission")
            }
            [ordered]@{
                surfaceName = "source-bucket-report-consumption-bundles"
                domain = "Sanctuary"
                spline = "Rest"
                semanticClass = "receipt"
                authorityClass = "evidence"
                continuityClass = "event"
                retentionClass = "daily_compacted"
                packageClass = "run_bundle"
                declaredBy = "docs\SOURCE_BUCKET_REPORT_CONSUMPTION_LANE.md"
                runSurfacePath = ".audit\runs\report-consumption\"
                summarySurfacePath = ".audit\runs\report-consumption-daily\"
                notes = @("consumption receipts and daily compaction roots")
            }
        )
    }

    return @()
}

function Convert-InventoryDescriptorToObject {
    param(
        [hashtable] $Descriptor,
        [string] $LineRootPath
    )

    $notes = [System.Collections.Generic.List[string]]::new()
    foreach ($note in @($Descriptor.notes)) {
        if (-not [string]::IsNullOrWhiteSpace([string] $note)) {
            $notes.Add([string] $note)
        }
    }

    foreach ($pathFieldName in @("stateSurfacePath", "runSurfacePath", "summarySurfacePath")) {
        $property = $Descriptor[$pathFieldName]
        if ([string]::IsNullOrWhiteSpace([string] $property)) {
            continue
        }

        $resolvedPath = Join-Path $LineRootPath ([string] $property)
        if (-not (Test-Path -LiteralPath $resolvedPath)) {
            $notes.Add(("{0} currently unavailable" -f $pathFieldName))
        }
    }

    return [pscustomobject][ordered]@{
        surfaceName = [string] $Descriptor.surfaceName
        domain = [string] $Descriptor.domain
        spline = [string] $Descriptor.spline
        semanticClass = [string] $Descriptor.semanticClass
        authorityClass = [string] $Descriptor.authorityClass
        continuityClass = [string] $Descriptor.continuityClass
        retentionClass = [string] $Descriptor.retentionClass
        packageClass = [string] $Descriptor.packageClass
        declaredBy = [string] $Descriptor.declaredBy
        stateSurfacePath = if ($Descriptor.Contains("stateSurfacePath")) { [string] $Descriptor.stateSurfacePath } else { "undeclared" }
        runSurfacePath = if ($Descriptor.Contains("runSurfacePath")) { [string] $Descriptor.runSurfacePath } else { "undeclared" }
        summarySurfacePath = if ($Descriptor.Contains("summarySurfacePath")) { [string] $Descriptor.summarySurfacePath } else { "undeclared" }
        notes = @($notes)
    }
}

function Get-LineAuditReportObject {
    param(
        [string] $RepoRoot,
        [string] $LineRoot
    )

    $lineRootPath = Join-Path $RepoRoot $LineRoot
    if (-not (Test-Path -LiteralPath $lineRootPath -PathType Container)) {
        throw "Line root '$LineRoot' was not found under '$RepoRoot'."
    }

    $lineManifestPath = Join-Path $lineRootPath "build\line-manifest.json"
    $lineManifest = Get-JsonObjectOrNull -Path $lineManifestPath
    $lineName = if ($null -ne $lineManifest -and $null -ne $lineManifest.PSObject.Properties["lineName"]) { [string] $lineManifest.lineName } else { $LineRoot }
    $solutionRelativePath = Get-SolutionRelativePath -LineRootPath $lineRootPath -LineManifest $lineManifest

    $buildReadinessPath = Join-Path $lineRootPath "docs\BUILD_READINESS.md"
    $groupoidAuditPath = Join-Path $lineRootPath "docs\V1_1_1_DOMAIN_AND_SPLINE_GROUPOID_AUDIT.md"
    if (-not (Test-Path -LiteralPath $groupoidAuditPath -PathType Leaf)) {
        $groupoidAuditPath = Join-Path $lineRootPath "docs\V1_2_1_DOMAIN_AND_SPLINE_GROUPOID_AUDIT.md"
    }

    $taxonomyPath = Join-Path $lineRootPath "docs\TELEMETRY_BUNDLE_AND_GROUPOID_TAXONOMY.md"
    $schemaPath = Join-Path $lineRootPath "docs\LINE_AUDIT_REPORT_SCHEMA.md"
    $companionTelemetryPath = Join-Path $lineRootPath "docs\COMPANION_TOOL_TELEMETRY_LANE.md"
    $sourceBucketFederationPath = Join-Path $lineRootPath "docs\SOURCE_BUCKET_FEDERATION_LANE.md"
    $sourceBucketConsumptionPath = Join-Path $lineRootPath "docs\SOURCE_BUCKET_REPORT_CONSUMPTION_LANE.md"

    $buildReadinessText = Get-FileTextOrNull -Path $buildReadinessPath
    $groupoidAuditText = Get-FileTextOrNull -Path $groupoidAuditPath
    $taxonomyText = Get-FileTextOrNull -Path $taxonomyPath
    $taxonomyDeclared = -not [string]::IsNullOrWhiteSpace($taxonomyText)
    $schemaDeclared = Test-Path -LiteralPath $schemaPath -PathType Leaf

    $hygienePath = Join-Path $lineRootPath "tools\verify-private-corpus.ps1"
    $hygieneResult = Invoke-ReadOnlyPowershellScript -ScriptPath $hygienePath
    $diffCheck = Get-GitDiffCheckNoise -RepoRoot $RepoRoot -LineRoot $LineRoot
    $testCounts = Get-TestCountsFromSource -LineRootPath $lineRootPath

    $documentedBuildStatus = if (Test-TextContains -Text $buildReadinessText -Value "- build succeeded") { "documented-succeeded" } else { "unavailable" }
    $documentedTestStatus = if (Test-TextContains -Text $buildReadinessText -Value "- tests succeeded") { "documented-succeeded" } else { "unavailable" }

    $activeExecutableTruthStatus = if ($null -ne $lineManifest -and $null -ne $lineManifest.PSObject.Properties["activeExecutableTruth"]) {
        if ([string]::Equals([string] $lineManifest.activeExecutableTruth, $lineName, [System.StringComparison]::OrdinalIgnoreCase)) {
            "active-executable-truth"
        }
        else {
            "inactive-sibling"
        }
    }
    elseif ([string]::Equals($lineName, "OAN Mortalis V1.1.1", [System.StringComparison]::OrdinalIgnoreCase)) {
        "active-executable-truth"
    }
    else {
        "undeclared"
    }

    $siblingRelation = if ($null -ne $lineManifest -and $null -ne $lineManifest.PSObject.Properties["parentLine"] -and -not [string]::IsNullOrWhiteSpace([string] $lineManifest.parentLine)) {
        "sibling-of " + [string] $lineManifest.parentLine
    }
    elseif ([string]::Equals($lineName, "OAN Mortalis V1.1.1", [System.StringComparison]::OrdinalIgnoreCase) -and (Test-Path -LiteralPath (Join-Path $RepoRoot "OAN Mortalis V1.2.1") -PathType Container)) {
        "active-parent-of OAN Mortalis V1.2.1"
    }
    else {
        "undeclared"
    }

    $currentObjectiveSection = Get-MarkdownSectionBody -Text $buildReadinessText -Heading "## Immediate Line Objective"
    if ([string]::IsNullOrWhiteSpace($currentObjectiveSection)) {
        $currentObjectiveSection = Get-MarkdownSectionBody -Text $buildReadinessText -Heading "## Purpose"
    }

    $currentObjective = Convert-SectionBodyToSingleLine -Text $currentObjectiveSection
    if ([string]::IsNullOrWhiteSpace($currentObjective)) {
        $currentObjective = "undeclared"
    }

    $buildabilityStatus = if ($null -ne $lineManifest -and $null -ne $lineManifest.PSObject.Properties["buildable"]) {
        if ([bool] $lineManifest.buildable) { "buildable" } else { "not-buildable" }
    }
    elseif (-not [string]::IsNullOrWhiteSpace($solutionRelativePath)) {
        "solution-present"
    }
    else {
        "undeclared"
    }

    $stateClass = if ($null -ne $lineManifest -and $null -ne $lineManifest.PSObject.Properties["stateClass"]) {
        [string] $lineManifest.stateClass
    }
    elseif ($activeExecutableTruthStatus -eq "active-executable-truth") {
        "ActiveExecutableTruth"
    }
    else {
        "undeclared"
    }

    $authorityPosture = if ($null -ne $lineManifest -and $null -ne $lineManifest.PSObject.Properties["posture"]) {
        [string] $lineManifest.posture
    }
    elseif ($activeExecutableTruthStatus -eq "active-executable-truth") {
        "active-executable-truth"
    }
    else {
        "undeclared"
    }

    $keyLaneSurfaces = [System.Collections.Generic.List[string]]::new()
    foreach ($candidatePath in @($companionTelemetryPath, $sourceBucketFederationPath, $sourceBucketConsumptionPath)) {
        if (Test-Path -LiteralPath $candidatePath -PathType Leaf) {
            $keyLaneSurfaces.Add((Get-RelativePathString -BasePath $lineRootPath -TargetPath $candidatePath))
        }
    }

    $braidStatus = if (
        (Test-Path -LiteralPath $buildReadinessPath -PathType Leaf) -and
        (Test-Path -LiteralPath $groupoidAuditPath -PathType Leaf) -and
        $taxonomyDeclared -and
        $schemaDeclared
    ) { "anchored" } else { "partial" }

    $inventory = @(
        Get-LineTelemetryInventoryDescriptors -LineName $lineName |
            ForEach-Object { Convert-InventoryDescriptorToObject -Descriptor $_ -LineRootPath $lineRootPath }
    )

    $warnings = [System.Collections.Generic.List[string]]::new()
    if (-not $taxonomyDeclared) {
        $warnings.Add("Telemetry taxonomy is undeclared for this line.")
    }
    if (-not $schemaDeclared) {
        $warnings.Add("line-audit-report schema is undeclared for this line.")
    }
    if ($hygieneResult.status -eq "current-fail") {
        $warnings.Add("Current hygiene check failed.")
    }
    if ([string]::Equals($documentedBuildStatus, "unavailable", [System.StringComparison]::OrdinalIgnoreCase)) {
        $warnings.Add("Current build status is unavailable from declared read-only surfaces.")
    }
    if ([string]::Equals($documentedTestStatus, "unavailable", [System.StringComparison]::OrdinalIgnoreCase)) {
        $warnings.Add("Current test status is unavailable from declared read-only surfaces.")
    }

    $knownNoise = @()
    if ($diffCheck.lines.Count -gt 0) {
        $knownNoise = @(
            $diffCheck.lines | ForEach-Object {
                [pscustomobject]@{
                    noiseClass = "git-diff-check"
                    currentDisposition = "known-noise"
                    sourceSurface = "$_"
                }
            }
        )
    }

    $unavailableOrUndeclared = [System.Collections.Generic.List[object]]::new()
    if ($inventory.Count -eq 0) {
        $unavailableOrUndeclared.Add([pscustomobject]@{
            section = "telemetrySurfaceInventory"
            field = "inventory"
            status = if ([string]::Equals($lineName, "OAN Mortalis V1.2.1", [System.StringComparison]::OrdinalIgnoreCase)) { "undeclared" } else { "unavailable" }
            reason = if ([string]::Equals($lineName, "OAN Mortalis V1.2.1", [System.StringComparison]::OrdinalIgnoreCase)) { "Install-first line has not yet declared a thicker emitted telemetry inventory." } else { "No inventory descriptors were available." }
        })
    }

    if ($documentedBuildStatus -eq "unavailable") {
        $unavailableOrUndeclared.Add([pscustomobject]@{
            section = "verificationStatus"
            field = "buildStatus"
            status = "unavailable"
            reason = "No current build result is declared in read-only line surfaces."
        })
    }

    if ($documentedTestStatus -eq "unavailable") {
        $unavailableOrUndeclared.Add([pscustomobject]@{
            section = "verificationStatus"
            field = "testStatus"
            status = "unavailable"
            reason = "No current test result is declared in read-only line surfaces."
        })
    }

    $report = [pscustomobject][ordered]@{
        reportIdentity = [pscustomobject][ordered]@{
            reportSurfaceName = "line-audit-report"
            reportClass = "read-only-line-witness"
            generatedAtUtc = (Get-Date).ToUniversalTime().ToString("o")
            sourceLine = $lineName
        }
        lineIdentity = [pscustomobject][ordered]@{
            lineName = $lineName
            lineRoot = $LineRoot
            solutionPath = if ([string]::IsNullOrWhiteSpace($solutionRelativePath)) { "undeclared" } else { $solutionRelativePath }
            activeExecutableTruthStatus = $activeExecutableTruthStatus
            siblingRelation = $siblingRelation
        }
        linePosture = [pscustomobject][ordered]@{
            stateClass = $stateClass
            currentObjective = $currentObjective
            buildabilityStatus = $buildabilityStatus
            authorityPosture = $authorityPosture
            readOnlyStanding = "read-only"
        }
        verificationStatus = [pscustomobject][ordered]@{
            buildStatus = $documentedBuildStatus
            testStatus = $documentedTestStatus
            hygieneStatus = $hygieneResult.status
            diffCheckStatus = $diffCheck.status
            auditCount = $testCounts.audit
            integrationCount = $testCounts.integration
            verificationSource = "documented-readiness-plus-read-only-probes-and-source-counts"
        }
        doctrineBraid = [pscustomobject][ordered]@{
            readinessSurface = if (Test-Path -LiteralPath $buildReadinessPath -PathType Leaf) { (Get-RelativePathString -BasePath $lineRootPath -TargetPath $buildReadinessPath) } else { "undeclared" }
            groupoidAuditSurface = if (Test-Path -LiteralPath $groupoidAuditPath -PathType Leaf) { (Get-RelativePathString -BasePath $lineRootPath -TargetPath $groupoidAuditPath) } else { "undeclared" }
            telemetryTaxonomySurface = if ($taxonomyDeclared) { (Get-RelativePathString -BasePath $lineRootPath -TargetPath $taxonomyPath) } else { "undeclared" }
            keyLaneSurfaces = @($keyLaneSurfaces)
            braidStatus = $braidStatus
        }
        telemetryTaxonomy = Get-TelemetryTaxonomyObject -TaxonomyText $taxonomyText
        telemetrySurfaceInventory = @($inventory)
        warnings = @($warnings)
        knownNoise = @($knownNoise)
        unavailableOrUndeclared = @($unavailableOrUndeclared)
    }

    return $report
}

function Convert-ArrayToMarkdownList {
    param([object] $Value)

    if ($null -eq $Value) {
        return '- `undeclared`'
    }

    if ($Value -is [string]) {
        return ('- `{0}`' -f $Value)
    }

    $items = @($Value)
    if ($items.Count -eq 0) {
        return '- `undeclared`'
    }

    return ($items | ForEach-Object { '- `{0}`' -f [string] $_ }) -join [Environment]::NewLine
}

function Convert-LineAuditReportToMarkdown {
    param([object] $Report)

    $inventoryBlocks = @()
    foreach ($item in @($Report.telemetrySurfaceInventory)) {
        $inventoryBlocks += @(
            "### {0}" -f $item.surfaceName,
            "",
            '- domain: `{0}`' -f $item.domain,
            '- spline: `{0}`' -f $item.spline,
            '- semanticClass: `{0}`' -f $item.semanticClass,
            '- authorityClass: `{0}`' -f $item.authorityClass,
            '- continuityClass: `{0}`' -f $item.continuityClass,
            '- retentionClass: `{0}`' -f $item.retentionClass,
            '- packageClass: `{0}`' -f $item.packageClass,
            '- declaredBy: `{0}`' -f $item.declaredBy,
            '- stateSurfacePath: `{0}`' -f $item.stateSurfacePath,
            '- runSurfacePath: `{0}`' -f $item.runSurfacePath,
            '- summarySurfacePath: `{0}`' -f $item.summarySurfacePath,
            "- notes:"
        )

        foreach ($note in @($item.notes)) {
            $inventoryBlocks += "  - {0}" -f $note
        }

        $inventoryBlocks += ""
    }

    if ($inventoryBlocks.Count -eq 0) {
        $inventoryBlocks = @('- `undeclared`')
    }

    $warningLines = if (@($Report.warnings).Count -eq 0) { @("- none") } else { @($Report.warnings | ForEach-Object { "- {0}" -f $_ }) }
    $noiseLines = if (@($Report.knownNoise).Count -eq 0) { @("- none") } else { @($Report.knownNoise | ForEach-Object { "- {0}: {1}" -f $_.noiseClass, $_.sourceSurface }) }
    $unavailableLines = if (@($Report.unavailableOrUndeclared).Count -eq 0) { @("- none") } else { @($Report.unavailableOrUndeclared | ForEach-Object { '- {0}.{1}: `{2}` ({3})' -f $_.section, $_.field, $_.status, $_.reason }) }

    return @(
        "# line-audit-report",
        "",
        "## Report Identity",
        "",
        '- reportSurfaceName: `{0}`' -f $Report.reportIdentity.reportSurfaceName,
        '- reportClass: `{0}`' -f $Report.reportIdentity.reportClass,
        '- generatedAtUtc: `{0}`' -f $Report.reportIdentity.generatedAtUtc,
        '- sourceLine: `{0}`' -f $Report.reportIdentity.sourceLine,
        "",
        "## Line Identity",
        "",
        '- lineName: `{0}`' -f $Report.lineIdentity.lineName,
        '- lineRoot: `{0}`' -f $Report.lineIdentity.lineRoot,
        '- solutionPath: `{0}`' -f $Report.lineIdentity.solutionPath,
        '- activeExecutableTruthStatus: `{0}`' -f $Report.lineIdentity.activeExecutableTruthStatus,
        '- siblingRelation: `{0}`' -f $Report.lineIdentity.siblingRelation,
        "",
        "## Line Posture",
        "",
        '- stateClass: `{0}`' -f $Report.linePosture.stateClass,
        "- currentObjective: {0}" -f $Report.linePosture.currentObjective,
        '- buildabilityStatus: `{0}`' -f $Report.linePosture.buildabilityStatus,
        '- authorityPosture: `{0}`' -f $Report.linePosture.authorityPosture,
        '- readOnlyStanding: `{0}`' -f $Report.linePosture.readOnlyStanding,
        "",
        "## Verification Status",
        "",
        '- buildStatus: `{0}`' -f $Report.verificationStatus.buildStatus,
        '- testStatus: `{0}`' -f $Report.verificationStatus.testStatus,
        '- hygieneStatus: `{0}`' -f $Report.verificationStatus.hygieneStatus,
        '- diffCheckStatus: `{0}`' -f $Report.verificationStatus.diffCheckStatus,
        '- auditCount: `{0}`' -f $Report.verificationStatus.auditCount,
        '- integrationCount: `{0}`' -f $Report.verificationStatus.integrationCount,
        '- verificationSource: `{0}`' -f $Report.verificationStatus.verificationSource,
        "",
        "## Doctrine Braid",
        "",
        '- readinessSurface: `{0}`' -f $Report.doctrineBraid.readinessSurface,
        '- groupoidAuditSurface: `{0}`' -f $Report.doctrineBraid.groupoidAuditSurface,
        '- telemetryTaxonomySurface: `{0}`' -f $Report.doctrineBraid.telemetryTaxonomySurface,
        '- braidStatus: `{0}`' -f $Report.doctrineBraid.braidStatus,
        "- keyLaneSurfaces:",
        (Convert-ArrayToMarkdownList -Value $Report.doctrineBraid.keyLaneSurfaces),
        "",
        "## Telemetry Taxonomy",
        "",
        "- semanticClasses:",
        (Convert-ArrayToMarkdownList -Value $Report.telemetryTaxonomy.semanticClasses),
        "- packageClasses:",
        (Convert-ArrayToMarkdownList -Value $Report.telemetryTaxonomy.packageClasses),
        "- authorityClasses:",
        (Convert-ArrayToMarkdownList -Value $Report.telemetryTaxonomy.authorityClasses),
        "- continuityClasses:",
        (Convert-ArrayToMarkdownList -Value $Report.telemetryTaxonomy.continuityClasses),
        "- retentionClasses:",
        (Convert-ArrayToMarkdownList -Value $Report.telemetryTaxonomy.retentionClasses),
        "- domainGroupoid:",
        (Convert-ArrayToMarkdownList -Value $Report.telemetryTaxonomy.domainGroupoid),
        "- splineGroupoid:",
        (Convert-ArrayToMarkdownList -Value $Report.telemetryTaxonomy.splineGroupoid),
        "",
        "## Telemetry Surface Inventory",
        "",
        $inventoryBlocks,
        "## Warnings",
        "",
        $warningLines,
        "",
        "## Known Noise",
        "",
        $noiseLines,
        "",
        "## Unavailable Or Undeclared",
        "",
        $unavailableLines
    ) -join [Environment]::NewLine
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$report = Get-LineAuditReportObject -RepoRoot $repoRoot -LineRoot $LineRoot

if ($Format -eq "Json") {
    $report | ConvertTo-Json -Depth 8
    exit 0
}

Convert-LineAuditReportToMarkdown -Report $report
