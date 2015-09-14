using CodeContractNullabilityFxCopRules.Utilities;
using JetBrains.Annotations;
using Microsoft.FxCop.Sdk;

namespace CodeContractNullabilityFxCopRules
{
    /// <summary>
    /// FxCop rule that reports ItemNotNull/ItemCanBeNull should be applied if appropriate for the incoming symbol.
    /// </summary>
    public class ItemNullabilityRule : CodeContractBaseRule
    {
        public const string RuleName = "ItemDecorate";

        public ItemNullabilityRule()
            : base(typeof (ItemNullabilityRule).Name, RuleName, true)
        {
        }

        [NotNull]
        public override ProblemCollection Check([NotNull] Member member)
        {
            Guard.NotNull(member, "member");

            return CheckMember(member);
        }

        [NotNull]
        public override ProblemCollection Check([NotNull] Parameter parameter)
        {
            Guard.NotNull(parameter, "parameter");

            return CheckParameter(parameter);
        }
    }
}