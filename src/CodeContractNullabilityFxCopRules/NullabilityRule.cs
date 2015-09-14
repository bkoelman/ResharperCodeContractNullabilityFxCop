using JetBrains.Annotations;
using Microsoft.FxCop.Sdk;

namespace CodeContractNullabilityFxCopRules
{
    /// <summary>
    /// FxCop rule that reports NotNull/CanBeNull should be applied if appropriate for the incoming symbol.
    /// </summary>
    public class NullabilityRule : CodeContractBaseRule
    {
        public const string RuleName = "Decorate";

        public NullabilityRule()
            : base(typeof (NullabilityRule).Name, RuleName, false)
        {
        }

        [NotNull]
        public override ProblemCollection Check([NotNull] Member member)
        {
            return CheckMember(member);
        }

        [NotNull]
        public override ProblemCollection Check([NotNull] Parameter parameter)
        {
            return CheckParameter(parameter);
        }
    }
}