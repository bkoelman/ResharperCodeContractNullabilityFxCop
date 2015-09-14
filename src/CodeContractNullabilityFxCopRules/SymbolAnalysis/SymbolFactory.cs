using CodeContractNullabilityFxCopRules.SymbolAnalysis.Symbols;
using CodeContractNullabilityFxCopRules.Utilities;
using JetBrains.Annotations;
using Microsoft.FxCop.Sdk;

namespace CodeContractNullabilityFxCopRules.SymbolAnalysis
{
    /// <summary>
    /// Constructs <see cref="ISymbol" /> objects from FxCop <see cref="NodeType" /> objects.
    /// </summary>
    public class SymbolFactory
    {
        [CanBeNull]
        public MemberSymbol CreateOrNull([NotNull] Member member)
        {
            Guard.NotNull(member, "member");

            var field = member as Field;
            if (field != null)
            {
                return new FieldSymbol(field);
            }

            var property = member as PropertyNode;
            if (property != null)
            {
                return new PropertySymbol(property);
            }

            var method = member as Method;
            if (method != null)
            {
                return new MethodSymbol(method);
            }

            return null;
        }
    }
}