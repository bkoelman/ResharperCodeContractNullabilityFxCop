using CodeContractNullabilityFxCopRules.Utilities;
using JetBrains.Annotations;
using Microsoft.FxCop.Sdk;

namespace CodeContractNullabilityFxCopRules.SymbolAnalysis
{
    /// <summary />
    internal static class HelperForSymbols
    {
        public static bool IsNullabilityAttribute([NotNull] this AttributeNode attribute)
        {
            Guard.NotNull(attribute, "attribute");

            return attribute.Type.Name.Name == "NotNullAttribute" || attribute.Type.Name.Name == "CanBeNullAttribute";
        }

        public static bool IsItemNullabilityAttribute([NotNull] this AttributeNode attribute)
        {
            Guard.NotNull(attribute, "attribute");

            return attribute.Type.Name.Name == "ItemNotNullAttribute" ||
                attribute.Type.Name.Name == "ItemCanBeNullAttribute";
        }

        public static bool IsCompilerGeneratedAttribute([NotNull] AttributeNode attribute)
        {
            Guard.NotNull(attribute, "attribute");

            return attribute.Type.Name.Name == "CompilerGeneratedAttribute";
        }

        public static bool IsDebuggerNonUserCodeAttribute([NotNull] AttributeNode attribute)
        {
            Guard.NotNull(attribute, "attribute");

            return attribute.Type.Name.Name == "DebuggerNonUserCodeAttribute";
        }

        public static bool IsAsyncStateMachineAttribute([NotNull] AttributeNode attribute)
        {
            Guard.NotNull(attribute, "attribute");

            return attribute.Type.Name.Name == "AsyncStateMachineAttribute";
        }

        public static bool IsResharperConditionalAttribute([NotNull] AttributeNode attribute)
        {
            Guard.NotNull(attribute, "attribute");

            if (attribute.Type.Name.Name == "ConditionalAttribute")
            {
                Expression expr = attribute.GetPositionalArgument(0);
                string argument = expr.ToString();
                return argument == "JETBRAINS_ANNOTATIONS";
            }

            return false;
        }
    }
}