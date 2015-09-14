using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace FxCopUnitTestRunner
{
    /// <summary>
    /// Member precondition checks.
    /// </summary>
    public static class Guard
    {
        [AssertionMethod]
        [ContractAnnotation("value: null => halt")]
        [DebuggerStepThrough]
        public static void NotNull<T>([CanBeNull] [NoEnumeration] T value, [NotNull] [InvokerParameterName] string name)
        {
            // ReSharper disable once CompareNonConstrainedGenericWithNull
            if (value == null)
            {
                throw new ArgumentNullException(name);
            }
        }
    }
}