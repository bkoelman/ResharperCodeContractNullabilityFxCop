using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CodeContractNullabilityFxCopRules.Utilities;
using JetBrains.Annotations;
using Microsoft.FxCop.Sdk;

namespace CodeContractNullabilityFxCopRules.SymbolAnalysis
{
    /// <summary>
    /// Gets the full name of a symbol, in the format used for XML Documentation Comment files.
    /// </summary>
    /// <remarks>
    /// Based on source code in: <seealso href="http://www.binarycoder.net/fxcop/pdf/fxcop.pdf" />
    /// </remarks>
    public static class DocumentationCommentFactory
    {
        [NotNull]
        public static string GetDocumentationCommentId([NotNull] Member member)
        {
            Guard.NotNull(member, "member");

            var textBuilder = new StringBuilder();
            char prefixCharacter = DeterminePrefixCharacter(member);
            TypeNode declaringType = member.DeclaringType;

            // Determine all parent types of this potentially nested type.
            IEnumerable<TypeNode> parentTypes = DetermineParentTypesIncludingCurrent(declaringType).ToArray();

            List<TypeNode> typeTemplateParameters = CollectTypeTemplateParameters(parentTypes);
            List<TypeNode> typeTemplateArguments = CollectTypeTemplateArguments(parentTypes);
            List<TypeNode> memberTemplateParameters = CollectMethodTemplateParameters(member);

            var context = new WriteContext(typeTemplateParameters, typeTemplateArguments, memberTemplateParameters,
                textBuilder);

            OutputFullMethodName(context, member, prefixCharacter, declaringType);
            OutputMethodTemplateParameterCount(context);
            OutputParameters(context, member);
            OutputConversionOperatorReturnType(context, member);

            return textBuilder.ToString();
        }

        private static char DeterminePrefixCharacter([NotNull] Member member)
        {
            switch (member.NodeType)
            {
                case NodeType.Class:
                case NodeType.Interface:
                case NodeType.Struct:
                case NodeType.EnumNode:
                case NodeType.DelegateNode:
                    return 'T';
                case NodeType.Field:
                    return 'F';
                case NodeType.Property:
                    return 'P';
                case NodeType.Method:
                case NodeType.InstanceInitializer:
                case NodeType.StaticInitializer:
                    return 'M';
                case NodeType.Event:
                    return 'E';
                default:
                    throw new NotSupportedException(string.Format("Unsupported NodeType '{0}'.", member.NodeType));
            }
        }

        [NotNull]
        [ItemNotNull]
        private static IEnumerable<TypeNode> DetermineParentTypesIncludingCurrent([NotNull] TypeNode type)
        {
            var parentTypes = new List<TypeNode>();

            for (TypeNode currentType = type; currentType != null; currentType = currentType.DeclaringType)
            {
                parentTypes.Add(currentType);
            }

            parentTypes.Reverse();
            return parentTypes;
        }

        [NotNull]
        [ItemNotNull]
        private static List<TypeNode> CollectTypeTemplateParameters(
            [NotNull] [ItemNotNull] IEnumerable<TypeNode> parentTypes)
        {
            var typeTemplateParameters = new List<TypeNode>();
            foreach (TypeNode type in parentTypes)
            {
                if (type.TemplateParameters != null)
                {
                    typeTemplateParameters.AddRange(type.TemplateParameters);
                }
            }
            return typeTemplateParameters;
        }

        [NotNull]
        [ItemNotNull]
        private static List<TypeNode> CollectTypeTemplateArguments(
            [NotNull] [ItemNotNull] IEnumerable<TypeNode> parentTypes)
        {
            var typeTemplateArguments = new List<TypeNode>();
            foreach (TypeNode type in parentTypes)
            {
                if (type.TemplateArguments != null)
                {
                    typeTemplateArguments.AddRange(type.TemplateArguments);
                }
            }
            return typeTemplateArguments;
        }

        [NotNull]
        [ItemNotNull]
        private static List<TypeNode> CollectMethodTemplateParameters([NotNull] Member member)
        {
            var memberTemplateParameters = new List<TypeNode>();
            switch (member.NodeType)
            {
                case NodeType.Method:
                case NodeType.InstanceInitializer:
                case NodeType.StaticInitializer:
                    var method = (Method) member;
                    if (method.TemplateParameters != null)
                    {
                        memberTemplateParameters.AddRange(method.TemplateParameters);
                    }
                    break;
            }
            return memberTemplateParameters;
        }

