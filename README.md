# Resharper Code Contract Nullability FxCop Rules

[![Build status](https://ci.appveyor.com/api/projects/status/pm2fkhbaip5fqrov?svg=true)](https://ci.appveyor.com/project/bkoelman/resharpercodecontractnullabilityfxcop) [![resharpercodecontractnullabilityfxcop MyGet Build Status](https://www.myget.org/BuildSource/Badge/resharpercodecontractnullabilityfxcop?identifier=abf5b05e-e912-4e89-aa6a-adc774d161c1)](https://www.myget.org/)

This Visual Studio analyzer supports you in consequently annotating your codebase with Resharpers nullability attributes. Doing so improves the [nullability analysis engine in Resharper](https://www.jetbrains.com/resharper/help/Code_Analysis__Code_Annotations.html), so `NullReferenceException`s at runtime will become something from the past.

## Get started

* You need [Visual Studio](https://www.visualstudio.com/) 2013 (or lower) and [Resharper](https://www.jetbrains.com/resharper/) v8 or higher to use this analyzer. See [here](https://github.com/bkoelman/ResharperCodeContractNullability) if you use Visual Studio 2015.

* Activate the solution configuration in which you want to install (Debug/Release)

* From the NuGet package manager console:

  `Install-Package ResharperCodeContractNullabilityFxCop`

  `Install-Package JetBrains.Annotations`

* Rebuild your solution

Instead of adding the JetBrains package, you can [put the annotation definitions directly in your source code](https://www.jetbrains.com/resharper/help/Code_Analysis__Annotations_in_Source_Code.html).

![Analyzer in action](https://github.com/bkoelman/ResharperCodeContractNullabilityFxCop/blob/gh-pages/images/fxcop-analyzer-in-action.png)

## Building, Testing, and Debugging

To build, open `ResharperCodeContractNullabilityFxCop.sln` in Visual Studio 2013. Press `Ctrl + A` to run all unit tests. See [How-to-debug.md](src/How-to-debug.md) for debugging instructions.

## Trying out the latest build

After each commit, a new prerelease NuGet package is automatically published to [MyGet](http://www.myget.org). To try it out, follow the next steps:

* In Visual Studio: **Tools**, **Options**, **NuGet Package Manager**, **Package Sources**
    * Click **+**
    * Name: **MyGet**, Source: **http://www.myget.org/F/resharpercodecontractnullabilityfxcop**
    * Click **Update**, **Ok**
* In Visual Studio, select **MyGet** as package source in the GUI or Package Manager Console.

## Running on your build server

This assumes your project uses ResharperCodeContractNullabilityFxCop, but Resharper is not installed on your build server. 
To make the analyzer run there, make sure to install External Annotations on the server as a pre-build step:

* ```nuget install JetBrains.ExternalAnnotations -Version 10.0.15```
* ```xcopy JetBrains.ExternalAnnotations.10.0.15\*.xml "%LOCALAPPDATA%\JetBrains\Installations\ReSharperPlatformVs14\ExternalAnnotations" /s /i /r /y /q```
