using System.Text;
using CodeContractNullabilityFxCopRules.Utilities;
using FxCopUnitTestRunner.TestDataBuilders;
using JetBrains.Annotations;

namespace CodeContractNullabilityFxCopRules.Test.TestDataBuilders
{
    public abstract class SourceCodeBuilder : AssemblyUnderTestBuilder
    {
        [NotNull]
        private NullabilityAttributesDefinition nullabilityAttributes = new NullabilityAttributesDefinition(string.Empty);

        [NotNull]
        protected abstract string GetSourceCode();

        protected override string GetCompleteSourceText()
        {
            var sourceBuilder = new StringBuilder();

            string sourceCode = GetSourceCode();
            sourceBuilder.Append(sourceCode);

            sourceBuilder.Append(nullabilityAttributes.SourceText);

            return sourceBuilder.ToString();
        }

        internal void _WithNullabilityAttributes([NotNull] NullabilityAttributesBuilder builder)
        {
            nullabilityAttributes = builder.Build();
        }
    }

    public static class SourceCodeBuilderExtensions
    {
        [NotNull]
        public static TBuilder WithNullabilityAttributes<TBuilder>([NotNull] this TBuilder source,
            [NotNull] NullabilityAttributesBuilder builder) where TBuilder : SourceCodeBuilder
        {
            Guard.NotNull(source, "source");
            Guard.NotNull(builder, "builder");

            source._WithNullabilityAttributes(builder);
            return source;
        }
    }
}