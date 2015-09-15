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
    }
}