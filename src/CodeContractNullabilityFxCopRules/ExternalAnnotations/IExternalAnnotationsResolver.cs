using CodeContractNullabilityFxCopRules.SymbolAnalysis.Symbols;
using JetBrains.Annotations;

namespace CodeContractNullabilityFxCopRules.ExternalAnnotations
{
    /// <summary>
    /// Determines whether an external nullability annotation exists for a symbol.
    /// </summary>
    public interface IExternalAnnotationsResolver
    {
        void EnsureScanned();

        bool HasAnnotationForSymbol([NotNull] ISymbol symbol, bool appliesToItem);
    }
}