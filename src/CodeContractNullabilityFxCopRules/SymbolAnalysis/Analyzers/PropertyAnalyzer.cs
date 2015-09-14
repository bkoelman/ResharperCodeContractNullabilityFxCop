using CodeContractNullabilityFxCopRules.ExternalAnnotations.Storage;
using CodeContractNullabilityFxCopRules.SymbolAnalysis.Symbols;
using JetBrains.Annotations;

namespace CodeContractNullabilityFxCopRules.SymbolAnalysis.Analyzers
{
    /// <summary>
    /// Performs analysis of a property.
    /// </summary>
    public class PropertyAnalyzer : BaseAnalyzer<PropertySymbol>
    {
        public PropertyAnalyzer([NotNull] PropertySymbol symbol, [NotNull] ExternalAnnotationsMap externalAnnotations,
            bool appliesToItem)
            : base(symbol, externalAnnotations, appliesToItem)
        {
        }

        protected override bool RequiresAnnotation()
        {
            if (Symbol.ContainingType != null && Symbol.ContainingType.HasCompilerGeneratedAnnotation)
            {
                return false;
            }

            return base.RequiresAnnotation();
        }

        protected override bool HasAnnotationInBaseClass()
        {
            PropertySymbol baseMember = Symbol.OverriddenProperty;
            while (baseMember != null)
            {
                if (baseMember.HasNullabilityAnnotation(AppliesToItem) ||
                    ExternalAnnotations.Contains(baseMember, AppliesToItem) || HasAnnotationInInterface(baseMember))
                {
                    return true;
                }

                baseMember = baseMember.OverriddenProperty;
            }
            return false;
        }
    }
}