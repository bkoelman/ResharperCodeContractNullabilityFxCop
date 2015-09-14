using JetBrains.Annotations;

namespace FxCopUnitTestRunner.TestDataBuilders
{
    public interface ITestDataBuilder<out T>
    {
        // ReSharper disable once UnusedMemberInSuper.Global
        [NotNull]
        T Build();
    }
}