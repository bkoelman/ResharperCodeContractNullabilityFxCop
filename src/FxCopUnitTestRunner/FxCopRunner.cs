using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.FxCop.Sdk;

namespace FxCopUnitTestRunner
{
    internal class FxCopRunner
    {
        [NotNull]
        private readonly BaseIntrospectionRule rule;

        [NotNull]
        private readonly AssemblyUnderTest assembly;

        internal FxCopRunner([NotNull] BaseIntrospectionRule ruleToRun, [NotNull] AssemblyUnderTest onAssembly)
        {
            Guard.NotNull(ruleToRun, "ruleToRun");
            Guard.NotNull(onAssembly, "onAssembly");

            rule = ruleToRun;
            assembly = onAssembly;
        }

        public void Run()
        {
            AssemblyNode module = assembly.GetAssemblyNode();

            rule.BeforeAnalysis();
            CheckModule(module);
            CheckTypes(module);
            CheckResources(module);
            rule.AfterAnalysis();
        }

        private void CheckModule([NotNull] AssemblyNode module)
        {
            if (HasCheckOverloadAccepting(typeof (AssemblyNode)))
            {
                rule.Check(module);
            }
        }

        private void CheckResources([NotNull] AssemblyNode module)
        {
            if (HasCheckOverloadAccepting(typeof (Resource)))
            {
                foreach (Resource resource in module.Resources)
                {
                    rule.Check(resource);
                }
            }
        }

        private void CheckTypes([NotNull] AssemblyNode module)
        {
            foreach (TypeNode type in module.Types)
            {
                CheckType(type);
            }
        }

        private void CheckType([NotNull] TypeNode type)
        {
            if (HasCheckOverloadAccepting(typeof (TypeNode)))
            {
                rule.Check(type);
            }
            CheckMembers(type);
        }

        private void CheckMembers([NotNull] TypeNode type)
        {
            foreach (Member member in type.Members)
            {
                CheckMember(member);
            }
        }

        private void CheckMember([NotNull] Member member)
        {
            var nestedType = member as TypeNode;
            if (nestedType != null)
            {
                CheckType(nestedType);
            }
            else
            {
                if (HasCheckOverloadAccepting(typeof (Member)))
                {
                    rule.Check(member);
                }

                CheckParameters(member);
            }
        }

        private void CheckParameters([NotNull] Member member)
        {
            var method = member as Method;
            if (method != null && method.Parameters != null)
            {
                foreach (Parameter parameter in method.Parameters)
                {
                    CheckParameter(parameter);
                }
            }
        }

        private void CheckParameter([NotNull] Parameter parameter)
        {
            if (HasCheckOverloadAccepting(typeof (Parameter)))
            {
                rule.Check(parameter);
            }
        }

        private const BindingFlags DefaultFlags =
            BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public;

        private bool HasCheckOverloadAccepting([NotNull] [ItemNotNull] params Type[] parameterTypes)
        {
            return rule.GetType().GetMethod("Check", DefaultFlags, null, parameterTypes, new ParameterModifier[] { }) !=
                null;
        }
    }
}