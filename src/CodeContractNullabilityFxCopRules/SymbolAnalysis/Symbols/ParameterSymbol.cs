using System;
using System.Linq;
using CodeContractNullabilityFxCopRules.Utilities;
using JetBrains.Annotations;
using Microsoft.FxCop.Sdk;

namespace CodeContractNullabilityFxCopRules.SymbolAnalysis.Symbols
{
    /// <summary>
    /// Represents a parameter of a method or property.
    /// </summary>
    public sealed class ParameterSymbol : ISymbol
    {
        [NotNull]
        private readonly Parameter fxCopParameter;

        public string Name
        {
            get
            {
                return fxCopParameter.Name.Name;
            }
        }

        public TypeSymbol Type
        {
            get
            {
                return new TypeSymbol(fxCopParameter.Type);
            }
        }

        [NotNull]
        public TypeSymbol ContainingType
        {
            get
            {
                return new TypeSymbol(fxCopParameter.DeclaringMethod.DeclaringType);
            }
        }

        public string ContainingAssemblyPath
        {
            get
            {
                return ContainingType.ContainingAssemblyPath;
            }
        }

        [NotNull]
        public MethodSymbol ContainingMethod
        {
            get
            {
                return new MethodSymbol(fxCopParameter.DeclaringMethod);
            }
        }

        public int ParameterListIndex
        {
            get
            {
                return fxCopParameter.ParameterListIndex;
            }
        }

        public bool HasCompilerGeneratedAnnotation
        {
            get
            {
                return fxCopParameter.Attributes.Any(HelperForSymbols.IsCompilerGeneratedAttribute);
            }
        }

        public bool HasDebuggerNonUserCodeAnnotation
        {
            get
            {
                return fxCopParameter.Attributes.Any(HelperForSymbols.IsDebuggerNonUserCodeAttribute);
            }
        }

        public ParameterSymbol([NotNull] Parameter fxCopParameter)
        {
            Guard.NotNull(fxCopParameter, "fxCopParameter");
            this.fxCopParameter = fxCopParameter;
        }

        public bool HasNullabilityAnnotation(bool appliesToItem)
        {
            return
                fxCopParameter.Attributes.Any(
                    x => appliesToItem ? x.IsItemNullabilityAttribute() : x.IsNullabilityAttribute());
        }

        public string GetDocumentationCommentId()
        {
            throw new NotSupportedException();
        }

        public T Accept<T>(ISymbolVisitor<T> visitor)
        {
            return visitor.VisitParameter(this);
        }

        public ParameterSymbol AsUnboundGenericParameterOrThis()
        {
            Method template = fxCopParameter.DeclaringMethod.Template;
            if (template != null)
            {
                return new ParameterSymbol(template.Parameters[fxCopParameter.ParameterListIndex]);
            }
            return this;
        }

        public override string ToString()
        {
            return fxCopParameter.Name.ToString();
        }
    }
}