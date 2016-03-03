using System.Collections.Generic;
using System.Linq;
using CodeContractNullabilityFxCopRules.Utilities;
using JetBrains.Annotations;
using Microsoft.FxCop.Sdk;

namespace CodeContractNullabilityFxCopRules.SymbolAnalysis.Symbols
{
    /// <summary>
    /// Represents a method or method-like symbol (including constructor, destructor, operator, or property/event accessor).
    /// </summary>
    public sealed class MethodSymbol : MemberSymbol
    {
        [NotNull]
        private readonly Method fxCopMethod;

        public override TypeSymbol Type
        {
            get
            {
                return new TypeSymbol(fxCopMethod.ReturnType);
            }
        }

        [CanBeNull]
        public PropertySymbol ContainingProperty
        {
            get
            {
                PropertyNode propertyNode = IsPropertyOrEventAccessor
                    ? fxCopMethod.DeclaringMember as PropertyNode
                    : null;
                return propertyNode != null ? new PropertySymbol(propertyNode) : null;
            }
        }

        [CanBeNull]
        public MethodSymbol OverriddenMethod
        {
            get
            {
                return fxCopMethod.OverriddenMethod != null ? new MethodSymbol(fxCopMethod.OverriddenMethod) : null;
            }
        }

        public bool IsPropertyOrEventAccessor
        {
            get
            {
                return fxCopMethod.IsAccessor;
            }
        }

        public bool IsCompilerControlled
        {
            get
            {
                return fxCopMethod.IsCompilerControlled;
            }
        }

        public bool IsAsync
        {
            get
            {
                return fxCopMethod.Attributes.Any(HelperForSymbols.IsAsyncStateMachineAttribute);
            }
        }

        [NotNull]
        [ItemNotNull]
        public IList<ParameterSymbol> Parameters
        {
            get
            {
                return fxCopMethod.Parameters.Select(x => new ParameterSymbol(x)).ToArray();
            }
        }

        public MethodSymbol([NotNull] Method fxCopMethod)
            : base(fxCopMethod)
        {
            this.fxCopMethod = fxCopMethod;
        }

        public override string GetDocumentationCommentId()
        {
            return DocumentationCommentFactory.GetDocumentationCommentId(fxCopMethod);
        }

        public override T Accept<T>(ISymbolVisitor<T> visitor)
        {
            return visitor.VisitMethod(this);
        }

        [NotNull]
        public MethodSymbol AsUnboundGenericMethodOrThis()
        {
            Method template = fxCopMethod.Template;
            return template != null ? new MethodSymbol(template) : this;
        }

        public override bool IsImplementationForInterfaceMember(MemberSymbol interfaceMember)
        {
            var interfaceMethod = interfaceMember as MethodSymbol;
            if (interfaceMethod != null)
            {
                if (fxCopMethod.ImplementedInterfaceMethods.EmptyIfNull().Contains(interfaceMethod.fxCopMethod))
                {
                    // This method is an explicit interface implementation.
                    return true;
                }

                bool isImplicitImplementation = Name == interfaceMethod.Name &&
                    fxCopMethod.ParametersMatchStructurally(interfaceMethod.fxCopMethod.Parameters);

                // If, besides this method, an explicit implementation exists, that one wins.
                if (isImplicitImplementation && !ContainingTypeHasExplicitInterfaceImplementationFor(interfaceMethod))
                {
                    return true;
                }
            }

            return false;
        }

        private bool ContainingTypeHasExplicitInterfaceImplementationFor([NotNull] MethodSymbol interfaceMethod)
        {
            string explicitInterfaceMethodName =
                interfaceMethod.ContainingType.GetFullUnmangledNameWithTypeParameters() + "." + Name;

            return
                ContainingType.Members.OfType<MethodSymbol>()
                    .Any(
                        m =>
                            m.Name == explicitInterfaceMethodName &&
                                m.fxCopMethod.ParametersMatchStructurally(fxCopMethod.Parameters));
        }
    }
}