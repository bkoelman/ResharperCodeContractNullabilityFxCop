using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using CodeContractNullabilityFxCopRules.Test.TestDataBuilders;
using FluentAssertions;
using FxCopUnitTestRunner;
using FxCopUnitTestRunner.TestDataBuilders;
using NUnit.Framework;

namespace CodeContractNullabilityFxCopRules.Test.Specs
{
    /// <summary>
    /// Tests for reporting nullability diagnostics on properties.
    /// </summary>
    [TestFixture]
    internal class PropertySpecs
    {
        [Test]
        public void When_property_is_annotated_with_not_nullable_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<NullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .InGlobalScope(@"
                        class C
                        {
                            [NotNull]
                            string P { get; set; }
                        }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_property_is_annotated_with_nullable_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<NullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .InGlobalScope(@"
                        class C
                        {
                            [CanBeNull]
                            string P { get; set; }
                        }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_property_is_externally_annotated_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<NullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .InGlobalScope(@"
                        namespace N
                        {
                            class C
                            {
                                string P { get; set; }
                            }
                        }
                    "))
                .ExternallyAnnotated(new ExternalAnnotationsBuilder()
                    .IncludingMember(new ExternalAnnotationFragmentBuilder()
                        .Named("P:N.C.P")
                        .NotNull()))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_property_type_is_value_type_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<NullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .InDefaultClass(@"
                        int P { get; set; }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_property_type_is_generic_value_type_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<NullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .InGlobalScope(@"
                        class C<T> where T : struct
                        {
                            T P { get; set; }
                        }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_property_type_is_enum_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<NullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .Using(typeof (BindingFlags).Namespace)
                    .InDefaultClass(@"
                        BindingFlags P { get; set; }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_property_type_is_nullable_it_must_be_reported()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<NullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .InDefaultClass(@"
                        int? P { get; set; }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.Problems.Should().HaveCount(1);
            result.Problems[0].Resolution.Name.Should().Be(NullabilityRule.RuleName);
        }

        [Test]
        public void When_property_type_is_generic_nullable_it_must_be_reported()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<NullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .InGlobalScope(@"
                        class C<T> where T : struct
                        {
                            T? P { get; set; }
                        }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.Problems.Should().HaveCount(1);
            result.Problems[0].Resolution.Name.Should().Be(NullabilityRule.RuleName);
        }

        [Test]
        public void When_property_type_is_reference_it_must_be_reported()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<NullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .InDefaultClass(@"
                        string P { get; set; }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.Problems.Should().HaveCount(1);
            result.Problems[0].Resolution.Name.Should().Be(NullabilityRule.RuleName);
        }

        [Test]
        public void When_property_is_compiler_generated_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<NullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .Using(typeof (CompilerGeneratedAttribute).Namespace)
                    .InDefaultClass(@"
                        [CompilerGenerated]
                        string P { get; set; }
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
                .ForRule<NullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .Using(typeof (DebuggerNonUserCodeAttribute).Namespace)
                    .InDefaultClass(@"
                        [DebuggerNonUserCode]
                        string P { get; set; }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_indexer_property_type_is_reference_type_it_must_be_reported_and_fixed()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<NullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .InDefaultClass(@"
                        int? this[int p]
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
            result.Problems[0].Resolution.Name.Should().Be(NullabilityRule.RuleName);
        }

        [Test]
        public void When_property_in_base_class_is_annotated_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<NullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .InGlobalScope(@"
                        class B
                        {
                            [NotNull]
                            public virtual string P { get; set; }
                        }

                        class D1 : B { }

                        class D2 : D1
                        {
                            // implicitly inherits decoration from base class
                            public override string P { get; set; }
                        }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_property_in_base_class_is_externally_annotated_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<NullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .InGlobalScope(@"
                        namespace N
                        {
                            class B
                            {
                                public virtual string P { get; set; }
                            }

                            class D1 : B { }

                            class D2 : D1
                            {
                                // implicitly inherits decoration from base class
                                public override string P { get; set; }
                            }
                        }
                    "))
                .ExternallyAnnotated(new ExternalAnnotationsBuilder()
                    .IncludingMember(new ExternalAnnotationFragmentBuilder()
                        .Named("P:N.B.P")
                        .NotNull()))
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
                .ForRule<NullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .InGlobalScope(@"
                        interface I
                        {
                            [CanBeNull]
                            string P { get; set; }
                        }

                        class C : I
                        {
                            // implicitly inherits decoration from interface
                            public string P { get; set; }
                        }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_property_in_implicit_interface_is_externally_annotated_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<NullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .InGlobalScope(@"
                        namespace N
                        {
                            interface I
                            {
                                string P { get; set; }
                            }

                            class C : I
                            {
                                // implicitly inherits decoration from interface
                                public string P { get; set; }
                            }
                        }
                    "))
                .ExternallyAnnotated(new ExternalAnnotationsBuilder()
                    .IncludingMember(new ExternalAnnotationFragmentBuilder()
                        .Named("P:N.I.P")
                        .CanBeNull()))
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
                .ForRule<NullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .InGlobalScope(@"
                        interface I
                        {
                            [CanBeNull]
                            string P { get; set; }
                        }

                        class C : I
                        {
                            // implicitly inherits decoration from interface
                            string I.P { get; set; }
                        }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_property_in_explicit_interface_is_externally_annotated_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<NullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .InGlobalScope(@"
                        namespace N
                        {
                            interface I
                            {
                                string P { get; set; }
                            }

                            class C : I
                            {
                                // implicitly inherits decoration from interface
                                string I.P { get; set; }
                            }
                        }
                    "))
                .ExternallyAnnotated(new ExternalAnnotationsBuilder()
                    .IncludingMember(new ExternalAnnotationFragmentBuilder()
                        .Named("P:N.I.P")
                        .CanBeNull()))
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
                .ForRule<NullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .InGlobalScope(@"
                        interface I
                        {
                            [NotNull]
                            string P { get; set; }
                        }

                        class C : I
                        {
                            // implicitly inherits decoration from interface
                            string I.P { get; set; }

                            // requires explicit decoration
                            public string P { get; set; }
                        }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.Problems.Should().HaveCount(1);
            result.Problems[0].Resolution.Name.Should().Be(NullabilityRule.RuleName);
        }

        [Test]
        public void When_indexer_property_type_in_implicit_interface_is_not_annotated_it_must_be_reported()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<NullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .InGlobalScope(@"
                        namespace N
                        {
                            interface I
                            {
                                [CanBeNull]
                                int? this[char p] { get; set; }
                            }

                            class C : I
                            {
                                // implicitly inherits decoration from interface
                                int? I.this[char p]
                                {
                                    get { throw new NotImplementedException(); }
                                    set { throw new NotImplementedException(); }
                                }

                                // requires explicit decoration
                                public int? this[char p]
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
            result.Problems[0].Resolution.Name.Should().Be(NullabilityRule.RuleName);
        }

        [Test]
        public void
            When_property_in_implicit_interface_is_annotated_with_externally_annotated_explicit_interface_it_must_be_skipped
            ()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<NullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .InGlobalScope(@"
                        namespace N
                        {
                            interface I
                            {
                                string P { get; set; }
                            }

                            class C : I
                            {
                                // implicitly inherits decoration from interface
                                string I.P { get; set; }

                                // requires explicit decoration
                                [NotNull]
                                public string P { get; set; }
                            }
                        }
                    "))
                .ExternallyAnnotated(new ExternalAnnotationsBuilder()
                    .IncludingMember(new ExternalAnnotationFragmentBuilder()
                        .Named("P:N.I.P")
                        .NotNull()))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_containing_type_is_decorated_with_conditional_its_members_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<NullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .WithReference(typeof (ConditionalAttribute).Assembly)
                    .Using(typeof (ConditionalAttribute).Namespace)
                    .InGlobalScope(@"
                        namespace N
                        {
                            [Conditional(""JETBRAINS_ANNOTATIONS"")]
                            class C : Attribute
                            {
                                public string P { get; set; }
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
        public void When_base_property_inherits_annotation_from_interface_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<NullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .InGlobalScope(@"
                        namespace N
                        {
                            public interface I
                            {
                                [NotNull]
                                string P { get; set; }
                            }

                            public class B : I
                            {
                                public virtual string P { get; set; }
                            }

                            public class C : B
                            {
                                public override string P { get; set; }
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
                .ForRule<NullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .InGlobalScope(@"
                    namespace N
                    {
                        public class B
                        {
                            [NotNull]
                            public virtual string P { get; set; }
                        }

                        public class C : B
                        {
                            public new string P { get; set; }
                        }
                    }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.Problems.Should().HaveCount(1);
            result.Problems[0].Resolution.Name.Should().Be(NullabilityRule.RuleName);
        }
    }
}