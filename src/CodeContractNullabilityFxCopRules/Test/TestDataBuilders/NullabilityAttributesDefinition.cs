using System.Text;
using CodeContractNullabilityFxCopRules.Utilities;
using JetBrains.Annotations;

namespace CodeContractNullabilityFxCopRules.Test.TestDataBuilders
{
    public class NullabilityAttributesDefinition
    {
        [NotNull]
        private readonly string codeNamespace;

        [NotNull]
        public string SourceText
        {
            get
            {
                var textBuilder = new StringBuilder();
                if (true)
                {
                    if (!string.IsNullOrEmpty(codeNamespace))
                    {
                        textBuilder.AppendLine();
                        textBuilder.AppendLine("namespace " + codeNamespace);
                        textBuilder.AppendLine("{");
                    }

                    textBuilder.AppendLine(@"
    [System.AttributeUsage(
        System.AttributeTargets.Method | System.AttributeTargets.Parameter | System.AttributeTargets.Property |
        System.AttributeTargets.Delegate | System.AttributeTargets.Field | System.AttributeTargets.Event)]
    //[System.Diagnostics.Conditional(""JETBRAINS_ANNOTATIONS"")]
    public sealed class CanBeNullAttribute : System.Attribute { }

    [System.AttributeUsage(
        System.AttributeTargets.Method | System.AttributeTargets.Parameter | System.AttributeTargets.Property |
        System.AttributeTargets.Delegate | System.AttributeTargets.Field | System.AttributeTargets.Event)]
    //[System.Diagnostics.Conditional(""JETBRAINS_ANNOTATIONS"")]
    public sealed class NotNullAttribute : System.Attribute { }

    [System.AttributeUsage(
        System.AttributeTargets.Method | System.AttributeTargets.Parameter | System.AttributeTargets.Property |
        System.AttributeTargets.Delegate | System.AttributeTargets.Field)]
    //[System.Diagnostics.Conditional(""JETBRAINS_ANNOTATIONS"")]
    public sealed class ItemNotNullAttribute : System.Attribute { }

    [System.AttributeUsage(
        System.AttributeTargets.Method | System.AttributeTargets.Parameter | System.AttributeTargets.Property |
        System.AttributeTargets.Delegate | System.AttributeTargets.Field)]
    //[System.Diagnostics.Conditional(""JETBRAINS_ANNOTATIONS"")]
    public sealed class ItemCanBeNullAttribute : System.Attribute { }
");

                    if (!string.IsNullOrEmpty(codeNamespace))
                    {
                        textBuilder.AppendLine("}");
                    }
                }
                return textBuilder.ToString();
            }
        }

        public NullabilityAttributesDefinition([NotNull] string codeNamespace)
        {
            Guard.NotNull(codeNamespace, "codeNamespace");

            this.codeNamespace = codeNamespace;
        }
    }
}