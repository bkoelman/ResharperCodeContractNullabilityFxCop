using JetBrains.Annotations;
using Microsoft.FxCop.Sdk;

namespace FxCopUnitTestRunner
{
    public class FxCopRuleValidator
    {
        [NotNull]
        private readonly BaseIntrospectionRule rule;

        [NotNull]
        private readonly AssemblyUnderTest assembly;

        public FxCopRuleValidator([NotNull] BaseIntrospectionRule rule, [NotNull] AssemblyUnderTest assembly)
        {
            Guard.NotNull(rule, "rule");
            Guard.NotNull(assembly, "assembly");

            this.rule = rule;
            this.assembly = assembly;
        }

        [NotNull]
        public FxCopRuleValidationResult Execute()
        {
            var runner = new FxCopRunner(rule, assembly);
            runner.Run();

            return new FxCopRuleValidationResult(rule.Problems);
        }
    }
}