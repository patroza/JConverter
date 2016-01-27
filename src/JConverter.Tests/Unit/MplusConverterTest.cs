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
            Fixture.Inject(new MplusConverter.Config());
            BuildFixture();
        }

        [TearDown]
        public void TearDown()
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

        private void WriteTestInputFile(string content = "test in") => File.WriteAllText(SUT.InFile.ToString(), content);

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
        public void WhenProcessingFileAndInputFileDoesNotExistShouldThrowError()
        {
            ExceptionTest = () => SUT.ProcessFile();
            ExceptionTest.ShouldThrow<FileNotFoundException>();
        }

        [Test]
        public void WhenProcessingFileAndOutputDatFileExistShouldThrowError()
        {
            WriteTestInputFile();
            WriteOutFile(SUT.OutDatFile, "dat");
            ExceptionTest = () => SUT.ProcessFile();
            ExceptionTest.ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void WhenProcessingFileAndOutputInpFileExistShouldThrowError()
        {
            WriteTestInputFile();
            WriteOutFile(SUT.OutInpFile, "inp");
            ExceptionTest = () => SUT.ProcessFile();
            ExceptionTest.ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void WhenProcessingFileOutputFilesShouldExist()
        {
            WriteTestInputFile();
            SUT.ProcessFile();
            SUT.OutInpFile.Exists.Should().BeTrue();
            SUT.OutDatFile.Exists.Should().BeTrue();
        }


        [Test]
        public void WhenProcessingFileWithMultipleRowsOfNonNumericalShouldThrowError()
        {
            WriteTestInputFile(
                @"Header1	Header2
0,00001	NotAHeader
");
            ExceptionTest = () => SUT.ProcessFile();
            ExceptionTest.ShouldThrow<NonNumericalException>();

            try
            {
                ExceptionTest();
            } catch (NonNumericalException ex)
            {
                ex.Context.LineNumber.Should().Be(1);
                ex.Context.Column.Should().Be(2);
                ex.Context.Context.Should().Be("0,00001\tNotAHeader");
                ex.Context.FirstMatch.Should().Be("NotAHeader");
            }
        }
    }
}