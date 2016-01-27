using System;
using FluentAssertions;
using NDepend.Path;
using NUnit.Framework;

namespace JConverter.Tests.Unit
{
    public class MplusConverterTest : BaseTest<MplusConverter>
    {
        private static readonly IAbsoluteDirectoryPath TestPath;
        private static readonly IAbsoluteFilePath TestFilePath;

        static MplusConverterTest()
        {
            TestPath = "C:\\temp\\JConverter.Test".ToAbsoluteDirectoryPath();
            TestFilePath = TestPath.GetChildFileWithName("testFile");
        }

        public class ConstructorTest : BaseTest<MplusConverter>
        {
            [Test]
            public void CanCreate()
            {
                SUT = new MplusConverter(TestFilePath, new MplusConverter.Config());
                SUT.Should().BeOfType<MplusConverter>();
            }

            [Test]
            public void CannotCreateWithNullArguments()
            {
                Action act = () => SUT = new MplusConverter(null, new MplusConverter.Config());
                act.ShouldThrow<ArgumentNullException>();

                act = () => SUT = new MplusConverter(TestFilePath, null);
                act.ShouldThrow<ArgumentNullException>();
            }
        }
    }
}