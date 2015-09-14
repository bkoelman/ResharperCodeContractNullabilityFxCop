using System;
using CodeContractNullabilityFxCopRules.ExternalAnnotations.Storage;
using CodeContractNullabilityFxCopRules.SymbolAnalysis.Symbols;
using JetBrains.Annotations;

namespace CodeContractNullabilityFxCopRules.SymbolAnalysis.Analyzers
{
    /// <summary>
    /// Performs analysis of a field.
    /// </summary>
    public class FieldAnalyzer : BaseAnalyzer<FieldSymbol>
    {
        [NotNull]
        private readonly GeneratedCodeHeuristics heuristics;

        public FieldAnalyzer([NotNull] FieldSymbol field, [NotNull] ExternalAnnotationsMap externalAnnotations,
            bool appliesToItem)
            : base(field, externalAnnotations, appliesToItem)
        {
            heuristics = new GeneratedCodeHeuristics(field);
        }

        private bool IsEventField
        {
            get
            {
                return Symbol.Type.FullName == "System.EventHandler" ||
                    Symbol.Type.FullName.StartsWith("System.EventHandler`1<", StringComparison.Ordinal);
            }
        }

        protected override bool RequiresAnnotation()
        {
            if (Symbol.IsConstant)
            {
                return false;
            }

            if (IsEventField)
            {
                return false;
            }

            if (heuristics.IsWindowsFormsDesignerGenerated())
            {
                return false;
            }

            return true;
        }
    }
}