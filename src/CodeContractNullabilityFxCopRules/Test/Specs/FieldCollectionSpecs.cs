using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CodeContractNullabilityFxCopRules.Test.TestDataBuilders;
using FluentAssertions;
using FxCopUnitTestRunner;
using FxCopUnitTestRunner.TestDataBuilders;
using NUnit.Framework;

namespace CodeContractNullabilityFxCopRules.Test.Specs
{
    /// <summary>
    /// Tests for reporting item nullability diagnostics on fields of collection types.
    /// </summary>
    [TestFixture]
    internal class FieldCollectionSpecs
    {
        [Test]
        public void When_field_is_annotated_with_item_not_nullable_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .WithNullabilityAttributes(new NullabilityAttributesBuilder()
                        .InCodeNamespace("N.M"))
                    .Using(typeof (IEnumerable<>).Namespace)
                    .InGlobalScope(@"
                        class C
                        {
                            [N.M.ItemNotNull] // Using fully qualified namespace
                            IEnumerable<string> f;
                        }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_field_is_annotated_with_item_nullable_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .WithNullabilityAttributes(new NullabilityAttributesBuilder()
                        .InCodeNamespace("N1"))
                    .Using(typeof (IEnumerable<>).Namespace)
                    .InGlobalScope(@"
                        namespace N2
                        {
                            using ICBN = N1.ItemCanBeNullAttribute;

                            class C
                            {
                                [ICBN()] // Using type/namespace alias
                                IEnumerable<string> f;
                            }
                        }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_field_is_constant_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .InDefaultClass(@"
                        public const string[] f = null;
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_field_item_type_is_value_type_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .Using(typeof (List<>).Namespace)
                    .InDefaultClass(@"
                        List<int> f;
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_field_item_type_is_generic_value_type_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .Using(typeof (IList<>).Namespace)
                    .InGlobalScope(@"
                        class C<T> where T : struct
                        {
                            IList<T> f;
                        }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_field_item_type_is_enum_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .WithReference(typeof (HashSet<>).Assembly)
                    .Using(typeof (HashSet<>).Namespace)
                    .Using(typeof (BindingFlags).Namespace)
                    .InDefaultClass(@"
                        HashSet<BindingFlags> f;
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_field_item_type_is_object_it_must_be_reported()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .Using(typeof (IList).Namespace)
                    .InDefaultClass(@"
                        IList f;
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.Problems.Should().HaveCount(1);
            result.Problems[0].Resolution.Name.Should().Be(ItemNullabilityRule.RuleName);
        }

        [Test]
        public void When_field_item_type_is_nullable_it_must_be_reported()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .Using(typeof (IList<>).Namespace)
                    .InDefaultClass(@"
                        IList<int?> f;
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.Problems.Should().HaveCount(1);
            result.Problems[0].Resolution.Name.Should().Be(ItemNullabilityRule.RuleName);
        }

        [Test]
        public void When_field_item_type_is_generic_nullable_it_must_be_reported()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .Using(typeof (ISet<>).Namespace)
                    .InGlobalScope(@"
                        class C<T> where T : struct
                        {
                            ISet<T?> f;
                        }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.Problems.Should().HaveCount(1);
            result.Problems[0].Resolution.Name.Should().Be(ItemNullabilityRule.RuleName);
        }

        [Test]
        public void When_field_item_type_is_reference_it_must_be_reported()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .Using(typeof (IEnumerable<>).Namespace)
                    .InDefaultClass(@"
                        IEnumerable<string> f;
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.Problems.Should().HaveCount(1);
            result.Problems[0].Resolution.Name.Should().Be(ItemNullabilityRule.RuleName);
        }

        [Test]
        public void When_field_is_compiler_generated_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .Using(typeof (IEnumerable<>).Namespace)
                    .Using(typeof (CompilerGeneratedAttribute).Namespace)
                    .InDefaultClass(@"
                        [CompilerGenerated]
                        IEnumerable<string> f;
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_field_contains_multiple_variables_they_must_be_reported()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .Using(typeof (List<>).Namespace)
                    .InDefaultClass(@"
                        List<int?> f, g;
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.Problems.Should().HaveCount(2);
            result.Problems[0].Resolution.Name.Should().Be(ItemNullabilityRule.RuleName);
            result.Problems[1].Resolution.Name.Should().Be(ItemNullabilityRule.RuleName);
        }

        [Test]
        public void When_field_type_is_lazy_it_must_be_reported()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .InDefaultClass(@"
                    Lazy<string> f;
                "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.Problems.Should().HaveCount(1);
            result.Problems[0].Resolution.Name.Should().Be(ItemNullabilityRule.RuleName);
        }

        [Test]
        public void When_field_type_is_task_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .Using(typeof (Task).Namespace)
                    .InDefaultClass(@"
                        Task f;
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_field_type_is_generic_task_it_must_be_reported()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .Using(typeof (Task<>).Namespace)
                    .InDefaultClass(@"
                        Task<string> f;
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.Problems.Should().HaveCount(1);
            result.Problems[0].Resolution.Name.Should().Be(ItemNullabilityRule.RuleName);
        }
    }
}