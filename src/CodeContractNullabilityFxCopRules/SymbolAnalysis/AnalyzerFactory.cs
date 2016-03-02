using System;
using CodeContractNullabilityFxCopRules.ExternalAnnotations;
using CodeContractNullabilityFxCopRules.SymbolAnalysis.Analyzers;
using CodeContractNullabilityFxCopRules.SymbolAnalysis.Symbols;
using CodeContractNullabilityFxCopRules.Utilities;
using JetBrains.Annotations;

namespace CodeContractNullabilityFxCopRules.SymbolAnalysis
{
    /// <summary>
    /// Constructs the matching nullability analyzer for a symbol.
    /// </summary>
    public class AnalyzerFactory : ISymbolVisitor<BaseAnalyzer>
    {
        [NotNull]
        private readonly IExternalAnnotationsResolver externalAnnotations;

        private readonly bool appliesToItem;

        public AnalyzerFactory([NotNull] IExternalAnnotationsResolver externalAnnotations, bool appliesToItem)
        {
            Guard.NotNull(externalAnnotations, "externalAnnotations");

            this.externalAnnotations = externalAnnotations;
            this.appliesToItem = appliesToItem;
        }

        [NotNull]
        public BaseAnalyzer CreateFor([NotNull] ISymbol symbol)
        {
            Guard.NotNull(symbol, "symbol");

            return symbol.Accept(this);
        }

        public BaseAnalyzer VisitField(FieldSymbol field)
        {
            return new FieldAnalyzer(field, externalAnnotations, appliesToItem);
        }

        public BaseAnalyzer VisitProperty(PropertySymbol property)
        {
            return new PropertyAnalyzer(property, externalAnnotations, appliesToItem);
        }

        public BaseAnalyzer VisitMethod(MethodSymbol method)
        {
            return new MethodReturnValueAnalyzer(method, externalAnnotations, appliesToItem);
        }

        public BaseAnalyzer VisitParameter(ParameterSymbol parameter)
        {
            return new ParameterAnalyzer(parameter, externalAnnotations, appliesToItem);
        }

        public BaseAnalyzer VisitType(TypeSymbol type)
        {
            throw new NotSupportedException();
        }
    }
}