using System;
using System.IO;
using System.Reflection;
using CodeContractNullabilityFxCopRules.Utilities;
using JetBrains.Annotations;
using MsgPack.Serialization;

namespace CodeContractNullabilityFxCopRules.ExternalAnnotations
{
    public sealed class MsgPackSerializationService
    {
        static MsgPackSerializationService()
        {
            MsgPackAssemblyLoader.EnsureInitialized();
        }

        [NotNull]
        public T ReadObject<T>([NotNull] Stream source)
        {
            Guard.NotNull(source, "source");

            var serializer = CreateSerializer<T>();
            return serializer.Unpack(source);
        }

        public void WriteObject<T>([NotNull] T instance, [NotNull] Stream target)
        {
            Guard.NotNull(instance, "instance");
            Guard.NotNull(target, "target");

            var serializer = CreateSerializer<T>();
            serializer.Pack(target, instance);
        }

        [NotNull]
        private static MessagePackSerializer<T> CreateSerializer<T>()
        {
            var serializer = SerializationContext.Default.GetSerializer<T>();
            return serializer;
        }

        private static class MsgPackAssemblyLoader
        {
            static MsgPackAssemblyLoader()
            {
                Assembly assembly = GetAssemblyFromResource();
                RegisterResolverFor(assembly);
            }

            public static void EnsureInitialized()
            {
            }

            [NotNull]
            private static Assembly GetAssemblyFromResource()
            {
                string thisAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
                string resourceName = thisAssemblyName + ".MsgPack.dll";

                using (Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    var assemblyData = new byte[imageStream.Length];
                    imageStream.Read(assemblyData, 0, assemblyData.Length);

                    return Assembly.Load(assemblyData);
                }
            }

            private static void RegisterResolverFor([NotNull] Assembly assembly)
            {
                AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
                {
                    var assemblyName = new AssemblyName(args.Name);
                    return assemblyName.ToString() == assembly.GetName().ToString() ? assembly : null;
                };
            }
        }
    }
}