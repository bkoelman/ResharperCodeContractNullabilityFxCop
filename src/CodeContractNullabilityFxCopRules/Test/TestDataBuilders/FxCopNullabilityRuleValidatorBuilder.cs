using CodeContractNullabilityFxCopRules.ExternalAnnotations;
using FxCopUnitTestRunner;
using FxCopUnitTestRunner.TestDataBuilders;
using JetBrains.Annotations;

namespace CodeContractNullabilityFxCopRules.Test.TestDataBuilders
{
    public class FxCopNullabilityRuleValidatorBuilder : FxCopRuleValidatorBuilder
    {
        [NotNull]
        private IExternalAnnotationsResolver externalAnnotationsResolver =
            new SimpleExternalAnnotationsResolver(new ExternalAnnotationsBuilder().Build());

        public override FxCopRuleValidator Build()
        {
            InjectExternalAnnotations();

            return base.Build();
        }

        private void InjectExternalAnnotations()
        {
            var nullabilityRule = (CodeContractBaseRule) Rule;
            if (nullabilityRule != null)
            {
                nullabilityRule.ExternalAnnotationsResolver.Override(externalAnnotationsResolver);
            }
        }

        [NotNull]
        public FxCopNullabilityRuleValidatorBuilder ExternallyAnnotated([NotNull] ExternalAnnotationsBuilder builder)
        {
            Guard.NotNull(builder, "builder");

            externalAnnotationsResolver = new SimpleExternalAnnotationsResolver(builder.Build());
            return this;
        }

        [NotNull]
        public new FxCopNullabilityRuleValidatorBuilder ForRule<TRule>() where TRule : CodeContractBaseRule, new()
        {
            base.ForRule<TRule>();
            return this;
        }

        [NotNull]
        public new FxCopNullabilityRuleValidatorBuilder OnAssembly([NotNull] AssemblyUnderTestBuilder assemblyBuilder)
        {
            base.OnAssembly(assemblyBuilder);
            return this;
        }
    }
}