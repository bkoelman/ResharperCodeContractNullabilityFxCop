using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using CodeContractNullabilityFxCopRules.Test.TestDataBuilders;
using FluentAssertions;
using FxCopUnitTestRunner;
using FxCopUnitTestRunner.TestDataBuilders;
using NUnit.Framework;

namespace CodeContractNullabilityFxCopRules.Test.Specs
{
    /// <summary>
    /// Tests for reporting nullability diagnostics on fields.
    /// </summary>
    [TestFixture]
    internal class FieldSpecs
    {
        [Test]
        public void When_field_is_annotated_with_not_nullable_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<NullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .WithNullabilityAttributes(new NullabilityAttributesBuilder()
                        .InCodeNamespace("N.M"))
                    .InGlobalScope(@"
                        class C
                        {
                            [N.M.NotNull] // Using fully qualified namespace
                            string f;
                        }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_field_is_annotated_with_nullable_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<NullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .WithNullabilityAttributes(new NullabilityAttributesBuilder()
                        .InCodeNamespace("N1"))
                    .InGlobalScope(@"
                        namespace N2
                        {
                            using CBN = N1.CanBeNullAttribute;

                            class C
                            {
                                [CBN()] // Using type/namespace alias
                                string f;
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
        public void When_field_is_externally_annotated_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<NullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .InGlobalScope(@"
                        namespace N
                        {
                            public class C
                            {
                                public string F;
                            }
                        }
                    "))
                .ExternallyAnnotated(new ExternalAnnotationsBuilder()
                    .IncludingMember(new ExternalAnnotationFragmentBuilder()
                        .Named("F:N.C.F")
                        .NotNull()))
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
                .ForRule<NullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .InDefaultClass(@"
                        public const string F = ""X"";
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_field_type_is_value_type_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<NullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .InDefaultClass(@"
                        int f;
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_field_type_is_generic_value_type_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<NullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .InGlobalScope(@"
                        class C<T> where T : struct
                        {
                            T f;
                        }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_field_type_is_enum_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<NullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .Using(typeof (BindingFlags).Namespace)
                    .InDefaultClass(@"
                        BindingFlags f;
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_field_type_is_nullable_it_must_be_reported()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<NullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .InDefaultClass(@"
                        int? f;
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.Problems.Should().HaveCount(1);
            result.Problems[0].Resolution.Name.Should().Be(NullabilityRule.RuleName);
        }

        [Test]
        public void When_field_type_is_generic_nullable_it_must_be_reported()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<NullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .InGlobalScope(@"
                        class C<T> where T : struct
                        {
                            T? f;
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
        public void When_generic_field_is_externally_annotated_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<NullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .InGlobalScope(@"
                        namespace N
                        {
                            public class C<T> where T : struct
                            {
                                public T? F;
                            }
                        }
                    "))
                .ExternallyAnnotated(new ExternalAnnotationsBuilder()
                    .IncludingMember(new ExternalAnnotationFragmentBuilder()
                        .Named("F:N.C`1.F")
                        .CanBeNull()))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_field_type_is_reference_it_must_be_reported()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<NullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .InDefaultClass(@"
                        string f;
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.Problems.Should().HaveCount(1);
            result.Problems[0].Resolution.Name.Should().Be(NullabilityRule.RuleName);
        }

        [Test]
        public void When_field_is_compiler_generated_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<NullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .Using(typeof (CompilerGeneratedAttribute).Namespace)
                    .InDefaultClass(@"
                        [CompilerGenerated]
                        string f;
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_field_is_event_handler_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<NullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .Using(typeof (EventHandler).Namespace)
                    .InDefaultClass(@"
                        public event EventHandler E;
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_field_is_event_handler_with_accessors_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<NullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .Using(typeof (EventHandler).Namespace)
                    .InDefaultClass(@"
                        public event EventHandler E
                        {
                            add { throw new NotImplementedException(); }
                            remove { throw new NotImplementedException(); }
                        }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_field_is_custom_event_handler_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<NullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .Using(typeof (EventHandler<>).Namespace)
                    .Using(typeof (EventArgs).Namespace)
                    .InGlobalScope(@"
                        public class DerivedEventArgs : EventArgs { }

                        class C
                        {
                            public event EventHandler<DerivedEventArgs> E;
                        }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_field_is_designer_generated_control_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<NullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .WithReference(typeof (Control).Assembly)
                    .Using(typeof (Control).Namespace)
                    .Using(typeof (Button).Namespace)
                    .InGlobalScope(@"
                        public partial class DerivedControl : Control
                        {
                            private Button button1;
                        }
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.ProblemText.Should().Be(FxCopRuleValidationResult.NoProblemsText);
        }

        [Test]
        public void When_field_is_designer_generated_container_it_must_be_skipped()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<NullabilityRule>()
                .OnAssembly(new ClassSourceCodeBuilder()
                    .WithReference(typeof (Form).Assembly)
                    .Using(typeof (Form).Namespace)
                    .Using(typeof (IContainer).Namespace)
                    .InGlobalScope(@"
                        public partial class DerivedForm : Form
                        {
                            private IContainer components;
                        }
                    "))
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
                                private string f;
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
        public void When_field_contains_multiple_variables_they_must_be_reported()
        {
            // Arrange
            FxCopRuleValidator validator = new FxCopNullabilityRuleValidatorBuilder()
                .ForRule<NullabilityRule>()
                .OnAssembly(new MemberSourceCodeBuilder()
                    .InDefaultClass(@"
                        int? f, g;
                    "))
                .Build();

            // Act
            FxCopRuleValidationResult result = validator.Execute();

            // Assert
            result.Problems.Should().HaveCount(2);
            result.Problems[0].Resolution.Name.Should().Be(NullabilityRule.RuleName);
            result.Problems[1].Resolution.Name.Should().Be(NullabilityRule.RuleName);
        }
    }
}