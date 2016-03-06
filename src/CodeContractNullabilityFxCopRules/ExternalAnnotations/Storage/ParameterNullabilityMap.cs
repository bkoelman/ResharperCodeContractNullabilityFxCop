using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace CodeContractNullabilityFxCopRules.ExternalAnnotations.Storage
{
    /// <summary>
    /// Data storage for external annotations.
    /// </summary>
    [Serializable]
    [CollectionDataContract(Name = "p", ItemName = "e", KeyName = "k", ValueName = "v",
        Namespace = ExternalAnnotationsCache.CacheNamespace)]
    public class ParameterNullabilityMap : Dictionary<string, bool>
    {
        public ParameterNullabilityMap()
        {
        }

        protected ParameterNullabilityMap([NotNull] SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}