using System;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoFakeItEasy;

namespace JConverter.Tests
{
    [TestFixture]
    public abstract class BaseTest
    {
        protected Fixture Fixture { get; } = new Fixture();
        protected Action ExceptionTest { get; set; }

        protected BaseTest()
        {
            Fixture.Customize(new AutoFakeItEasyCustomization());
        }
    }

    public abstract class BaseTest<T> : BaseTest
    {
        // ReSharper disable once InconsistentNaming
        public T SUT { get; set; }

        protected virtual void BuildFixture() => SUT = Fixture.Create<T>();
    }
}