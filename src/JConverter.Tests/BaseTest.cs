using NUnit.Framework;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoFakeItEasy;

namespace JConverter.Tests
{
    [TestFixture]
    public abstract class BaseTest
    {
        protected Fixture Fixturer { get; } = new Fixture();

        protected BaseTest()
        {
            Fixturer.Customize(new AutoFakeItEasyCustomization());
        }
    }
}