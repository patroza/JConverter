using System;
using FluentAssertions;
using NUnit.Framework;

namespace JConverter.Tests.Unit
{
    public class MplusConverterTest : BaseTest<MplusConverter>
    {
        public class ConstructorTest : BaseTest<MplusConverter>
        {

            [Test]
            public void CanCreate()
            {
                SUT = new MplusConverter("testFile", new MplusConverter.Config());
                SUT.Should().BeOfType<MplusConverter>();
            }

            [Test]
            public void CannotCreateWithNullArguments()
            {
                Action act = () => SUT = new MplusConverter(null, new MplusConverter.Config());
                act.ShouldThrow<ArgumentNullException>();

                act = () => SUT = new MplusConverter("testFile", null);
                act.ShouldThrow<ArgumentNullException>();
            }
        }
    }
}