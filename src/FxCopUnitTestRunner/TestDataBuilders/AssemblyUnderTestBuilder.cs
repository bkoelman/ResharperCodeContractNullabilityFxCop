using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;

namespace FxCopUnitTestRunner.TestDataBuilders
{
    public abstract class AssemblyUnderTestBuilder : ITestDataBuilder<AssemblyUnderTest>
    {
        [NotNull]
        [ItemNotNull]
        private readonly HashSet<string> namespaces = new HashSet<string> { "System" };

        [NotNull]
        [ItemNotNull]
        private readonly HashSet<Assembly> assemblyReferences = new HashSet<Assembly>
        {
            // mscorlib.dll
            typeof (object).Assembly,

            // System.dll
            typeof (Component).Assembly
        };

        [NotNull]
        protected abstract string GetCompleteSourceText();

        public AssemblyUnderTest Build()
        {
            string sourceCode = GetCompleteSourceCode();
            return new AssemblyUnderTest(sourceCode, assemblyReferences);
        }

        [NotNull]
        private string GetCompleteSourceCode()
        {
            var sourceBuilder = new StringBuilder();
            foreach (string ns in namespaces)
            {
                sourceBuilder.AppendLine(string.Format("using {0};", ns));
            }
            sourceBuilder.AppendLine();
            sourceBuilder.Append(GetCompleteSourceText());
            return sourceBuilder.ToString();
        }

        internal void _Using([NotNull] string codeNamespace)
        {
            namespaces.Add(codeNamespace);
        }

        internal void _WithReference([NotNull] Assembly assembly)
        {
            assemblyReferences.Add(assembly);
        }
    }

    public static class AssemblyUnderTestBuilderExtensions
    {
        [NotNull]
        public static TBuilder Using<TBuilder>([NotNull] this TBuilder source, [CanBeNull] string codeNamespace)
            where TBuilder : AssemblyUnderTestBuilder
        {
            Guard.NotNull(source, "source");

            if (!string.IsNullOrWhiteSpace(codeNamespace))
            {
                source._Using(codeNamespace);
            }
            return source;
        }

        [NotNull]
        public static TBuilder WithReference<TBuilder>([NotNull] this TBuilder source, [NotNull] Assembly assembly)
            where TBuilder : AssemblyUnderTestBuilder
        {
            Guard.NotNull(source, "source");
            Guard.NotNull(assembly, "assembly");

            source._WithReference(assembly);
            return source;
        }
    }
}