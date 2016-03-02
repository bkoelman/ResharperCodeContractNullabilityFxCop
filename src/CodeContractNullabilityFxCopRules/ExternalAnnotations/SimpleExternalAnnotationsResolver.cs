using CodeContractNullabilityFxCopRules.ExternalAnnotations.Storage;
using CodeContractNullabilityFxCopRules.SymbolAnalysis.Symbols;
using CodeContractNullabilityFxCopRules.Utilities;
using JetBrains.Annotations;

namespace CodeContractNullabilityFxCopRules.ExternalAnnotations
{
    /// <summary>
    /// Provides a simple wrapper for an existing <see cref="ExternalAnnotationsMap" />.
    /// </summary>
    public sealed class SimpleExternalAnnotationsResolver : IExternalAnnotationsResolver
    {
        [NotNull]
        private readonly ExternalAnnotationsMap source;

        public SimpleExternalAnnotationsResolver([NotNull] ExternalAnnotationsMap source)
        {
            Guard.NotNull(source, "source");

            this.source = source;
        }

        public void EnsureScanned()
        {
        }

        public bool HasAnnotationForSymbol(ISymbol symbol, bool appliesToItem)
        {
            Guard.NotNull(symbol, "symbol");

            return source.Contains(symbol, appliesToItem);
        }
    }
}