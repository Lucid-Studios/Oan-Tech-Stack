function Get-OanWorkspaceCyclePolicyValue {
    param(
        [object] $CyclePolicy,
        [string] $PropertyName
    )

    if ($null -eq $CyclePolicy) {
        return $null
    }

    $property = $CyclePolicy.PSObject.Properties[$PropertyName]
    if ($null -eq $property) {
        return $null
    }

    return $property.Value
}

function Join-OanWorkspacePath {
    param(
        [string] $BasePath,
        [string] $CandidatePath
    )

    if ([string]::IsNullOrWhiteSpace($CandidatePath)) {
        return $null
    }

    if ([System.IO.Path]::IsPathRooted($CandidatePath)) {
        return [System.IO.Path]::GetFullPath($CandidatePath)
    }

    $normalizedCandidatePath = $CandidatePath.Replace('/', '\')
    return [System.IO.Path]::GetFullPath((Join-Path $BasePath $normalizedCandidatePath))
}

function Add-OanWorkspaceCandidate {
    param(
        [System.Collections.Generic.List[string]] $Candidates,
        [string] $BasePath,
        [string] $CandidatePath
    )

    $resolvedCandidate = Join-OanWorkspacePath -BasePath $BasePath -CandidatePath $CandidatePath
    if ([string]::IsNullOrWhiteSpace($resolvedCandidate)) {
        return
    }

    foreach ($existingCandidate in $Candidates) {
        if ([string]::Equals($existingCandidate, $resolvedCandidate, [System.StringComparison]::OrdinalIgnoreCase)) {
            return
        }
    }

    $Candidates.Add($resolvedCandidate)
}

function Get-OanWorkspaceContext {
    param(
        [string] $BasePath,
        [object] $CyclePolicy
    )

    if ($null -eq $CyclePolicy) {
        $cyclePolicyVariable = Get-Variable -Name cyclePolicy -Scope Script -ErrorAction SilentlyContinue
        if ($null -ne $cyclePolicyVariable) {
            $CyclePolicy = $cyclePolicyVariable.Value
        }
    }

    $activeBuildRoot = [string] (Get-OanWorkspaceCyclePolicyValue -CyclePolicy $CyclePolicy -PropertyName 'activeBuildRoot')
    if ([string]::IsNullOrWhiteSpace($activeBuildRoot)) {
        $activeBuildRoot = 'OAN Mortalis V1.1.1'
    }

    $automationPolicyRoot = [string] (Get-OanWorkspaceCyclePolicyValue -CyclePolicy $CyclePolicy -PropertyName 'automationPolicyRoot')
    if ([string]::IsNullOrWhiteSpace($automationPolicyRoot)) {
        $automationPolicyRoot = 'OAN Mortalis V1.1.1/build'
    }

    $resolvedBasePath = [System.IO.Path]::GetFullPath($BasePath)
    $legacyWorkspaceRoot = $activeBuildRoot
    $policyWorkspaceRoot = Split-Path -Parent $automationPolicyRoot

    return [ordered]@{
        BasePath = $resolvedBasePath
        ActiveBuildRoot = $activeBuildRoot.Replace('\', '/')
        AutomationPolicyRoot = $automationPolicyRoot.Replace('\', '/')
        PolicyWorkspaceRoot = $policyWorkspaceRoot.Replace('\', '/')
        LegacyWorkspaceRoot = $legacyWorkspaceRoot
    }
}

$script:OanWorkspaceTouchPointMatrixCache = @{}

function Get-OanWorkspaceTouchPointMatrixPath {
    param(
        [string] $BasePath,
        [object] $CyclePolicy
    )

    $context = Get-OanWorkspaceContext -BasePath $BasePath -CyclePolicy $CyclePolicy
    $matrixPath = [string] (Get-OanWorkspaceCyclePolicyValue -CyclePolicy $CyclePolicy -PropertyName 'versionedTouchPointMatrixPath')
    if ([string]::IsNullOrWhiteSpace($matrixPath)) {
        $matrixPath = '{0}/versioned-touchpoint-matrix.json' -f $context.AutomationPolicyRoot
    }

    return Join-OanWorkspacePath -BasePath $context.BasePath -CandidatePath $matrixPath
}

function Read-OanWorkspaceTouchPointMatrix {
    param(
        [string] $BasePath,
        [object] $CyclePolicy
    )

    if (-not (Get-Variable -Name OanWorkspaceTouchPointMatrixCache -Scope Script -ErrorAction SilentlyContinue)) {
        $script:OanWorkspaceTouchPointMatrixCache = @{}
    }

    $matrixPath = Get-OanWorkspaceTouchPointMatrixPath -BasePath $BasePath -CyclePolicy $CyclePolicy
    if ([string]::IsNullOrWhiteSpace($matrixPath) -or -not (Test-Path -LiteralPath $matrixPath -PathType Leaf)) {
        return $null
    }

    if (-not $script:OanWorkspaceTouchPointMatrixCache.ContainsKey($matrixPath)) {
        $script:OanWorkspaceTouchPointMatrixCache[$matrixPath] = Get-Content -Raw -LiteralPath $matrixPath | ConvertFrom-Json
    }

    return $script:OanWorkspaceTouchPointMatrixCache[$matrixPath]
}

function Expand-OanWorkspacePathTemplate {
    param(
        [string] $Template,
        [hashtable] $Context
    )

    if ([string]::IsNullOrWhiteSpace($Template)) {
        return $Template
    }

    $expandedTemplate = $Template
    foreach ($entry in $Context.GetEnumerator()) {
        $expandedTemplate = $expandedTemplate.Replace(('{' + $entry.Key + '}'), [string] $entry.Value)
    }

    return $expandedTemplate
}

function Get-OanWorkspaceTouchPointDefinition {
    param(
        [string] $BasePath,
        [string] $TouchPointId,
        [object] $CyclePolicy
    )

    if ([string]::IsNullOrWhiteSpace($TouchPointId)) {
        return $null
    }

    $matrix = Read-OanWorkspaceTouchPointMatrix -BasePath $BasePath -CyclePolicy $CyclePolicy
    if ($null -eq $matrix -or $null -eq $matrix.touchPoints) {
        return $null
    }

    $property = $matrix.touchPoints.PSObject.Properties[$TouchPointId]
    if ($null -eq $property) {
        return $null
    }

    return $property.Value
}

function Get-OanWorkspaceTouchPointResolution {
    param(
        [string] $BasePath,
        [string] $TouchPointId,
        [object] $CyclePolicy
    )

    $definition = Get-OanWorkspaceTouchPointDefinition -BasePath $BasePath -TouchPointId $TouchPointId -CyclePolicy $CyclePolicy
    if ($null -eq $definition) {
        return $null
    }

    $context = Get-OanWorkspaceContext -BasePath $BasePath -CyclePolicy $CyclePolicy
    $templateContext = @{
        activeBuildRoot = $context.ActiveBuildRoot
        automationPolicyRoot = $context.AutomationPolicyRoot
        policyWorkspaceRoot = $context.PolicyWorkspaceRoot
        legacyWorkspaceRoot = $context.LegacyWorkspaceRoot
    }

    $localCandidates = @()
    $localCandidatesProperty = $definition.PSObject.Properties['localCandidates']
    if ($null -ne $localCandidatesProperty) {
        $localCandidates = @($localCandidatesProperty.Value)
    }

    $legacyPathTemplate = $null
    $legacyPathProperty = $definition.PSObject.Properties['legacyPath']
    if ($null -ne $legacyPathProperty) {
        $legacyPathTemplate = [string] $legacyPathProperty.Value
    }

    $preferFirstCandidate = $false
    $preferLocalCandidateProperty = $definition.PSObject.Properties['preferLocalCandidate']
    if ($null -ne $preferLocalCandidateProperty) {
        $preferFirstCandidate = [bool] $preferLocalCandidateProperty.Value
    }

    $candidates = [System.Collections.Generic.List[string]]::new()
    foreach ($template in $localCandidates) {
        $candidatePath = Expand-OanWorkspacePathTemplate -Template ([string] $template) -Context $templateContext
        Add-OanWorkspaceCandidate -Candidates $candidates -BasePath $context.BasePath -CandidatePath $candidatePath
    }

    $fallbackPath = $null
    if (-not [string]::IsNullOrWhiteSpace($legacyPathTemplate)) {
        $fallbackTemplate = Expand-OanWorkspacePathTemplate -Template $legacyPathTemplate -Context $templateContext
        $fallbackPath = Join-OanWorkspacePath -BasePath $context.BasePath -CandidatePath $fallbackTemplate
        Add-OanWorkspaceCandidate -Candidates $candidates -BasePath $context.BasePath -CandidatePath $fallbackTemplate
    }

    if ($null -eq $fallbackPath -and $candidates.Count -gt 0) {
        $fallbackPath = $candidates[0]
    }

    return [pscustomobject]@{
        Candidates = $candidates
        FallbackPath = $fallbackPath
        PreferFirstCandidate = $preferFirstCandidate
        TouchPointId = $TouchPointId
    }
}

function Resolve-OanWorkspaceTouchPoint {
    param(
        [string] $BasePath,
        [string] $TouchPointId,
        [object] $CyclePolicy
    )

    $resolution = Get-OanWorkspaceTouchPointResolution -BasePath $BasePath -TouchPointId $TouchPointId -CyclePolicy $CyclePolicy
    if ($null -eq $resolution) {
        return $null
    }

    foreach ($candidate in $resolution.Candidates) {
        if (Test-Path -LiteralPath $candidate) {
            return $candidate
        }
    }

    if ($resolution.PreferFirstCandidate -and $resolution.Candidates.Count -gt 0) {
        return $resolution.Candidates[0]
    }

    return $resolution.FallbackPath
}

function Resolve-OanWorkspaceTouchPointFamily {
    param(
        [string] $BasePath,
        [string] $FamilyName,
        [object] $CyclePolicy
    )

    if ([string]::IsNullOrWhiteSpace($FamilyName)) {
        return @()
    }

    $matrix = Read-OanWorkspaceTouchPointMatrix -BasePath $BasePath -CyclePolicy $CyclePolicy
    if ($null -eq $matrix -or $null -eq $matrix.families) {
        return @()
    }

    $familyProperty = $matrix.families.PSObject.Properties[$FamilyName]
    if ($null -eq $familyProperty) {
        return @()
    }

    $familyDefinition = $familyProperty.Value
    $touchPointIds = if ($familyDefinition -is [System.Array]) {
        @($familyDefinition)
    } else {
        @($familyDefinition.touchPoints)
    }

    $resolvedPaths = @()
    foreach ($touchPointId in $touchPointIds) {
        $resolvedPath = Resolve-OanWorkspaceTouchPoint -BasePath $BasePath -TouchPointId ([string] $touchPointId) -CyclePolicy $CyclePolicy
        if (-not [string]::IsNullOrWhiteSpace($resolvedPath)) {
            $resolvedPaths += $resolvedPath
        }
    }

    return $resolvedPaths
}

function Get-OanWorkspaceCandidatePaths {
    param(
        [string] $BasePath,
        [string] $CandidatePath,
        [object] $CyclePolicy
    )

    $resolvedBasePath = [System.IO.Path]::GetFullPath($BasePath)
    $originalResolvedPath = Join-OanWorkspacePath -BasePath $resolvedBasePath -CandidatePath $CandidatePath
    $candidates = [System.Collections.Generic.List[string]]::new()

    if ([System.IO.Path]::IsPathRooted($CandidatePath)) {
        $candidates.Add($originalResolvedPath)
        return [pscustomobject]@{
            Candidates = $candidates
            FallbackPath = $originalResolvedPath
            PreferFirstCandidate = $false
        }
    }

    $context = Get-OanWorkspaceContext -BasePath $resolvedBasePath -CyclePolicy $CyclePolicy
    $normalizedCandidatePath = $CandidatePath.Replace('\', '/')
    $preferFirstCandidate = $false
    $matrix = Read-OanWorkspaceTouchPointMatrix -BasePath $resolvedBasePath -CyclePolicy $CyclePolicy

    if ($null -ne $matrix -and $null -ne $matrix.touchPoints) {
        foreach ($touchPointProperty in $matrix.touchPoints.PSObject.Properties) {
            $touchPointDefinition = $touchPointProperty.Value
            $legacyPath = [string] $touchPointDefinition.legacyPath
            if ([string]::IsNullOrWhiteSpace($legacyPath)) {
                continue
            }

            if ([string]::Equals($legacyPath.Replace('\', '/'), $normalizedCandidatePath, [System.StringComparison]::OrdinalIgnoreCase)) {
                return Get-OanWorkspaceTouchPointResolution -BasePath $resolvedBasePath -TouchPointId $touchPointProperty.Name -CyclePolicy $CyclePolicy
            }
        }
    }

    if ($normalizedCandidatePath -match '(?i)^OAN Mortalis V1\.0/(.+)$') {
        $legacyRelativePath = $Matches[1]

        switch -Regex ($legacyRelativePath) {
            '^build/(.+)$' {
                Add-OanWorkspaceCandidate -Candidates $candidates -BasePath $resolvedBasePath -CandidatePath ('{0}/{1}' -f $context.AutomationPolicyRoot, $Matches[1])
                $preferFirstCandidate = $true
                break
            }
            '^tools/(.+)$' {
                Add-OanWorkspaceCandidate -Candidates $candidates -BasePath $resolvedBasePath -CandidatePath ('{0}/tools/{1}' -f $context.ActiveBuildRoot, $Matches[1])
                break
            }
            '^docs/(.+)$' {
                Add-OanWorkspaceCandidate -Candidates $candidates -BasePath $resolvedBasePath -CandidatePath ('{0}/docs/{1}' -f $context.ActiveBuildRoot, $Matches[1])
                break
            }
            '^src/Oan\.Common/(.+)$' {
                Add-OanWorkspaceCandidate -Candidates $candidates -BasePath $resolvedBasePath -CandidatePath ('{0}/src/Sanctuary/Oan.Common/Oan.Common/{1}' -f $context.ActiveBuildRoot, $Matches[1])
                break
            }
            '^src/CradleTek\.Runtime/(.+)$' {
                Add-OanWorkspaceCandidate -Candidates $candidates -BasePath $resolvedBasePath -CandidatePath ('{0}/src/TechStack/CradleTek/CradleTek.Runtime/CradleTek.Runtime/{1}' -f $context.ActiveBuildRoot, $Matches[1])
                break
            }
            '^src/AgentiCore/Services/(.+)$' {
                Add-OanWorkspaceCandidate -Candidates $candidates -BasePath $resolvedBasePath -CandidatePath ('{0}/src/TechStack/AgentiCore/AgentiCore/AgentiCore/Services/{1}' -f $context.ActiveBuildRoot, $Matches[1])
                Add-OanWorkspaceCandidate -Candidates $candidates -BasePath $resolvedBasePath -CandidatePath ('{0}/src/TechStack/AgentiCore/AgentiCore/AgentiCore/{1}' -f $context.ActiveBuildRoot, $Matches[1])
                break
            }
            '^src/AgentiCore\.Runtime/(.+)$' {
                Add-OanWorkspaceCandidate -Candidates $candidates -BasePath $resolvedBasePath -CandidatePath ('{0}/src/TechStack/AgentiCore/AgentiCore.Runtime/AgentiCore.Runtime/{1}' -f $context.ActiveBuildRoot, $Matches[1])
                Add-OanWorkspaceCandidate -Candidates $candidates -BasePath $resolvedBasePath -CandidatePath ('{0}/src/TechStack/AgentiCore/AgentiCore/AgentiCore/{1}' -f $context.ActiveBuildRoot, $Matches[1])
                break
            }
            '^tests/Oan\.Runtime\.IntegrationTests/(.+)$' {
                Add-OanWorkspaceCandidate -Candidates $candidates -BasePath $resolvedBasePath -CandidatePath ('{0}/tests/Sanctuary/Oan.Runtime.IntegrationTests/Oan.Runtime.IntegrationTests/{1}' -f $context.ActiveBuildRoot, $Matches[1])
                break
            }
            '^tests/Oan\.Audit\.Tests/(.+)$' {
                Add-OanWorkspaceCandidate -Candidates $candidates -BasePath $resolvedBasePath -CandidatePath ('{0}/tests/Sanctuary/Oan.Audit.Tests/Oan.Audit.Tests/{1}' -f $context.ActiveBuildRoot, $Matches[1])
                break
            }
        }
    }

    Add-OanWorkspaceCandidate -Candidates $candidates -BasePath $resolvedBasePath -CandidatePath $CandidatePath

    return [pscustomobject]@{
        Candidates = $candidates
        FallbackPath = $originalResolvedPath
        PreferFirstCandidate = $preferFirstCandidate
    }
}

function Resolve-OanWorkspacePath {
    param(
        [string] $BasePath,
        [string] $CandidatePath,
        [object] $CyclePolicy
    )

    $resolution = Get-OanWorkspaceCandidatePaths -BasePath $BasePath -CandidatePath $CandidatePath -CyclePolicy $CyclePolicy
    foreach ($candidate in $resolution.Candidates) {
        if (Test-Path -LiteralPath $candidate) {
            return $candidate
        }
    }

    if ($resolution.PreferFirstCandidate -and $resolution.Candidates.Count -gt 0) {
        return $resolution.Candidates[0]
    }

    return $resolution.FallbackPath
}
