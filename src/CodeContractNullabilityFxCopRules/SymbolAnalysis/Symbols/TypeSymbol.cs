using System;
using System.Collections.Generic;
using System.Linq;
using CodeContractNullabilityFxCopRules.Utilities;
using JetBrains.Annotations;
using Microsoft.FxCop.Sdk;

namespace CodeContractNullabilityFxCopRules.SymbolAnalysis.Symbols
{
    /// <summary>
    /// Represents a type (such as class, struct or enum).
    /// </summary>
    public class TypeSymbol : ISymbol
    {
        [NotNull]
        private readonly TypeNode fxCopType;

        public string Name
        {
            get
            {
                return fxCopType.Name.Name;
            }
        }

        [NotNull]
        public string FullName
        {
            get
            {
                return fxCopType.FullName;
            }
        }

        public TypeSymbol Type
        {
            get
            {
                return new TypeSymbol(FrameworkTypes.Type);
            }
        }

        [CanBeNull]
        public TypeSymbol BaseType
        {
            get
            {
                return fxCopType.BaseType != null ? new TypeSymbol(fxCopType.BaseType) : null;
            }
        }

        public TypeSymbol ContainingType
        {
            get
            {
                return fxCopType.DeclaringType != null ? new TypeSymbol(fxCopType.DeclaringType) : null;
            }
        }

        [CanBeNull]
        public TypeSymbol UnboundGenericType
        {
            get
            {
                return fxCopType.Template == null ? null : new TypeSymbol(fxCopType.Template);
            }
        }

        [NotNull]
        [ItemNotNull]
        public IList<TypeSymbol> TypeArguments
        {
            get
            {
                return fxCopType.TemplateArguments.EmptyIfNull().Select(x => new TypeSymbol(x)).ToArray();
            }
        }

        [NotNull]
        [ItemNotNull]
        public IList<TypeSymbol> Interfaces
        {
            get
            {
                return fxCopType.Interfaces.EmptyIfNull().Select(x => new TypeSymbol(x)).ToArray();
            }
        }

        [NotNull]
        [ItemNotNull]
        public IEnumerable<MemberSymbol> Members
        {
            get
            {
                var factory = new SymbolFactory();
                return fxCopType.Members.Select(factory.CreateOrNull).Where(symbol => symbol != null);
            }
        }

        public bool HasCompilerGeneratedAnnotation
        {
            get
            {
                return fxCopType.Attributes.Any(HelperForSymbols.IsCompilerGeneratedAttribute);
            }
        }

        public bool HasDebuggerNonUserCodeAnnotation
        {
            get
            {
                return fxCopType.Attributes.Any(HelperForSymbols.IsDebuggerNonUserCodeAttribute);
            }
        }

        public bool HasResharperConditionalAnnotation
        {
            get
            {
                return fxCopType.Attributes.Any(HelperForSymbols.IsResharperConditionalAttribute);
            }
        }

        public bool CanContainNull
        {
            get
            {
                return !IsVoidType && (IsSystemNullableType || !IsValueType);
            }
        }

        private bool IsVoidType
        {
            get
            {
                return FullName == "System.Void";
            }
        }

        private bool IsSystemNullableType
        {
            get
            {
                return FullName.StartsWith("System.Nullable`1", StringComparison.Ordinal);
            }
        }

        private bool IsValueType
        {
            get
            {
                // FxCop incorrectly reports IsValueType => false in the next scenario:
                // public class ValueHolder<T> where T : struct
                // {
                //     public T Result { get; set; }
                // }
                return fxCopType.IsValueType ||
                    (fxCopType.BaseType != null && fxCopType.BaseType.FullName == "System.ValueType");
            }
        }

        public bool IsCompilerControlled
        {
            get
            {
                return fxCopType.IsCompilerControlled;
            }
        }

        public TypeSymbol([NotNull] TypeNode fxCopType)
        {
            Guard.NotNull(fxCopType, "fxCopType");
            this.fxCopType = UnwrapRefOutType(fxCopType);
        }

        [NotNull]
        private static TypeNode UnwrapRefOutType([NotNull] TypeNode type)
        {
            var reference = type as Reference;
            return reference != null ? reference.ElementType : type;
        }

        public bool HasNullabilityAnnotation(bool appliesToItem)
        {
            return false;
        }

        public string GetDocumentationCommentId()
        {
            return DocumentationCommentFactory.GetDocumentationCommentId(fxCopType);
        }

        public T Accept<T>(ISymbolVisitor<T> visitor)
        {
            return visitor.VisitType(this);
        }

        [NotNull]
        public string GetFullUnmangledNameWithTypeParameters()
        {
            return fxCopType.GetFullUnmangledNameWithTypeParameters();
        }

        public bool IsOrDerivesFrom([CanBeNull] string typeName)
        {
            TypeSymbol baseType = this;
            while (baseType != null)
            {
                if (baseType.FullName == typeName)
                {
                    return true;
                }

                baseType = baseType.BaseType;
            }
            return false;
        }

        public override string ToString()
        {
            return fxCopType.FullName;
        }
    }
}