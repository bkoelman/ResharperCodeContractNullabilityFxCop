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
    /// Tests for reporting item nullability diagnostics on method parameters.
    /// </summary>
    [TestFixture]
    internal class ParameterCollectionSpecs
    {
        [Test]
        public void When_parameter_is_annotated_with_item_not_nullable_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .Using(typeof (IEnumerable<>).Namespace)
                    .InGlobalScope(@"
                        class C
                        {
                            void M([ItemNotNull] IEnumerable<string> p) { }
                        }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_parameter_is_annotated_with_item_nullable_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .Using(typeof (IEnumerable<>).Namespace)
                    .InGlobalScope(@"
                        class C
                        {
                            void M([ItemCanBeNull] IEnumerable<string> p) { }
                        }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_parameter_item_type_is_value_type_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .Using(typeof (List<>).Namespace)
                    .InDefaultClass(@"
                        public void M(List<int> p) { }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_parameter_item_type_is_generic_value_type_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .Using(typeof (IEnumerable<>).Namespace)
                    .InGlobalScope(@"
                        class C<T> where T : struct
                        {
                            void M(IEnumerable<T> p) { }
                        }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_parameter_item_type_is_enum_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .Using(typeof (IEnumerable<>).Namespace)
                    .Using(typeof (BindingFlags).Namespace)
                    .InDefaultClass(@"
                        void M(IEnumerable<BindingFlags> p) { }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_parameter_item_type_is_nullable_it_must_be_reported()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .Using(typeof (IEnumerable<>).Namespace)
                    .InDefaultClass(@"
                        void M(IEnumerable<int?> p) { }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.Problems.Should().HaveCount(1);
            result.Problems[0].Resolution.Name.Should().Be(ItemNullabilityRule.RuleName);
        }

        [Test]
        public void When_parameter_item_type_is_generic_nullable_it_must_be_reported()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .Using(typeof (IEnumerable<>).Namespace)
                    .InGlobalScope(@"
                        class C<T> where T : struct
                        {
                            void M(IEnumerable<T?> p) { }
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
        public void When_parameter_item_type_is_reference_it_must_be_reported()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .Using(typeof (IEnumerable<>).Namespace)
                    .InDefaultClass(@"
                        void M(IEnumerable<string> p) { }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.Problems.Should().HaveCount(1);
            result.Problems[0].Resolution.Name.Should().Be(ItemNullabilityRule.RuleName);
        }

        [Test]
        public void When_parameter_item_type_is_object_it_must_be_reported()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .Using(typeof (IEnumerable).Namespace)
                    .InDefaultClass(@"
                        void M(IEnumerable p) { }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.Problems.Should().HaveCount(1);
            result.Problems[0].Resolution.Name.Should().Be(ItemNullabilityRule.RuleName);
        }

        [Test]
        public void When_indexer_parameter_is_collection_of_reference_type_it_must_be_reported()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .Using(typeof (IEnumerable<>).Namespace)
                    .InDefaultClass(@"
                        int this[IEnumerable<string> p]
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
        public void When_ref_parameter_is_collection_of_nullable_it_must_be_reported()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .Using(typeof (IEnumerable<>).Namespace)
                    .InDefaultClass(@"
                        void M(ref IEnumerable<int?> p) { }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.Problems.Should().HaveCount(1);
            result.Problems[0].Resolution.Name.Should().Be(ItemNullabilityRule.RuleName);
        }

        [Test]
        public void When_ref_parameter_is_collection_of_value_type_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .Using(typeof (IEnumerable<>).Namespace)
                    .InDefaultClass(@"
                        void M(ref IEnumerable<int> p) { }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_out_parameter_is_collection_of_nullable_it_must_be_reported()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .Using(typeof (IEnumerable<>).Namespace)
                    .InDefaultClass(@"
                        void M(out IEnumerable<int?> p) { throw new NotImplementedException(); }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.Problems.Should().HaveCount(1);
            result.Problems[0].Resolution.Name.Should().Be(ItemNullabilityRule.RuleName);
        }

        [Test]
        public void When_out_parameter_is_collection_of_value_type_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .Using(typeof (IList<>).Namespace)
                    .InDefaultClass(@"
                        void M(out IList<int> p) { throw new NotImplementedException(); }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_parameter_is_compiler_generated_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .Using(typeof (IEnumerable<>).Namespace)
                    .Using(typeof (CompilerGeneratedAttribute).Namespace)
                    .InDefaultClass(@"
                        void M([CompilerGenerated] IEnumerable<string> p) { }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_parameter_in_base_class_is_annotated_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .Using(typeof (IEnumerable<>).Namespace)
                    .InGlobalScope(@"
                        class B
                        {
                            public virtual void M([ItemNotNull] IEnumerable<string> p) { }
                        }

                        class D1 : B { }

                        class D2 : D1
                        {
                            // implicitly inherits decoration from base class
                            public override void M(IEnumerable<string> p) { }
                        }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_indexer_parameter_in_base_class_is_annotated_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .Using(typeof (IEnumerable<>).Namespace)
                    .InGlobalScope(@"
                        abstract class B
                        {
                            public abstract int this[[ItemNotNull] IEnumerable<string> p] { get; set; }
                        }

                        abstract class D1 : B { }

                        class D2 : D1
                        {
                            // implicitly inherits decoration from base class
                            public override int this[IEnumerable<string> p]
                            {
                                get { throw new NotImplementedException(); }
                                set { throw new NotImplementedException(); }
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
        public void When_parameter_in_base_constructor_is_annotated_it_must_be_reported()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .Using(typeof (IEnumerable<>).Namespace)
                    .InGlobalScope(@"
                        class B
                        {
                            protected B([ItemNotNull] IEnumerable<string> p) { }
                        }

                        class D : B
                        {
                            public D(IEnumerable<string> p) : base(p) { }
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
        public void When_parameter_in_implicit_interface_implementation_is_annotated_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .Using(typeof (IEnumerable<>).Namespace)
                    .InGlobalScope(@"
                        interface I
                        {
                            void M([ItemCanBeNull] IEnumerable<string> p);
                        }

                        class C : I
                        {
                            // implicitly inherits decoration from interface
                            public void M(IEnumerable<string> p) { }

                            // unrelated overload
                            public void M([ItemCanBeNull] IEnumerable<object> p) { }
                        }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void
            When_parameter_in_explicit_interface_implementation_is_effectively_annotated_through_annotation_on_interface_it_must_be_skipped
            ()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .Using(typeof (IEnumerable<>).Namespace)
                    .InGlobalScope(@"
                        interface I
                        {
                            void M([ItemCanBeNull] IEnumerable<string> p);
                        }

                        class C : I
                        {
                            // implicitly inherits decoration from interface
                            void I.M(IEnumerable<string> p) { }
                        }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_parameter_in_implicit_interface_implementation_is_not_annotated_it_must_be_reported()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .Using(typeof (IEnumerable<>).Namespace)
                    .InGlobalScope(@"
                        interface I
                        {
                            void M([ItemNotNull] IEnumerable<string> p);
                        }

                        class C : I
                        {
                            // implicitly inherits decoration from interface
                            void I.M(IEnumerable<string> p) { }

                            // requires explicit decoration
                            public void M(IEnumerable<string> p) { }

                            // unrelated overload
                            public void M([ItemNotNull] IEnumerable<object> p) { }
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
        public void
            When_parameter_in_implicit_interface_implementation_is_effectively_annotated_through_annotation_on_interface_it_must_be_skipped
            ()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .Using(typeof (IEnumerable<>).Namespace)
                    .InGlobalScope(@"
                        interface I
                        {
                            void M([ItemNotNull] IEnumerable<string> p);
                        }

                        class C : I
                        {
                            // implicitly inherits decoration from interface
                            void I.M(IEnumerable<string> p) { }

                            // requires explicit decoration
                            public void M([ItemNotNull] IEnumerable<string> p) { }

                            // unrelated overload
                            public void M([ItemNotNull] IEnumerable<object> p) { }
                        }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_parameter_type_is_lazy_it_must_be_reported()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .InDefaultClass(@"
                    void M(Lazy<string> p) { }
                "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.Problems.Should().HaveCount(1);
            result.Problems[0].Resolution.Name.Should().Be(ItemNullabilityRule.RuleName);
        }

        [Test]
        public void When_parameter_type_is_task_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .Using(typeof (Task).Namespace)
                    .InDefaultClass(@"
                        public void M(Task p) { }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_parameter_type_is_generic_task_it_must_be_reported()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .Using(typeof (Task<>).Namespace)
                    .InDefaultClass(@"
                        void M(Task<string> p) { }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.Problems.Should().HaveCount(1);
            result.Problems[0].Resolution.Name.Should().Be(ItemNullabilityRule.RuleName);
        }

        [Test]
        public void When_base_parameter_inherits_item_annotation_from_interface_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .Using(typeof (List<>).Namespace)
                    .InGlobalScope(@"
                        namespace N
                        {
                            public interface I
                            {
                                void M([ItemNotNull] List<string> p);
                            }

                            public class B : I
                            {
                                public virtual void M(List<string> p) { }
                            }

                            public class C : B
                            {
                                public override void M(List<string> p) { }
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
                            public virtual void M([ItemNotNull] IEnumerable<int?> p) { throw new NotImplementedException(); }
                        }

                        public class C : B
                        {
                            public new void M(IEnumerable<int?> p) { throw new NotImplementedException(); }
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