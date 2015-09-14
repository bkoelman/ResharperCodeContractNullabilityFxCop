# Resharper Code Contract Nullability FxCop Rules

This Visual Studio analyzer supports you in consequently annotating your codebase with Resharpers nullability attributes. Doing so improves the [nullability analysis engine in Resharper](https://www.jetbrains.com/resharper/help/Code_Analysis__Code_Annotations.html), so `NullReferenceException`s at runtime will become something from the past.

## Get started

* You need [Visual Studio](https://www.visualstudio.com/) 2013 (or lower) and [Resharper](https://www.jetbrains.com/resharper/) v8 or v9 to use this analyzer. See [here](https://github.com/bkoelman/ResharperCodeContractNullability) if you use Visual Studio 2015.

* From the NuGet package manager console:

  `Install-Package ResharperCodeContractNullabilityFxCop`

  `Install-Package JetBrains.Annotations`

* Rebuild your solution

Instead of adding the JetBrains package, you can [put the annotation definitions directly in your source code](https://www.jetbrains.com/resharper/help/Code_Analysis__Annotations_in_Source_Code.html). In that case, it's recommended to set the `conditional` option checked.

![Analyzer in action](https://github.com/bkoelman/ResharperCodeContractNullability/blob/gh-pages/images/analyzer-in-action.png)
