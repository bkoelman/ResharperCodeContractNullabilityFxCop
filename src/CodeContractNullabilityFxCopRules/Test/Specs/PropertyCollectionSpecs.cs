﻿using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Tests for reporting item nullability diagnostics on properties.
    /// </summary>
    [TestFixture]
    internal class PropertyCollectionSpecs
    {
        [Test]
        public void When_property_is_annotated_with_item_not_nullable_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .Using(typeof (IEnumerable<>).Namespace)
                    .InGlobalScope(@"
                        class C
                        {
                            [ItemNotNull]
                            IEnumerable<string> P { get; set; }
                        }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_property_is_annotated_with_item_nullable_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .Using(typeof (IEnumerable<>).Namespace)
                    .InGlobalScope(@"
                        class C
                        {
                            [ItemCanBeNull]
                            IEnumerable<string> P { get; set; }
                        }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_property_item_type_is_value_type_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .Using(typeof (IList<>).Namespace)
                    .InDefaultClass(@"
                        IList<int> P { get; set; }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_property_item_type_is_generic_value_type_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .Using(typeof (IEnumerable<>).Namespace)
                    .InGlobalScope(@"
                        class C<T> where T : struct
                        {
                            IEnumerable<T> P { get; set; }
                        }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_property_item_type_is_enum_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .Using(typeof (IEnumerable<>).Namespace)
                    .Using(typeof (BindingFlags).Namespace)
                    .InDefaultClass(@"
                        IEnumerable<BindingFlags> P { get; set; }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_property_item_type_is_nullable_it_must_be_reported()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .Using(typeof (IEnumerable<>).Namespace)
                    .InDefaultClass(@"
                        IEnumerable<int?> P { get; set; }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.Problems.Should().HaveCount(1);
            result.Problems[0].Resolution.Name.Should().Be(ItemNullabilityRule.RuleName);
        }

        [Test]
        public void When_property_item_type_is_generic_nullable_it_must_be_reported()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .Using(typeof (IEnumerable<>).Namespace)
                    .InGlobalScope(@"
                        class C<T> where T : struct
                        {
                            IEnumerable<T?> P { get; set; }
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
        public void When_property_item_type_is_reference_it_must_be_reported()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .Using(typeof (IEnumerable<>).Namespace)
                    .InDefaultClass(@"
                        IEnumerable<string> P { get; set; }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.Problems.Should().HaveCount(1);
            result.Problems[0].Resolution.Name.Should().Be(ItemNullabilityRule.RuleName);
        }

        [Test]
        public void When_property_item_type_is_object_it_must_be_reported()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .Using(typeof (ArrayList).Namespace)
                    .InDefaultClass(@"
                        ArrayList P { get; set; }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.Problems.Should().HaveCount(1);
            result.Problems[0].Resolution.Name.Should().Be(ItemNullabilityRule.RuleName);
        }

        [Test]
        public void When_property_is_compiler_generated_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .Using(typeof (IEnumerable<>).Namespace)
                    .Using(typeof (CompilerGeneratedAttribute).Namespace)
                    .InDefaultClass(@"
                        [CompilerGenerated]
                        IEnumerable<string> P { get; set; }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_property_is_not_debuggable_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .Using(typeof (IEnumerable<>).Namespace)
                    .Using(typeof (DebuggerNonUserCodeAttribute).Namespace)
                    .InDefaultClass(@"
                        [DebuggerNonUserCode]
                        IEnumerable<string> P { get; set; }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_indexer_property_type_is_collection_of_reference_type_it_must_be_reported()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .Using(typeof (IEnumerable<>).Namespace)
                    .InDefaultClass(@"
                        IEnumerable<int?> this[int p]
                        {
                            get { throw new NotImplementedException(); }
                            set { throw new NotImplementedException(); }
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
        public void When_property_in_base_class_is_annotated_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .Using(typeof (IEnumerable<>).Namespace)
                    .InGlobalScope(@"
                        class B
                        {
                            [ItemNotNull]
                            public virtual IEnumerable<string> P { get; set; }
                        }

                        class D1 : B { }

                        class D2 : D1
                        {
                            // implicitly inherits decoration from base class
                            public override IEnumerable<string> P { get; set; }
                        }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_property_in_implicit_interface_is_annotated_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .Using(typeof (IList<>).Namespace)
                    .InGlobalScope(@"
                        interface I
                        {
                            [ItemCanBeNull]
                            IList<string> P { get; set; }
                        }

                        class C : I
                        {
                            // implicitly inherits decoration from interface
                            public IList<string> P { get; set; }
                        }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_property_in_explicit_interface_is_annotated_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .Using(typeof (IEnumerable<>).Namespace)
                    .InGlobalScope(@"
                        interface I
                        {
                            [ItemCanBeNull]
                            IEnumerable<string> P { get; set; }
                        }

                        class C : I
                        {
                            // implicitly inherits decoration from interface
                            IEnumerable<string> I.P { get; set; }
                        }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_property_in_implicit_interface_is_not_annotated_with_explicit_interface_it_must_be_reported()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .Using(typeof (IEnumerable<>).Namespace)
                    .InGlobalScope(@"
                        interface I
                        {
                            [ItemNotNull]
                            IEnumerable<string> P { get; set; }
                        }

                        class C : I
                        {
                            // implicitly inherits decoration from interface
                            IEnumerable<string> I.P { get; set; }

                            // requires explicit decoration
                            public IEnumerable<string> P { get; set; }
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
        public void When_indexer_property_type_in_implicit_interface_is_not_annotated_it_must_be_reported()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .Using(typeof (IEnumerable<>).Namespace)
                    .InGlobalScope(@"
                        namespace N
                        {
                            interface I
                            {
                                [ItemCanBeNull]
                                IEnumerable<int?> this[char p] { get; set; }
                            }

                            class C : I
                            {
                                // implicitly inherits decoration from interface
                                IEnumerable<int?> I.this[char p]
                                {
                                    get { throw new NotImplementedException(); }
                                    set { throw new NotImplementedException(); }
                                }

                                public IEnumerable<int?> this[char p]
                                {
                                    get { throw new NotImplementedException(); }
                                    set { throw new NotImplementedException(); }
                                }
                            }
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
        public void When_property_type_is_lazy_it_must_be_reported()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .InDefaultClass(@"
                    Lazy<string> P { get; set; }
                "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.Problems.Should().HaveCount(1);
            result.Problems[0].Resolution.Name.Should().Be(ItemNullabilityRule.RuleName);
        }

        [Test]
        public void When_property_type_is_task_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .Using(typeof (Task).Namespace)
                    .InDefaultClass(@"
                        public Task P { get; set; }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_property_type_is_generic_task_it_must_be_reported()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .Using(typeof (Task<>).Namespace)
                    .InDefaultClass(@"
                        Task<string> P { get; set; }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.Problems.Should().HaveCount(1);
            result.Problems[0].Resolution.Name.Should().Be(ItemNullabilityRule.RuleName);
        }

        [Test]
        public void When_base_property_inherits_item_annotation_from_interface_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .Using(typeof (IEnumerable).Namespace)
                    .InGlobalScope(@"
                        namespace N
                        {
                            public interface I
                            {
                                [ItemNotNull]
                                IEnumerable P { get; set; }
                            }

                            public class B : I
                            {
                                public virtual IEnumerable P { get; set; }
                            }

                            public class C : B
                            {
                                public override IEnumerable P { get; set; }
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
        public void When_override_breaks_inheritance_it_must_be_reported()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .Using(typeof (IEnumerable<>).Namespace)
                    .InGlobalScope(@"
                    namespace N
                    {
                        public class B
                        {
                            [ItemNotNull]
                            public virtual IEnumerable<int?> P { get; set; }
                        }

                        public class C : B
                        {
                            public new IEnumerable<int?> P { get; set; }
                        }
                    }
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