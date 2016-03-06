using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using CodeContractNullabilityFxCopRules.SymbolAnalysis.Symbols;
using CodeContractNullabilityFxCopRules.Utilities;
using JetBrains.Annotations;

namespace CodeContractNullabilityFxCopRules.ExternalAnnotations.Storage
{
    /// <summary>
    /// Data storage for external annotations.
    /// </summary>
    [Serializable]
    [CollectionDataContract(Name = "annotations", ItemName = "e", KeyName = "k", ValueName = "v",
        Namespace = ExternalAnnotationsCache.CacheNamespace)]
    public class ExternalAnnotationsMap : Dictionary<string, MemberNullabilityInfo>
    {
        public ExternalAnnotationsMap()
        {
        }

        protected ExternalAnnotationsMap([NotNull] SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public bool Contains<TSymbol>([NotNull] TSymbol symbol, bool appliesToItem) where TSymbol : class, ISymbol
        {
            Guard.NotNull(symbol, "symbol");

            if (appliesToItem)
            {
                // Note: At the time of writing (August 2015), the set of Resharper's external annotations does not
                // include ItemNotNull / ItemCanBeNull elements. But we likely need to add support for them in the future.
                return false;
            }

            var parameterSymbol = symbol as ParameterSymbol;
            if (parameterSymbol != null)
            {
                parameterSymbol = parameterSymbol.AsUnboundGenericParameterOrThis();

                ISymbol containingSymbol = parameterSymbol.ContainingMethod.ContainingProperty != null
                    ? (ISymbol) parameterSymbol.ContainingMethod.ContainingProperty
                    : parameterSymbol.ContainingMethod;

                string methodId = containingSymbol.GetDocumentationCommentId();
                MemberNullabilityInfo memberInfo = TryGetMemberById(methodId);
                return memberInfo != null && memberInfo.ParametersNullability.ContainsKey(symbol.Name) &&
                    memberInfo.ParametersNullability[symbol.Name];
            }
            else
            {
                var methodSymbol = symbol as MethodSymbol;
                if (methodSymbol != null)
                {
                    symbol = (TSymbol) (ISymbol) methodSymbol.AsUnboundGenericMethodOrThis();
                }

                string id = symbol.GetDocumentationCommentId();
                MemberNullabilityInfo memberInfo = TryGetMemberById(id);
                return memberInfo != null && memberInfo.HasNullabilityDefined;
            }
        }

        [CanBeNull]
        private MemberNullabilityInfo TryGetMemberById([CanBeNull] string id)
        {
            if (!string.IsNullOrEmpty(id) && id[1] == ':')
            {
                // N = namespace, M = method, F = field, E = event, P = property, T = type
                string type = id.Substring(0, 1);
                string key = id.Substring(2);

                if (ContainsKey(key))
                {
                    MemberNullabilityInfo memberInfo = this[key];
                    if (memberInfo.Type == type)
                    {
                        return memberInfo;
                    }
                }
            }

            return null;
        }
    }
}