        private static void OutputFullMethodName([NotNull] WriteContext context, [NotNull] Member member,
            char prefixCharacter, [CanBeNull] TypeNode declaringType)
        {
            context.TextBuilder.Append(prefixCharacter);
            context.TextBuilder.Append(':');
            if (declaringType == null)
            {
                var type = member as TypeNode;
                if (type != null)
                {
                    if (type.Namespace.Name.Length != 0)
                    {
                        context.TextBuilder.Append(type.Namespace.Name);
                        context.TextBuilder.Append('.');
                    }
                }
            }
            else
            {
                WriteOrdinaryType(context, declaringType, true);
                context.TextBuilder.Append('.');
            }
            context.TextBuilder.Append(member.Name.Name.Replace('.', '#'));
        }

        private static void OutputMethodTemplateParameterCount([NotNull] WriteContext context)
        {
            if (context.MemberTemplateParameters.Count != 0)
            {
                // Undocumented: based on output from MS compilers.
                context.TextBuilder.AppendFormat(CultureInfo.InvariantCulture, "``{0}",
                    context.MemberTemplateParameters.Count);
            }
        }

        private static void OutputParameters([NotNull] WriteContext context, [NotNull] Member member)
        {
            ParameterCollection parameters = TryGetParameters(member);

            if (parameters != null && parameters.Count != 0)
            {
                context.TextBuilder.Append('(');

                bool includeComma = false;
                foreach (Parameter parameter in parameters)
                {
                    if (includeComma)
                    {
                        context.TextBuilder.Append(',');
                    }

                    AppendTextForTypeNode(context, parameter.Type);
                    includeComma = true;
                }

                context.TextBuilder.Append(')');
            }
        }

        [CanBeNull]
        private static ParameterCollection TryGetParameters([NotNull] Member member)
        {
            switch (member.NodeType)
            {
                case NodeType.Property:
                    return ((PropertyNode) member).Parameters;
                case NodeType.Method:
                case NodeType.InstanceInitializer:
                case NodeType.StaticInitializer:
                    return ((Method) member).Parameters;
                default:
                    return null;
            }
        }

        private static void AppendTextForTypeNode([NotNull] WriteContext context, [NotNull] TypeNode type)
        {
            switch (type.NodeType)
            {
                case NodeType.Class:
                case NodeType.Interface:
                case NodeType.Struct:
                case NodeType.EnumNode:
                case NodeType.DelegateNode:
                    WriteOrdinaryType(context, type, false);
                    break;
                case NodeType.Reference:
                    WriteReferenceType(context, type);
                    break;
                case NodeType.Pointer:
                    WritePointerType(context, type);
                    break;
                case NodeType.ClassParameter:
                case NodeType.TypeParameter:
                    WriteGenericParameter(context, type);
                    break;
                case NodeType.ArrayType:
                    WriteArray(context, type);
                    break;
                case NodeType.FunctionPointer:
                    WriteCppCliTypes(context, type);
                    break;
                case NodeType.RequiredModifier:
                    WriteRequiredModifier(context, type);
                    break;
                case NodeType.OptionalModifier:
                    WriteOptionalModifier(context, type);
                    break;
                default:
                    throw new NotSupportedException(string.Format("Unsupported NodeType '{0}'.", type.NodeType));
            }

            if (type.IsGeneric && type.TemplateArguments != null && type.TemplateArguments.Count != 0)
            {
                // Undocumented: based on output from MS compilers.
                context.TextBuilder.Append('{');
                bool comma = false;
                foreach (TypeNode templateArgumentType in type.TemplateArguments)
                {
                    if (comma)
                    {
                        context.TextBuilder.Append(',');
                    }
                    AppendTextForTypeNode(context, templateArgumentType);
                    comma = true;
                }
                context.TextBuilder.Append('}');
            }
        }

        private static void WriteOrdinaryType([NotNull] WriteContext context, [NotNull] TypeNode type, bool includeArity)
        {
            if (type.DeclaringType == null)
            {
                if (type.Namespace.Name.Length != 0)
                {
                    context.TextBuilder.Append(type.Namespace.Name);
                    context.TextBuilder.Append('.');
                }
            }
            else
            {
                AppendTextForTypeNode(context, type.DeclaringType);
                context.TextBuilder.Append('.');
            }

            if (type.IsGeneric && type.Template != null)
            {
                string templateName = type.Template.Name.Name.Replace('+', '.');
                int pos = includeArity ? -1 : templateName.LastIndexOf('`');
                context.TextBuilder.Append(pos != -1 ? templateName.Substring(0, pos) : templateName);
            }
            else
            {
                context.TextBuilder.Append(type.Name.Name.Replace('+', '.'));
            }
        }

        private static void WriteReferenceType([NotNull] WriteContext context, [NotNull] TypeNode type)
        {
            AppendTextForTypeNode(context, ((Reference) type).ElementType);
            context.TextBuilder.Append('@');
        }

        private static void WritePointerType([NotNull] WriteContext context, [NotNull] TypeNode type)
        {
            AppendTextForTypeNode(context, ((Pointer) type).ElementType);
            context.TextBuilder.Append('*');
        }

