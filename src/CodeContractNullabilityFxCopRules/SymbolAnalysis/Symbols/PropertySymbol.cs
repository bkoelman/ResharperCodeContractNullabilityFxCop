using System.Collections.Generic;
using System.Linq;
using CodeContractNullabilityFxCopRules.Utilities;
using JetBrains.Annotations;
using Microsoft.FxCop.Sdk;

namespace CodeContractNullabilityFxCopRules.SymbolAnalysis.Symbols
{
    /// <summary>
    /// Represents a property or indexer.
    /// </summary>
    public sealed class PropertySymbol : MemberSymbol
    {
        [NotNull]
        private readonly PropertyNode fxCopProperty;

        public override TypeSymbol Type
        {
            get
            {
                return new TypeSymbol(fxCopProperty.Type);
            }
        }

        [CanBeNull]
        public PropertySymbol OverriddenProperty
        {
            get
            {
                return fxCopProperty.OverriddenProperty != null
                    ? new PropertySymbol(fxCopProperty.OverriddenProperty)
                    : null;
            }
        }

        [NotNull]
        [ItemNotNull]
        public IList<ParameterSymbol> Parameters
        {
            get
            {
                return fxCopProperty.Parameters.Select(x => new ParameterSymbol(x)).ToArray();
            }
        }

        public PropertySymbol([NotNull] PropertyNode fxCopProperty)
            : base(fxCopProperty)
        {
            this.fxCopProperty = fxCopProperty;
        }

        public override string GetDocumentationCommentId()
        {
            return DocumentationCommentFactory.GetDocumentationCommentId(fxCopProperty);
        }

        public override T Accept<T>(ISymbolVisitor<T> visitor)
        {
            return visitor.VisitProperty(this);
        }

        public override bool IsImplementationForInterfaceMember(MemberSymbol interfaceMember)
        {
            var interfaceProperty = interfaceMember as PropertySymbol;
            if (interfaceProperty != null)
            {
                if (HasExplicitGetterImplementation(interfaceProperty) ||
                    HasExplicitSetterImplementation(interfaceProperty))
                {
                    // This property is an explicit interface implementation.
                    return true;
                }

                bool isImplicitImplementation = Name == interfaceProperty.Name &&
                    fxCopProperty.ParametersMatchStructurally(interfaceProperty.fxCopProperty.Parameters);

                // If, besides this method, an explicit implementation exists, that one wins.
                if (isImplicitImplementation && !ContainingTypeHasExplicitInterfaceImplementationFor(interfaceProperty))
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasExplicitGetterImplementation([NotNull] PropertySymbol interfaceProperty)
        {
            return fxCopProperty.Getter != null &&
                fxCopProperty.Getter.ImplementedInterfaceMethods.EmptyIfNull()
                    .Contains(interfaceProperty.fxCopProperty.Getter);
        }

        private bool HasExplicitSetterImplementation([NotNull] PropertySymbol interfaceProperty)
        {
            return fxCopProperty.Setter != null &&
                fxCopProperty.Setter.ImplementedInterfaceMethods.EmptyIfNull()
                    .Contains(interfaceProperty.fxCopProperty.Setter);
        }

        private bool ContainingTypeHasExplicitInterfaceImplementationFor([NotNull] PropertySymbol interfaceProperty)
        {
            string explicitInterfaceMethodName =
                interfaceProperty.ContainingType.GetFullUnmangledNameWithTypeParameters() + "." + Name;

            return
                ContainingType.Members.OfType<PropertySymbol>()
                    .Any(
                        m =>
                            m.Name == explicitInterfaceMethodName &&
                                m.fxCopProperty.ParametersMatchStructurally(fxCopProperty.Parameters));
        }
    }
}