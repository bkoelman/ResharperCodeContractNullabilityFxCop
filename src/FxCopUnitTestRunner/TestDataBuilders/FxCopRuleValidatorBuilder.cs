using System;
using JetBrains.Annotations;
using Microsoft.FxCop.Sdk;

namespace FxCopUnitTestRunner.TestDataBuilders
{
    public abstract class FxCopRuleValidatorBuilder : ITestDataBuilder<FxCopRuleValidator>
    {
        [NotNull]
        private AssemblyUnderTestBuilder assemblyUnderTestBuilder = EmptySourceCodeBuilder.Default;

        [CanBeNull]
        protected BaseIntrospectionRule Rule { get; private set; }

        public virtual FxCopRuleValidator Build()
        {
            if (Rule == null)
            {
                throw new InvalidOperationException("Set rule first.");
            }

            AssemblyUnderTest assembly = assemblyUnderTestBuilder.Build();
            return new FxCopRuleValidator(Rule, assembly);
        }

        protected void ForRule<TRule>() where TRule : BaseIntrospectionRule, new()
        {
            Rule = new TRule();
        }

        protected void OnAssembly([NotNull] AssemblyUnderTestBuilder assemblyBuilder)
        {
            Guard.NotNull(assemblyBuilder, "assemblyBuilder");

            assemblyUnderTestBuilder = assemblyBuilder;
        }

        private sealed class EmptySourceCodeBuilder : AssemblyUnderTestBuilder
        {
            [NotNull]
            public static readonly AssemblyUnderTestBuilder Default = new EmptySourceCodeBuilder();

            protected override string GetCompleteSourceText()
            {
                return string.Empty;
            }
        }
    }
}