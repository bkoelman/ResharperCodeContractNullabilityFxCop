using System.Linq;
using CodeContractNullabilityFxCopRules.SymbolAnalysis.Symbols;
using CodeContractNullabilityFxCopRules.Utilities;
using JetBrains.Annotations;
using Microsoft.FxCop.Sdk;

namespace CodeContractNullabilityFxCopRules.SymbolAnalysis
{
    /// <summary>
    /// Gets the underlying type for the symbol being analyzed.
    /// </summary>
    public static class SymbolTypeResolver
    {
        [CanBeNull]
        public static TypeSymbol TryGetEffectiveType([NotNull] ISymbol symbol, bool appliesToItem)
        {
            Guard.NotNull(symbol, "symbol");

            if (!appliesToItem)
            {
                return symbol.Type;
            }

            if (symbol.Type.FullName == "System.Void")
            {
                return null;
            }

            return TryGetItemTypeForSequenceOrCollection(symbol.Type) ?? TryGetItemTypeForLazyOrGenericTask(symbol.Type);
        }

        [CanBeNull]
        private static TypeSymbol TryGetItemTypeForSequenceOrCollection([NotNull] TypeSymbol type)
        {
            Guard.NotNull(type, "type");

            foreach (TypeSymbol iface in type.Interfaces.PrependIfNotNull(type))
            {
                if (iface.UnboundGenericType != null &&
                    iface.UnboundGenericType.FullName == "System.Collections.Generic.IEnumerable`1")
                {
                    return iface.TypeArguments.Single();
                }
                if (iface.FullName == "System.Collections.IEnumerable")
                {
                    return new TypeSymbol(FrameworkTypes.Object);
                }
            }

            return null;
        }

        [CanBeNull]
        private static TypeSymbol TryGetItemTypeForLazyOrGenericTask([NotNull] TypeSymbol type)
        {
            Guard.NotNull(type, "type");

            if (type.UnboundGenericType != null)
            {
                if (type.UnboundGenericType.FullName == "System.Lazy`1" ||
                    type.UnboundGenericType.FullName == "System.Threading.Tasks.Task`1")
                {
                    return type.TypeArguments.Single();
                }
            }

            return null;
        }
    }
}