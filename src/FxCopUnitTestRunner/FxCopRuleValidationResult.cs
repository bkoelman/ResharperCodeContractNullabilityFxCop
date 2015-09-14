using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.FxCop.Sdk;

namespace FxCopUnitTestRunner
{
    public class FxCopRuleValidationResult
    {
        [NotNull]
        [ItemNotNull]
        public IReadOnlyList<Problem> Problems { get; private set; }

        public FxCopRuleValidationResult([NotNull] [ItemNotNull] IEnumerable<Problem> problems)
        {
            Guard.NotNull(problems, "problems");
            Problems = new List<Problem>(problems);
        }

        [NotNull]
        public string ProblemText
        {
            get
            {
                return Problems.Count == 0
                    ? NoProblemsText
                    : Problems.Count + " problems: " +
                        string.Join(", ", Problems.Select(p => "[" + p.Resolution.ToString() + "]"));
            }
        }

        public const string NoProblemsText = "No problems.";
    }
}