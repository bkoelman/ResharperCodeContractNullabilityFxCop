param($installPath, $toolsPath, $package, $project)

# Debugging tips
#---------------
# - Get-Project in the NuGet console returns the current project as a DTE object (the same as
#       the $project parameter that is passed to the NuGet scripts)
#
# - See comment in AfterBuild target of CodeContractNullabilityFxCopRules.csproj to bypass nupkg caching,
#       then run in a second VS instance on one of the projects in NuGetInstallTargets.sln:
#           install-package ResharperCodeContractNullabilityFxCop -source "..\..\ResharperCodeContractNullabilityFxCop\CodeContractNullabilityFxCopRules\bin\x86\Debug\"; uninstall-package ResharperCodeContractNullabilityFxCop
#
# - Use Write-Debug in this script to write messages to the NuGet console during install/uninstall
#
# - Consider using the Interactive Debugger at:
#       http://stackoverflow.com/questions/7031944/how-to-debug-install-ps1-script-of-nuget-package

Set-StrictMode -Version 2.0

# Set to nonzero to active debug messages
$global:enableDebug = 0

# Writes a line of text to the NuGet PowerShell console, if debugging is enabled
function Write-Debug ($text) {
    if ($global:enableDebug) {
        Write-Host "DEBUG: $text"
    }
}

