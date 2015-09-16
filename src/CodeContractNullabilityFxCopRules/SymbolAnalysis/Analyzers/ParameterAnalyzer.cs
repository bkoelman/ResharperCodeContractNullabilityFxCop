using System;
using System.Collections.Generic;
using System.Linq;
using CodeContractNullabilityFxCopRules.ExternalAnnotations.Storage;
using CodeContractNullabilityFxCopRules.SymbolAnalysis.Symbols;
using JetBrains.Annotations;

namespace CodeContractNullabilityFxCopRules.SymbolAnalysis.Analyzers
{
    /// <summary>
    /// Performs analysis of a method parameter.
    /// </summary>
    public class ParameterAnalyzer : BaseAnalyzer<ParameterSymbol>
    {
        public ParameterAnalyzer([NotNull] ParameterSymbol symbol, [NotNull] ExternalAnnotationsMap externalAnnotations,
            bool appliesToItem)
            : base(symbol, externalAnnotations, appliesToItem)
        {
        }

        protected override bool RequiresAnnotation()
        {
            if (IsCompilerNamed(Symbol.ContainingMethod.Name))
            {
                return false;
            }

            if (Symbol.ContainingType != null && Symbol.ContainingType.HasCompilerGeneratedAnnotation)
            {
                return false;
            }

            if (Symbol.ContainingMethod.IsPropertyOrEventAccessor)
            {
                if (Symbol.Name != "value" &&
                    (Symbol.ContainingMethod.Name == "get_Item" || Symbol.ContainingMethod.Name == "set_Item"))
                {
                    // When the parameter of an indexer property requires annotation, this code gets 
                    // called twice: once for the parameter in get_Item method, then for the parameter 
                    // in the set_Item method.
                    // As a workaround to prevent reporting twice, we instead report on the property itself.
                    if (Symbol.ContainingMethod.ContainingProperty != null)
                    {
                        ReportOnSymbol(Symbol.ContainingMethod.ContainingProperty);
                    }
                }
                else
                {
                    return false;
                }
            }

            return base.RequiresAnnotation();
        }

        protected override bool HasAnnotationInBaseClass()
        {
            ParameterSymbol baseParameter = TryGetBaseParameterFor(Symbol);
            while (baseParameter != null)
            {
                if (baseParameter.HasNullabilityAnnotation(AppliesToItem) ||
                    ExternalAnnotations.Contains(baseParameter, AppliesToItem) ||
                    HasAnnotationInInterface(baseParameter))
                {
                    return true;
                }

                baseParameter = TryGetBaseParameterFor(baseParameter);
            }
            return false;
        }

        [CanBeNull]
        private ParameterSymbol TryGetBaseParameterFor([NotNull] ParameterSymbol parameterSymbol)
        {
            MethodSymbol baseMethod = parameterSymbol.ContainingMethod.OverriddenMethod;
            if (baseMethod != null)
            {
                return baseMethod.Parameters[parameterSymbol.ParameterListIndex];
            }

            if (parameterSymbol.ContainingMethod.ContainingProperty != null)
            {
                PropertySymbol baseProperty = parameterSymbol.ContainingMethod.ContainingProperty.OverriddenProperty;
                if (baseProperty != null)
                {
                    return baseProperty.Parameters[parameterSymbol.ParameterListIndex];
                }
            }

            return null;
        }

        protected override bool HasAnnotationInInterface(ParameterSymbol parameter)
        {
            if (parameter.ContainingType != null)
            {
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (TypeSymbol iface in parameter.ContainingType.Interfaces)
                {
                    MemberSymbol ifaceMember =
                        iface.Members.FirstOrDefault(parameter.ContainingMethod.IsImplementationForInterfaceMember);

                    if (ifaceMember != null)
                    {
                        IList<ParameterSymbol> ifaceParameters = GetParametersFor(ifaceMember);
                        ParameterSymbol ifaceParameter = ifaceParameters[parameter.ParameterListIndex];

                        if (ifaceParameter.HasNullabilityAnnotation(AppliesToItem) ||
                            ExternalAnnotations.Contains(ifaceParameter, AppliesToItem))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        [NotNull]
        [ItemNotNull]
        private static IList<ParameterSymbol> GetParametersFor([NotNull] MemberSymbol member)
        {
            var method = member as MethodSymbol;
            if (method != null)
            {
                return method.Parameters;
            }

            var property = member as PropertySymbol;
            if (property != null)
            {
                return property.Parameters;
            }

            throw new NotSupportedException(string.Format("Expected MethodSymbol or PropertySymbol, not {0}.",
                member.GetType()));
        }
    }
}