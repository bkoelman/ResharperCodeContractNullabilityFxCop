using System.Linq;
using CodeContractNullabilityFxCopRules.Utilities;
using JetBrains.Annotations;
using Microsoft.FxCop.Sdk;

namespace CodeContractNullabilityFxCopRules.SymbolAnalysis.Symbols
{
    /// <summary>
    /// Base definition for the member of a type.
    /// </summary>
    public abstract class MemberSymbol : ISymbol
    {
        [NotNull]
        private readonly Member fxCopMember;

        protected MemberSymbol([NotNull] Member fxCopMember)
        {
            Guard.NotNull(fxCopMember, "fxCopMember");
            this.fxCopMember = fxCopMember;
        }

        public string Name
        {
            get
            {
                return fxCopMember.Name.Name;
            }
        }

        public abstract TypeSymbol Type { get; }

        public TypeSymbol ContainingType
        {
            get
            {
                return new TypeSymbol(fxCopMember.DeclaringType);
            }
        }

        public bool HasCompilerGeneratedAnnotation
        {
            get
            {
                return fxCopMember.Attributes.Any(HelperForSymbols.IsCompilerGeneratedAttribute);
            }
        }

        public bool HasDebuggerNonUserCodeAnnotation
        {
            get
            {
                return fxCopMember.Attributes.Any(HelperForSymbols.IsDebuggerNonUserCodeAttribute);
            }
        }

        public bool HasNullabilityAnnotation(bool appliesToItem)
        {
            return
                fxCopMember.Attributes.Any(
                    x => appliesToItem ? x.IsItemNullabilityAttribute() : x.IsNullabilityAttribute());
        }

        public abstract string GetDocumentationCommentId();

        public abstract T Accept<T>(ISymbolVisitor<T> visitor);

        public abstract bool IsImplementationForInterfaceMember([NotNull] MemberSymbol interfaceMember);

        public override string ToString()
        {
            return fxCopMember.FullName;
        }
    }
}