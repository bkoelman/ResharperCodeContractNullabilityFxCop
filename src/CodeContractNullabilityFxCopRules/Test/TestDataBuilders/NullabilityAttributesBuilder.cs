using CodeContractNullabilityFxCopRules.Utilities;
using FxCopUnitTestRunner.TestDataBuilders;
using JetBrains.Annotations;

namespace CodeContractNullabilityFxCopRules.Test.TestDataBuilders
{
    public class NullabilityAttributesBuilder : ITestDataBuilder<NullabilityAttributesDefinition>
    {
        [NotNull]
        private string codeNamespace = "Namespace.For.JetBrains.Annotation.Attributes";

        public NullabilityAttributesDefinition Build()
        {
            return new NullabilityAttributesDefinition(codeNamespace);
        }

        [NotNull]
        public NullabilityAttributesBuilder InCodeNamespace([NotNull] string ns)
        {
            Guard.NotNull(ns, "ns");

            codeNamespace = ns;
            return this;
        }
    }
}