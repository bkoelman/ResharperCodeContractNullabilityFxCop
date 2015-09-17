using CodeContractNullabilityFxCopRules.Test.TestDataBuilders;
using FluentAssertions;
using FxCopUnitTestRunner;
using NUnit.Framework;

namespace CodeContractNullabilityFxCopRules.Test.Specs
{
    [TestFixture]
    internal class Bugfixes
    {
        [Test]
        public void When_type_parameter_is_inherited_it_must_be_resolved_without_error()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<NullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .InGlobalScope(@"
                        public class B<T>
                        {
                            [NotNull]
                            public virtual T M(/* missing annotation */ T p)
                            {
                                throw new NotImplementedException();
                            }
                        }

                        public class D<T> : B<T>
                        {
                            [NotNull]
                            public override T M(/* missing annotation */ T p) // Should not throw 'Unable to resolve TypeParameter to a type argument.'
                            {
                                throw new NotImplementedException();
                            }
                        }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.Problems.Should().HaveCount(2);
        }

        [Test]
        [GitHubIssue(1)]
        public void
            When_deriving_constructed_arrays_from_externally_annotated_interface_with_open_array_types_it_must_be_skipped
            ()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<NullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .InGlobalScope(@"
                        public interface I<T>
                        {
                            T[] P { get; }

                            T[] M(T[] p, int i);
                        }

                        public class C : I<string>
                        {
                            public string[] P { get { throw new NotImplementedException(); } }

                            public string[] M(string[] p, int i) { throw new NotImplementedException(); }
                        }
                    "))
                .ExternallyAnnotated(new ExternalAnnotationsBuilder()
                    .IncludingMember(new ExternalAnnotationFragmentBuilder()
                        .Named("P:I`1.P")
                        .NotNull())
                    .IncludingMember(new ExternalAnnotationFragmentBuilder()
                        .Named("M:I`1.M(`0[],System.Int32)")
                        .NotNull()
                        .WithParameter(new ExternalAnnotationParameterBuilder()
                            .Named("p")
                            .NotNull())))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        [GitHubIssue(1)]
        public void
            When_deriving_constructed_arrays_from_externally_annotated_class_with_open_array_types_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<NullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .InGlobalScope(@"
                        public abstract class B<T>
                        {
                            public abstract T[] P { get; }

                            public abstract T[] M(T[] p, int i);
                        }

                        public class D : B<string>
                        {
                            public override string[] P { get { throw new NotImplementedException(); } }

                            public override string[] M(string[] p, int i) { throw new NotImplementedException(); }
                        }
                    "))
                .ExternallyAnnotated(new ExternalAnnotationsBuilder()
                    .IncludingMember(new ExternalAnnotationFragmentBuilder()
                        .Named("P:B`1.P")
                        .NotNull())
                    .IncludingMember(new ExternalAnnotationFragmentBuilder()
                        .Named("M:B`1.M(`0[],System.Int32)")
                        .NotNull()
                        .WithParameter(new ExternalAnnotationParameterBuilder()
                            .Named("p")
                            .NotNull())))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        [GitHubIssue(2)]
        public void When_indexer_has_multiple_parameters_it_must_be_reported()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<NullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .InGlobalScope(@"
                        class C
                        {
                            public int this[string a, string b, string c]
                            {
                                get { throw new NotImplementedException();}
                                set { throw new NotImplementedException();}
                            }
                        }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.Problems.Should().HaveCount(3);
        }

        [Test]
        [GitHubIssue(3)]
        public void When_array_item_type_is_value_type_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<ItemNullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .InGlobalScope(@"
                        class C
                        {
                            byte[] f;
                        }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }
    }
}