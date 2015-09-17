using System.Collections.Generic;
using CodeContractNullabilityFxCopRules.ExternalAnnotations;
using CodeContractNullabilityFxCopRules.ExternalAnnotations.Storage;
using CodeContractNullabilityFxCopRules.SymbolAnalysis;
using CodeContractNullabilityFxCopRules.SymbolAnalysis.Analyzers;
using CodeContractNullabilityFxCopRules.SymbolAnalysis.Symbols;
using CodeContractNullabilityFxCopRules.Utilities;
using JetBrains.Annotations;
using Microsoft.FxCop.Sdk;

namespace CodeContractNullabilityFxCopRules
{
    /// <summary>
    /// Base FcXop rule implementation for nullability analysis.
    /// </summary>
    public abstract class CodeContractBaseRule : BaseIntrospectionRule
    {
        [NotNull]
        private readonly string ruleName;

        private readonly bool appliesToItem;

        [NotNull]
        [ItemNotNull]
        private readonly HashSet<string> problemSymbols = new HashSet<string>();

        [NotNull]
        private readonly SymbolFactory symbolFactory = new SymbolFactory();

        [NotNull]
        public ExtensionPoint<ExternalAnnotationsMap> ExternalAnnotationsRegistry { get; private set; }

        protected CodeContractBaseRule([NotNull] string name, [NotNull] string ruleName, bool appliesToItem)
            : base(
                name, typeof (CodeContractBaseRule).Assembly.GetName().Name + ".RuleMetadata",
                typeof (CodeContractBaseRule).Assembly)
        {
            Guard.NotNull(ruleName, "ruleName");

            this.ruleName = ruleName;
            this.appliesToItem = appliesToItem;

            ExternalAnnotationsRegistry =
                new ExtensionPoint<ExternalAnnotationsMap>(DiskExternalAnnotationsLoader.Create);
        }

        [NotNull]
        protected ProblemCollection CheckMember([NotNull] Member member)
        {
            ISymbol symbol = symbolFactory.CreateOrNull(member);
            return CheckSymbol(symbol);
        }

        [NotNull]
        protected ProblemCollection CheckParameter([NotNull] Parameter parameter)
        {
            var symbol = new ParameterSymbol(parameter);
            return CheckSymbol(symbol);
        }

        [NotNull]
        private ProblemCollection CheckSymbol([CanBeNull] ISymbol symbol)
        {
            if (symbol != null)
            {
                // When unable to load external annotations, the rule would likely report lots 
                // of false positives. This is prevented by letting it throw here and report nothing.
                var analyzerFactory = new AnalyzerFactory(ExternalAnnotationsRegistry.GetCached(), appliesToItem);

                BaseAnalyzer analyzer = analyzerFactory.CreateFor(symbol);
                analyzer.Analyze(ReportProblem);
            }

            return Problems;
        }

        private void ReportProblem([NotNull] ISymbol symbol, [CanBeNull] string uniqueKeyToReportSymbol)
        {
            // The string in the .xml <Resolution> should contain one string arg, {0}
            Resolution resolution = GetNamedResolution(ruleName, symbol.Name);

            // Because FxCop does not track duplicates, we must do so ourselves.
            if (!SymbolAlreadyReported(symbol, uniqueKeyToReportSymbol))
            {
                var problem = new Problem(resolution, CheckId);
                Problems.Add(problem);
            }
        }

        private bool SymbolAlreadyReported([NotNull] ISymbol symbol, [CanBeNull] string uniqueKeyToReportSymbol)
        {
            string symbolId = uniqueKeyToReportSymbol ?? GetIdentifierForSymbol(symbol);
            if (problemSymbols.Contains(symbolId))
            {
                return true;
            }

            problemSymbols.Add(symbolId);
            return false;
        }

        [NotNull]
        private string GetIdentifierForSymbol([NotNull] ISymbol symbol)
        {
            var parameter = symbol as ParameterSymbol;
            if (parameter != null)
            {
                return parameter.ContainingMethod.GetDocumentationCommentId() + ":" + parameter.Name;
            }
            return symbol.GetDocumentationCommentId();
        }
    }
}