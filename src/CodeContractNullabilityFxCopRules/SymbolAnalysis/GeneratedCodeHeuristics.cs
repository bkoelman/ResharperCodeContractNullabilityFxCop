using System.Linq;
using CodeContractNullabilityFxCopRules.SymbolAnalysis.Symbols;
using CodeContractNullabilityFxCopRules.Utilities;
using JetBrains.Annotations;

namespace CodeContractNullabilityFxCopRules.SymbolAnalysis
{
    /// <summary>
    /// Some heuristics to determine if a symbol likely origined from generated code.
    /// </summary>
    public class GeneratedCodeHeuristics
    {
        [NotNull]
        private readonly FieldSymbol field;

        private bool IsWinFormsControl
        {
            get
            {
                return field.ContainingType != null &&
                    field.ContainingType.IsOrDerivesFrom("System.Windows.Forms.Control");
            }
        }

        private bool IsGeneratedContainerComponentsField
        {
            get
            {
                return field.Name == "components" && field.Type.IsOrDerivesFrom("System.ComponentModel.IContainer");
            }
        }

        private bool ImplementsSystemComponent
        {
            get
            {
                TypeSymbol currentType = field.Type;
                while (currentType != null)
                {
                    if (currentType.Interfaces.Any(iface => iface.FullName == "System.ComponentModel.IComponent"))
                    {
                        return true;
                    }
                    currentType = currentType.BaseType;
                }
                return false;
            }
        }

        public GeneratedCodeHeuristics([NotNull] FieldSymbol field)
        {
            Guard.NotNull(field, "field");
            this.field = field;
        }

        public bool IsWindowsFormsDesignerGenerated()
        {
            if (!IsWinFormsControl)
            {
                return false;
            }

            if (IsGeneratedContainerComponentsField)
            {
                return true;
            }

            return ImplementsSystemComponent;
        }
    }
}