        private static void WriteGenericParameter([NotNull] WriteContext context, [NotNull] TypeNode type)
        {
            int index;
            if ((index = context.TypeTemplateParameters.IndexOf(type)) != -1)
            {
                context.TextBuilder.AppendFormat(CultureInfo.InvariantCulture, "`{0}", index);
            }
            else if ((index = context.MemberTemplateParameters.IndexOf(type)) != -1)
            {
                // Undocumented: based on output from MS compilers.
                context.TextBuilder.AppendFormat(CultureInfo.InvariantCulture, "``{0}", index);
            }
            else if ((index = context.TypeTemplateArguments.IndexOf(type)) != -1)
            {
                context.TextBuilder.AppendFormat(CultureInfo.InvariantCulture, "`{0}", index);
            }
            else
            {
                throw new InvalidOperationException("Unable to resolve TypeParameter to a type argument.");
            }
        }

        private static void WriteArray([NotNull] WriteContext context, [NotNull] TypeNode type)
        {
            var array = ((ArrayType) type);
            AppendTextForTypeNode(context, array.ElementType);
            if (array.IsSzArray())
            {
                context.TextBuilder.Append("[]");
            }
            else
            {
                // This case handles true multidimensional arrays.
                // For example, in C#: string[,] myArray
                context.TextBuilder.Append('[');
                for (int i = 0; i < array.Rank; i++)
                {
                    if (i != 0)
                    {
                        context.TextBuilder.Append(',');
                    }
                    // The following appears to be consistent with MS C# compiler output.
                    context.TextBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}:", array.GetLowerBound(i));
                    if (array.GetSize(i) != 0)
                    {
                        context.TextBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}", array.GetSize(i));
                    }
                }
                context.TextBuilder.Append(']');
            }
        }

        private static void WriteCppCliTypes([NotNull] WriteContext context, [NotNull] TypeNode type)
        {
            var functionPointer = (FunctionPointer) type;
            context.TextBuilder.Append("=FUNC:");
            AppendTextForTypeNode(context, functionPointer.ReturnType);
            if (functionPointer.ParameterTypes.Count != 0)
            {
                bool comma = false;
                context.TextBuilder.Append('(');
                foreach (TypeNode parameterType in functionPointer.ParameterTypes)
                {
                    if (comma)
                    {
                        context.TextBuilder.Append(',');
                    }
                    AppendTextForTypeNode(context, parameterType);
                    comma = true;
                }
                context.TextBuilder.Append(')');
            }
            else
            {
                // Inconsistent with documentation: based on MS C++ compiler output.
                context.TextBuilder.Append("(System.Void)");
            }
        }

        private static void WriteRequiredModifier([NotNull] WriteContext context, [NotNull] TypeNode type)
        {
            var requiredModifier = (RequiredModifier) type;
            AppendTextForTypeNode(context, requiredModifier.ModifiedType);
            context.TextBuilder.Append("|");
            AppendTextForTypeNode(context, requiredModifier.Modifier);
        }

        private static void WriteOptionalModifier([NotNull] WriteContext context, [NotNull] TypeNode type)
        {
            var optionalModifier = (OptionalModifier) type;
            AppendTextForTypeNode(context, optionalModifier.ModifiedType);
            context.TextBuilder.Append("!");
            AppendTextForTypeNode(context, optionalModifier.Modifier);
        }

        private static void OutputConversionOperatorReturnType([NotNull] WriteContext context, [NotNull] Member member)
        {
            if (member.NodeType == NodeType.Method && member.IsSpecialName &&
                (member.Name.Name == "op_Explicit" || member.Name.Name == "op_Implicit"))
            {
                var convOperator = (Method) member;
                context.TextBuilder.Append('~');
                AppendTextForTypeNode(context, convOperator.ReturnType);
            }
        }

        private sealed class WriteContext
        {
            [NotNull]
            [ItemNotNull]
            public List<TypeNode> TypeTemplateParameters { get; private set; }

            [NotNull]
            [ItemNotNull]
            public List<TypeNode> TypeTemplateArguments { get; private set; }

            [NotNull]
            [ItemNotNull]
            public List<TypeNode> MemberTemplateParameters { get; private set; }

            [NotNull]
            public StringBuilder TextBuilder { get; private set; }

            public WriteContext([NotNull] [ItemNotNull] List<TypeNode> typeTemplateParameters,
                [NotNull] [ItemNotNull] List<TypeNode> typeTemplateArguments,
                [NotNull] [ItemNotNull] List<TypeNode> memberTemplateParameters, [NotNull] StringBuilder textBuilder)
            {
                TypeTemplateParameters = typeTemplateParameters;
                TypeTemplateArguments = typeTemplateArguments;
                MemberTemplateParameters = memberTemplateParameters;
                TextBuilder = textBuilder;
            }
        }
    }
}