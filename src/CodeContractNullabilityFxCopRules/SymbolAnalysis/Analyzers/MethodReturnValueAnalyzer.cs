using CodeContractNullabilityFxCopRules.ExternalAnnotations.Storage;
using CodeContractNullabilityFxCopRules.SymbolAnalysis.Symbols;
using JetBrains.Annotations;

namespace CodeContractNullabilityFxCopRules.SymbolAnalysis.Analyzers
{
    /// <summary>
    /// Performs analysis of the return value of a method.
    /// </summary>
    public class MethodReturnValueAnalyzer : BaseAnalyzer<MethodSymbol>
    {
        public MethodReturnValueAnalyzer([NotNull] MethodSymbol symbol,
            [NotNull] ExternalAnnotationsMap externalAnnotations, bool appliesToItem)
            : base(symbol, externalAnnotations, appliesToItem)
        {
        }

        protected override bool RequiresAnnotation()
        {
            if (Symbol.IsCompilerControlled)
            {
                return false;
            }

            if (Symbol.IsPropertyOrEventAccessor)
            {
                return false;
            }

            if (Symbol.ContainingType != null && Symbol.ContainingType.IsOrDerivesFrom("System.Delegate"))
            {
                return false;
            }

            return base.RequiresAnnotation();
        }

        protected override bool HasAnnotationInBaseClass()
        {
            MethodSymbol baseMember = Symbol.OverriddenMethod;
            while (baseMember != null)
            {
                if (baseMember.HasNullabilityAnnotation(AppliesToItem) ||
                    ExternalAnnotations.Contains(baseMember, AppliesToItem) || HasAnnotationInInterface(baseMember))
                {
                    return true;
                }

                baseMember = baseMember.OverriddenMethod;
            }
            return false;
        }
    }
}