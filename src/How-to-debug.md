Debugging within Visual Studio through FxCop engine
---------------------------------------------------

1] Right-click FxCopCmd in Solution Explorer -> Set as Startup Project

2] Right-click FxCopCmd in Solution Explorer -> Properties

3] On the General tab -> Executable, change path to fxcopcmd.exe as appropriate on your system

4] On the General tab -> Environment, click ... and change the value for "projectrootfolder" 
   to the folder where you put the sources and confirm with OK

5] Set a breakpoint in CodeContractNullabilityFxCopRules project and press F5

   -or-

   Right-click FxCopCmd in Solution Explorer -> Debug -> Step Into new instance

The debugger should break when it hits the breakpoint.

Note that you always need to build CodeContractNullabilityFxCopRules manually after making changes,
before you'll see your changes in the debugger.

If you're not able to debug using these steps, the following article may help:
http://blogs.msdn.com/b/codeanalysis/archive/2007/05/16/faq-how-do-i-debug-a-custom-rule.aspx


Running from within a second Visual Studio instance
---------------------------------------------------

1] Right-click CodeContractNullabilityFxCopRules in Solution Explorer -> Set as Startup Project

2] Right-click CodeContractNullabilityFxCopRules in Solution Explorer -> Properties, tab Debug:

    Start external program:
        c:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.exe

    Command line arguments:
        ..\..\..\..\RunTargetForVisualStudio\RunTargetForVisualStudio.sln

3] Press F5 to run

4] In the second Visual Studio instance, rebuild the solution. The Code Analysis window should now
   contain a list of the reported problems.

Note this does not attach the debugger, so your breakpoints will not get hit. But it helps to quickly
see the impact of your changes. Alternatively, you can easily debug from within a unit test.
