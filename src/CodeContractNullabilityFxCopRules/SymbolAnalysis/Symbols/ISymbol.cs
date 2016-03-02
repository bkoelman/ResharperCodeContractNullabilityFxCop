using JetBrains.Annotations;
using Microsoft.FxCop.Sdk;

namespace CodeContractNullabilityFxCopRules.SymbolAnalysis.Symbols
{
    /// <summary>
    /// An abstraction over FxCop's <see cref="NodeType" /> model, which is more similar to the Microsoft.CodeAnalysis (Roslyn)
    /// symbol model.
    /// </summary>
    public interface ISymbol
    {
        [NotNull]
        string Name { get; }

        [NotNull]
        TypeSymbol Type { get; }

        [CanBeNull]
        TypeSymbol ContainingType { get; }

        [CanBeNull]
        string ContainingAssemblyPath { get; }

        bool HasCompilerGeneratedAnnotation { get; }
        bool HasDebuggerNonUserCodeAnnotation { get; }

        bool HasNullabilityAnnotation(bool appliesToItem);

        [NotNull]
        string GetDocumentationCommentId();

        [NotNull]
        T Accept<T>([NotNull] ISymbolVisitor<T> visitor);
    }
}