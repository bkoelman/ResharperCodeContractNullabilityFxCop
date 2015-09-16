using System;
using System.Linq;
using CodeContractNullabilityFxCopRules.ExternalAnnotations.Storage;
using CodeContractNullabilityFxCopRules.SymbolAnalysis.Symbols;
using CodeContractNullabilityFxCopRules.Utilities;
using JetBrains.Annotations;

namespace CodeContractNullabilityFxCopRules.SymbolAnalysis.Analyzers
{
    /// <summary>
    /// Performs the basic analysis (and reporting) required to determine whether a member or parameter needs annotation.
    /// </summary>
    public abstract class BaseAnalyzer
    {
        public abstract void Analyze([NotNull] Action<ISymbol> reportProblem);
    }

    /// <summary>
    /// Performs the basic analysis (and reporting) required to determine whether a member or parameter needs annotation.
    /// </summary>
    /// <typeparam name="TSymbol">
    /// The symbol type of the class member to analyze.
    /// </typeparam>
    public abstract class BaseAnalyzer<TSymbol> : BaseAnalyzer
        where TSymbol : class, ISymbol
    {
        [NotNull]
        protected TSymbol Symbol { get; private set; }

        // Used for tracking duplicates. Note this does not actually change which symbol VS will highlight.
        [CanBeNull]
        private ISymbol reportSymbol;

        [NotNull]
        protected ExternalAnnotationsMap ExternalAnnotations { get; private set; }

        protected bool AppliesToItem { get; private set; }

        protected BaseAnalyzer([NotNull] TSymbol symbol, [NotNull] ExternalAnnotationsMap externalAnnotations,
            bool appliesToItem)
        {
            Guard.NotNull(symbol, "symbol");
            Guard.NotNull(externalAnnotations, "externalAnnotations");

            Symbol = symbol;
            ExternalAnnotations = externalAnnotations;
            AppliesToItem = appliesToItem;
        }

        public override void Analyze(Action<ISymbol> reportProblem)
        {
            Guard.NotNull(reportProblem, "reportProblem");

            AnalyzeNullability(reportProblem);
        }

        private void AnalyzeNullability([NotNull] Action<ISymbol> reportProblem)
        {
            if (Symbol.HasNullabilityAnnotation(AppliesToItem))
            {
                return;
            }

            TypeSymbol symbolType = SymbolTypeResolver.TryGetEffectiveType(Symbol, AppliesToItem);
            if (symbolType == null || !symbolType.CanContainNull)
            {
                return;
            }

            if (symbolType.IsCompilerControlled || symbolType.HasCompilerGeneratedAnnotation)
            {
                return;
            }

            if (IsCompilerNamed(Symbol.Name) ||
                (Symbol.ContainingType != null && IsCompilerNamed(Symbol.ContainingType.Name)))
            {
                return;
            }

            if (Symbol.HasCompilerGeneratedAnnotation || Symbol.HasDebuggerNonUserCodeAnnotation)
            {
                return;
            }

            if (Symbol.ContainingType != null && Symbol.ContainingType.HasResharperConditionalAnnotation)
            {
                return;
            }

            if (ExternalAnnotations.Contains(Symbol, AppliesToItem))
            {
                return;
            }

            if (RequiresAnnotation())
            {
                reportProblem(reportSymbol ?? Symbol);
            }
        }

        protected static bool IsCompilerNamed([NotNull] string identifierName)
        {
            return identifierName.IndexOfAny(new[] { '<', '>', '$' }) != -1;
        }

        protected virtual bool RequiresAnnotation()
        {
            if (HasAnnotationInInterface(Symbol))
            {
                // Resharper reports nullability attribute as unneeded on explicit interface implementation
                // if property on interface contains nullability attribute.
                return false;
            }

            if (HasAnnotationInBaseClass())
            {
                // Resharper reports nullability attribute as unneeded 
                // if property on base class contains nullability attribute.
                return false;
            }

            return true;
        }

        protected virtual bool HasAnnotationInInterface([NotNull] TSymbol symbol)
        {
            // When a member implements a decorated interface member, that member must only 
            // be decorated when the class also contains an explicit interface implementation.

            var member = symbol as MemberSymbol;
            if (member != null && member.ContainingType != null)
            {
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (TypeSymbol iface in member.ContainingType.Interfaces)
                {
                    MemberSymbol ifaceMember = iface.Members.FirstOrDefault(member.IsImplementationForInterfaceMember);

                    if (ifaceMember != null)
                    {
                        if (ifaceMember.HasNullabilityAnnotation(AppliesToItem) ||
                            ExternalAnnotations.Contains(ifaceMember, AppliesToItem))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        protected virtual bool HasAnnotationInBaseClass()
        {
            return false;
        }

        protected void ReportOnSymbol([NotNull] ISymbol symbol)
        {
            Guard.NotNull(symbol, "symbol");
            reportSymbol = symbol;
        }
    }
}