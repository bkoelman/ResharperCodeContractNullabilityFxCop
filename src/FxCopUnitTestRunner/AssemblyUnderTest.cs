using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.FxCop.Sdk;
using NUnit.Framework;

namespace FxCopUnitTestRunner
{
    public class AssemblyUnderTest
    {
        [NotNull]
        private readonly string sourceCode;

        [NotNull]
        [ItemNotNull]
        private readonly ISet<Assembly> references;

        [NotNull]
        private readonly string outputAssemblyFileName = Path.GetFileName(Path.GetTempFileName()) + ".dll";

        internal AssemblyUnderTest([NotNull] string sourceCode, [NotNull] [ItemNotNull] ISet<Assembly> references)
        {
            Guard.NotNull(sourceCode, "sourceCode");
            Guard.NotNull(references, "references");

            this.sourceCode = sourceCode;
            this.references = references;
        }

        [NotNull]
        public AssemblyNode GetAssemblyNode()
        {
            CompileSourceCode();
            return AssemblyNode.GetAssembly(outputAssemblyFileName, true, false, true);
        }

        private void CompileSourceCode()
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(sourceCode, new CSharpParseOptions(LanguageVersion.CSharp5));
            IEnumerable<MetadataReference> assemblyReferences =
                references.Select(x => MetadataReference.CreateFromFile(x.Location));

            CSharpCompilation compilation = CSharpCompilation.Create(outputAssemblyFileName,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary), syntaxTrees: new[] { tree },
                references: assemblyReferences);

            ImmutableArray<Diagnostic> compilerDiagnostics = compilation.GetDiagnostics(CancellationToken.None);
            ValidateCompilerDiagnostics(compilerDiagnostics);

            using (FileStream assemblyStream = File.Create(outputAssemblyFileName))
            {
                EmitResult compileResult = compilation.Emit(assemblyStream);

                Assert.True(compileResult.Success,
                    string.Format("Test Assembly Generation failed due to: {0}\r\n\r\nin:\r\n{1}",
                        string.Join("\r\n", compileResult.Diagnostics), sourceCode));
            }
        }

        private void ValidateCompilerDiagnostics([ItemNotNull] ImmutableArray<Diagnostic> compilerDiagnostics)
        {
            bool hasErrors = compilerDiagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
            Assert.IsFalse(hasErrors);
        }
    }
}