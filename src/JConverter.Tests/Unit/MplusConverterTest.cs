using System;
using System.IO;
using FluentAssertions;
using NDepend.Path;
using NUnit.Framework;
using Ploeh.AutoFixture;

namespace JConverter.Tests.Unit
{
    public class MplusConverterTest : BaseTest<MplusConverter>
    {
        [SetUp]
        public void SetUp()
        {
            if (TestPath.Exists) TestPath.DirectoryInfo.Delete(true);
            Directory.CreateDirectory(TestPath.ToString());
            Fixture.Inject(TestFilePath);
            BuildFixture();
        }

        [TearDown]
        public void Teardown()
        {
            if (TestPath.Exists) TestPath.DirectoryInfo.Delete(true);
        }

        private static readonly IAbsoluteDirectoryPath TestPath;
        private static readonly IAbsoluteFilePath TestFilePath;


        static MplusConverterTest()
        {
            TestPath = "C:\\temp\\JConverter.Test".ToAbsoluteDirectoryPath();
            TestFilePath = TestPath.GetChildFileWithName("testFile");
        }

        private static void WriteOutFile(IAbsoluteFilePath outFile, string outType)
            => File.WriteAllText(outFile.ToString(), "test out " + outType);

        private void WriteTestInputFile() => File.WriteAllText(SUT.InFile.ToString(), "test in");

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

        [Test]
        public void WhenProcessingFileAndInputFileDoesNotExist()
        {
            ExceptionTest = () => SUT.ProcessFile();
            ExceptionTest.ShouldThrow<FileNotFoundException>();
        }

        [Test]
        public void WhenProcessingFileAndOutputDatFileExist()
        {
            WriteTestInputFile();
            WriteOutFile(SUT.OutDatFile, "dat");
            ExceptionTest = () => SUT.ProcessFile();
            ExceptionTest.ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void WhenProcessingFileAndOutputInpFileExist()
        {
            WriteTestInputFile();
            WriteOutFile(SUT.OutInpFile, "inp");
            ExceptionTest = () => SUT.ProcessFile();
            ExceptionTest.ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void WhenProcessingFileOutputFilesShouldExist() {
            WriteTestInputFile();
            SUT.ProcessFile();
            SUT.OutInpFile.Exists.Should().BeTrue();
            SUT.OutDatFile.Exists.Should().BeTrue();
        }
    }
}