# Transform an absolute path into a path that is relative to another folder
function Compute-RelativePath ($basePath, $referencePath) {
    $baseUri = New-Object -typename System.Uri -argumentlist $basePath
    $referenceUri = New-Object -typename System.Uri -argumentlist $referencePath

    $relativeUri = $baseUri.MakeRelativeUri($referenceUri)
    $relativePath = [System.Uri]::UnescapeDataString($relativeUri.ToString()).Replace('/', '\')
    return $relativePath
}

# Writes a default .ruleset file to disk
function Create-Empty-RuleSet-File ($path) {
    Write-Host "Creating .ruleset file at '$path'"
    Set-Content -Path $path -Value '<?xml version="1.0" encoding="utf-8"?>'
    Add-Content -Path $path -Value '<RuleSet Name="CodeAnalysis" ToolsVersion="12.0">'
    Add-Content -Path $path -Value '</RuleSet>'
}

# Appends a line like <Include Path="extendedcorrectnessrules.ruleset" Action="Default" /> to a .ruleset file
function Include-Rule-In-RuleSet-File ($path, $includePath) {
    $doc = New-Object xml
    $doc.Load($path)

    $includeElement = $doc.CreateElement('Include')
    $includeElement.SetAttribute('Path', $includePath)
    $includeElement.SetAttribute('Action', 'Default')
    $doc.RuleSet.InsertAfter($includeElement, $null)
    Write-Host "Added include for '$includePath' to .ruleset file"

    # Save the .ruleset file to disk
    $doc.Save($path)
}

# Adds lines like <Rule Id="XXXX" Action="Warning" /> to a .ruleset file, if not yet exist
function Ensure-RuleElements ($doc, [string[]] $ruleIds) {
    foreach ($ruleId in $ruleIds) {
        $ruleElement = $doc.RuleSet.Rules.SelectSingleNode("Rule[@Id='$ruleId']")
        if (!$ruleElement) {
            $ruleElement = $doc.CreateElement('Rule')
            $ruleElement.SetAttribute('Id', $ruleId)
            $ruleElement.SetAttribute('Action', 'Warning')
            $doc.RuleSet.Rules.AppendChild($ruleElement)
            Write-Host "Added rule '$ruleId' to .ruleset file"
        }
    }
}

function Append-Rules-To-RuleSet-File ($path, $assemblyPath) {
    # Load the .ruleset file
    $doc = New-Object xml
    $doc.Load($path)

    # Put relative path to rule assembly in .ruleset file
    $ruleHintPathsElement = $doc.RuleSet.SelectSingleNode('RuleHintPaths')
    if (!$ruleHintPathsElement) {
        $ruleHintPathsElement = $doc.CreateElement('RuleHintPaths')
        $doc.RuleSet.InsertAfter($ruleHintPathsElement, $null)
        Write-Host "Added container 'RuleHintPaths' to .ruleset file."
    }

    $assemblyName = Split-Path $assemblyPath -Leaf
    $pathElement = $ruleHintPathsElement.SelectSingleNode("Path[text()[contains(.,'$assemblyName')]]")
    if (!$pathElement) {
        $pathElement = $doc.CreateElement('Path')
        $pathElement.AppendChild($doc.CreateTextNode($assemblyPath))
        $ruleHintPathsElement.AppendChild($pathElement)
        Write-Host "Added hint path for '$assemblyPath' to .ruleset file."
    }
    else {
        Write-Host "Updated existing hint path to '$assemblyPath' in .ruleset file."
        $pathElement.InnerText = $assemblyPath
    }

    if (!$doc.RuleSet.SelectSingleNode('Rules')) {
        $rulesElement = $doc.CreateElement('Rules')
        $rulesElement.SetAttribute('AnalyzerId', 'Microsoft.Analyzers.ManagedCodeAnalysis')
        $rulesElement.SetAttribute('RuleNamespace', 'Microsoft.Rules.Managed')
        $doc.RuleSet.AppendChild($rulesElement)
        Write-Host "Added container 'Rules' to .ruleset file."
    }

    # add the rules
    Ensure-RuleElements $doc 'RNUL', 'RINUL'

    # Save the .ruleset file to disk
    $doc.Save($path)
}

$codeAnalysisRuleAssemblyFileName = "CodeContractNullabilityFxCopRules.dll"

# remove placeholder file; a content file must exist in nuget for install.ps1 to run
$placeholder = 'CodeAnalysisRulesPlaceHolder.txt'
$project.ProjectItems.Item($placeholder).Remove()
Split-Path $project.FullName -parent | Join-Path -ChildPath $placeholder | Remove-Item

$isCodeAnalysisEnabled = 0
$properties = $project.ConfigurationManager.ActiveConfiguration.Properties
if ($properties.Item('RunCodeAnalysis').Value -eq 'true') {
    $isCodeAnalysisEnabled = 1
}
Write-Debug "isCodeAnalysisEnabled = $isCodeAnalysisEnabled"

$ruleSetFileName = $properties.Item('CodeAnalysisRuleSet').Value
if (!$ruleSetFileName) {
    # No specific ruleset was selected, so fallback to default. The ruleset name we pick here
    # is correct only for non-C++ projects in non-Express editions.
    # Microsoft.CodeAnalysis.Targets (in VS install directory) contains how default is determined.
    $ruleSetFileName = 'MinimumRecommendedRules.ruleset'
}
Write-Debug "ruleSetFileName = $ruleSetFileName"

# Ensure that the project contains a .ruleset file
$ruleSetPath = Split-Path $project.FullName -parent | Join-Path -ChildPath $ruleSetFileName
if (!(Test-Path -Path $ruleSetPath)) {
    Write-Debug ".ruleset file not found at: '$ruleSetPath'"

    $existingRuleSetName = $null
    if ($isCodeAnalysisEnabled) {
        $existingRuleSetName = $ruleSetFileName
    }

    $ruleSetFileName = 'CodeAnalysis.ruleset'
    $ruleSetPath = Split-Path $project.FullName -parent | Join-Path -ChildPath $ruleSetFileName

    Create-Empty-RuleSet-File $ruleSetPath

    Write-Host "Attaching file at '$ruleSetPath' to project."
    $project.ProjectItems.AddFromFile($ruleSetPath)

    if ($existingRuleSetName) {
        Write-Debug "existingRuleSetName = $existingRuleSetName"
        Include-Rule-In-RuleSet-File $ruleSetPath $existingRuleSetName
    }
}
else {
    Write-Host "Reusing existing .ruleset file at '$ruleSetPath'."
}
Write-Debug "ruleSetPath = $ruleSetPath"
Write-Debug "toolsPath = $toolsPath"

# Compute the rule assembly path, relative to the tools folder
$ruleAssemblyFolder = Compute-RelativePath $ruleSetPath $toolsPath
Write-Debug "ruleAssemblyFolder = $ruleAssemblyFolder"
$ruleAssemblyPath = [System.IO.Path]::Combine($ruleAssemblyFolder, $codeAnalysisRuleAssemblyFileName)
Write-Debug "ruleAssemblyPath = $ruleAssemblyPath"

# Add our rules to the .ruleset file (if not already there)
Append-Rules-To-RuleSet-File $ruleSetPath $ruleAssemblyPath

# Enable code analysis and activate .ruleset file
$properties.Item("RunCodeAnalysis").Value = 'True'
$properties.Item('CodeAnalysisRuleSet').Value = $ruleSetFileName
Write-Host "Activated CodeAnalysis and activated .ruleset file '$ruleSetFileName'."