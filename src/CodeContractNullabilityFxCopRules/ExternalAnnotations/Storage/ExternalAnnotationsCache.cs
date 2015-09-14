﻿using System;
using System.Runtime.Serialization;
using CodeContractNullabilityFxCopRules.Utilities;
using JetBrains.Annotations;

namespace CodeContractNullabilityFxCopRules.ExternalAnnotations.Storage
{
    /// <summary>
    /// Represents the external annotations cache file, stored in compact form.
    /// </summary>
    [DataContract(Namespace = CacheNamespace)]
    [Serializable]
    internal class ExternalAnnotationsCache
    {
        internal const string CacheNamespace = "CodeContractNullability";

        [DataMember(Name = "lastWriteTimeUtc")]
        public DateTime LastWriteTimeUtc { get; private set; }

        [DataMember(Name = "annotations")]
        [NotNull]
        public ExternalAnnotationsMap ExternalAnnotations { get; private set; }

        public ExternalAnnotationsCache()
        {
            ExternalAnnotations = new ExternalAnnotationsMap();
        }

        public ExternalAnnotationsCache(DateTime lastWriteTimeUtc, [NotNull] ExternalAnnotationsMap externalAnnotations)
        {
            Guard.NotNull(externalAnnotations, "externalAnnotations");

            LastWriteTimeUtc = lastWriteTimeUtc;
            ExternalAnnotations = externalAnnotations;
        }
    }
}