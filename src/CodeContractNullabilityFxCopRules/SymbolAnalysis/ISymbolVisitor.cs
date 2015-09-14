using CodeContractNullabilityFxCopRules.SymbolAnalysis.Symbols;
using JetBrains.Annotations;

namespace CodeContractNullabilityFxCopRules.SymbolAnalysis
{
    /// <summary>
    /// Defines the contract for implementing the Visitor design pattern on the <see cref="ISymbol" /> hierachy.
    /// </summary>
    /// <typeparam name="TResult">
    /// The type of the return value for Visit methods.
    /// </typeparam>
    public interface ISymbolVisitor<out TResult>
    {
        [NotNull]
        TResult VisitField([NotNull] FieldSymbol field);

        [NotNull]
        TResult VisitProperty([NotNull] PropertySymbol property);

        [NotNull]
        TResult VisitMethod([NotNull] MethodSymbol method);

        [NotNull]
        TResult VisitParameter([NotNull] ParameterSymbol parameter);

        [NotNull]
        TResult VisitType([NotNull] TypeSymbol type);
    }
}