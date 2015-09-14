param($installPath, $toolsPath, $package, $project)

Set-StrictMode -Version 2.0

# Set to nonzero to active debug messages
$global:enableDebug = 0

# Writes a line of text to the NuGet PowerShell console, if debugging is enabled
function Write-Debug ($text) {
    if ($global:enableDebug) {
        Write-Host "DEBUG: $text"
    }
}

# Removes lines like <Rule Id="XXXX" Action="Warning" /> from a .ruleset file
function Remove-Rules-From-RuleSet ($doc, [string[]] $ruleIds) {
    if ($doc.RuleSet.SelectSingleNode('Rules')) {
        foreach ($ruleId in $ruleIds) {
            # Remove rule with matching ID
            $ruleElement = $doc.RuleSet.Rules.SelectSingleNode("Rule[@Id='$ruleId']")
            if ($ruleElement ) {
                $dummy = $doc.RuleSet.Rules.RemoveChild($ruleElement)
                Write-Host "Removed rule '$ruleId' from .ruleset file."
            }
        }

        # Remove empty parent element
        if (!$doc.RuleSet.Rules.HasChildNodes) {
            $dummy = $doc.RuleSet.RemoveChild($doc.RuleSet.Rules)
            Write-Host "Removed empty container 'Rules' from .ruleset file."
        }
    }
}

# Removes lines like <Path>..\some\path\XXX.dll</Path> from a .ruleset file
function Remove-RuleAssembly-From-RuleSet ($doc, $ruleAssemblyName) {
    # Remove rule assembly path
    if ($doc.RuleSet.SelectSingleNode('RuleHintPaths')) {
        $assemblyPathElement = $doc.RuleSet.RuleHintPaths.SelectSingleNode("Path[text()[contains(.,'$ruleAssemblyName')]]")
        if ($assemblyPathElement) {
            $dummy = $doc.RuleSet.RuleHintPaths.RemoveChild($assemblyPathElement)
            Write-Host "Removed hint path to '$ruleAssemblyName' from .ruleset file."
        }

        # Remove empty parent element
        $ruleHintPathsElement = $doc.RuleSet.SelectSingleNode('RuleHintPaths')
        if (!$doc.RuleSet.RuleHintPaths -and $ruleHintPathsElement -ne $null) {
            $dummy = $doc.RuleSet.RemoveChild($ruleHintPathsElement)
            Write-Host "Removed empty container 'RuleHintPaths' from .ruleset file."
        }
    }
}

$global:ruleSetFileIsEmpty = -1
$global:ruleSetFileContainsData = -2

# Removes assembly path and rule IDs from a .ruleset file; returns: a constant or the-single-left-ruleset-path
function Cleanup-RuleSet-File ($ruleSetPath, $ruleAssemblyName) {
    # Load the .ruleset file
    $doc = New-Object xml
    $doc.Load($ruleSetPath)

    Remove-Rules-From-RuleSet $doc 'RNUL', 'RINUL'
    Remove-RuleAssembly-From-RuleSet $doc $ruleAssemblyName

    # Save the .ruleset file to disk
    $doc.Save($ruleSetPath)

    if (!$doc.SelectSingleNode('RuleSet') -or !$doc.RuleSet.HasChildNodes) {
        # .ruleset file contains no more rules
        return $global:ruleSetFileIsEmpty
    }
    elseif ($doc.RuleSet.SelectNodes('Include').Count -eq 1 -and $doc.RuleSet.SelectNodes('Rules').Count -eq 0) {
        # .ruleset file contains a single rule
        $singleIncludePath = $doc.RuleSet.SelectSingleNode('Include/@Path').Value
        return $singleIncludePath
    }
    else {
        # .ruleset file contains multiple rules
        return $global:ruleSetFileContainsData
    }
}

# Recursively searches for a file in project; needed when such a file may exist in a subfolder
function Find-File-In-Project-Item ($projectItem, $path) {
    if ($projectItem -and $projectItem.FileNames(1) -eq $path) {
        return $item
    }
    foreach ($nestedItem in $projectItem.ProjectItems) {
        $result = Find-File-In-Project-Item $nestedItem $path
        if ($result) {
            return $result
        }
    }
    return $null
}

# Removes file at the specified path from the project and from disk
function Remove-File-From-Project ($path, $project) {
    foreach ($item in $project.ProjectItems) {
        $result = Find-File-In-Project-Item $item $path
        if ($result) {
            Write-Host "Detaching file at '$path' from project."
            $item.Remove()
            break
        }
    }
    Remove-Item $path
}


$codeAnalysisRuleAssemblyFileName = "CodeContractNullabilityFxCopRules.dll"

$isCodeAnalysisEnabled = 0
$properties = $project.ConfigurationManager.ActiveConfiguration.Properties
if ($properties.Item('RunCodeAnalysis').Value -eq 'true') {
    $isCodeAnalysisEnabled = 1
}
Write-Debug "isCodeAnalysisEnabled = $isCodeAnalysisEnabled"

$ruleSetFileName = $properties.Item('CodeAnalysisRuleSet').Value
Write-Debug "ruleSetFileName = $ruleSetFileName"

$ruleSetPath = Split-Path $project.FullName -parent | Join-Path -ChildPath $ruleSetFileName
if ((Test-Path -Path $ruleSetPath)) {
    Write-Host ".ruleset file found at: '$ruleSetPath'"

    $cleanupResult = Cleanup-RuleSet-File $ruleSetPath $codeAnalysisRuleAssemblyFileName
    if ($cleanupResult -eq $global:ruleSetFileIsEmpty) {
        $properties.Item('CodeAnalysisRuleSet').Value = ''
        $properties.Item("RunCodeAnalysis").Value = 'False'
        Remove-File-From-Project $ruleSetPath $project
        Write-Host "Deactivated CodeAnalysis and removed empty .ruleset file."
    }
    elseif ($cleanupResult -eq $global:ruleSetFileContainsData) {
        $properties.Item("RunCodeAnalysis").Value = 'True'
        Write-Host "Leaving non-empty .ruleset file active."
    }
    else {
        Write-Debug "cleanupResult = $cleanupResult"
        $properties.Item('CodeAnalysisRuleSet').Value = $cleanupResult
        $properties.Item("RunCodeAnalysis").Value = 'True'
        Remove-File-From-Project $ruleSetPath $project
        Write-Host "Moved single include '$cleanupResult' from .ruleset file into project settings and removed .ruleset file."
    }
}
else {
    Write-Host ".ruleset file not found."
}
