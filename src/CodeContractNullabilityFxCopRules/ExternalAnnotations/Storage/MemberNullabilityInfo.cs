using System;
using System.Runtime.Serialization;
using CodeContractNullabilityFxCopRules.Utilities;
using JetBrains.Annotations;

namespace CodeContractNullabilityFxCopRules.ExternalAnnotations.Storage
{
    /// <summary>
    /// Data storage for external annotations.
    /// </summary>
    [Serializable]
    [DataContract(Name = "i", Namespace = ExternalAnnotationsCache.CacheNamespace)]
    public class MemberNullabilityInfo
    {
        [DataMember(Name = "t")]
        [NotNull]
        public string Type { get; private set; }

        [DataMember(Name = "n")]
        public bool HasNullabilityDefined { get; set; }

        [DataMember(Name = "p")]
        [NotNull]
        public ParameterNullabilityMap ParametersNullability { get; private set; }

        // ReSharper disable once NotNullMemberIsNotInitialized
        // Reason: This ctor is only needed for MsgPack serializer.
        public MemberNullabilityInfo()
        {
        }

        public MemberNullabilityInfo([NotNull] string type)
        {
            Guard.NotNull(type, "type");

            Type = type;
            ParametersNullability = new ParameterNullabilityMap();
        }
    }
}