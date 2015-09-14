using JetBrains.Annotations;
using Microsoft.FxCop.Sdk;

namespace CodeContractNullabilityFxCopRules.SymbolAnalysis.Symbols
{
    /// <summary>
    /// Represents a field in a class, struct or enum.
    /// </summary>
    public sealed class FieldSymbol : MemberSymbol
    {
        [NotNull]
        private readonly Field fxCopField;

        public override TypeSymbol Type
        {
            get
            {
                return new TypeSymbol(fxCopField.Type);
            }
        }

        public bool IsConstant
        {
            get
            {
                return fxCopField.IsLiteral && fxCopField.IsStatic && fxCopField.Flags.HasFlag(FieldFlags.HasDefault);
            }
        }

        public FieldSymbol([NotNull] Field fxCopField)
            : base(fxCopField)
        {
            this.fxCopField = fxCopField;
        }

        public override bool IsImplementationForInterfaceMember(MemberSymbol interfaceMember)
        {
            return false;
        }

        public override string GetDocumentationCommentId()
        {
            return DocumentationCommentFactory.GetDocumentationCommentId(fxCopField);
        }

        public override T Accept<T>(ISymbolVisitor<T> visitor)
        {
            return visitor.VisitField(this);
        }
    }
}