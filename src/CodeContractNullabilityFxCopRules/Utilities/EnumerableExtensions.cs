using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace CodeContractNullabilityFxCopRules.Utilities
{
    /// <summary />
    public static class EnumerableExtensions
    {
        [NotNull]
        [ItemNotNull]
        public static IEnumerable<T> PrependIfNotNull<T>([NotNull] [ItemNotNull] this IEnumerable<T> source,
            [CanBeNull] T firstElement)
        {
            Guard.NotNull(source, "source");

            // ReSharper disable once CompareNonConstrainedGenericWithNull
            if (firstElement != null)
            {
                yield return firstElement;
            }

            foreach (T item in source)
            {
                yield return item;
            }
        }

        [NotNull]
        [ItemNotNull]
        public static IEnumerable<T> EmptyIfNull<T>([CanBeNull] [ItemNotNull] this IEnumerable<T> source)
        {
            return source ?? Enumerable.Empty<T>();
        }
    }
}