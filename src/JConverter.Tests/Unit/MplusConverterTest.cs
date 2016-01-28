using System;
using System.IO;
using FakeItEasy;
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
            private static ILogger CreateLogger() => A.Fake<ILogger>();

            private static MplusConverter.Config CreateConfig() => new MplusConverter.Config();

            [Test]
            public void CanCreate()
            {
                SUT = new MplusConverter(TestFilePath, CreateConfig(), CreateLogger());
                SUT.Should().BeOfType<MplusConverter>();
            }

            [Test]
            public void CannotCreateWithNullArguments()
            {
                Action act = () => SUT = new MplusConverter(null, CreateConfig(), CreateLogger());
                act.ShouldThrow<ArgumentNullException>();

                act = () => SUT = new MplusConverter(TestFilePath, null, CreateLogger());
                act.ShouldThrow<ArgumentNullException>();

                act = () => SUT = new MplusConverter(TestFilePath, CreateConfig(), null);
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
            }
            catch (NonNumericalException ex)
            {
                var info = ex.Context;
                info.Row.Should().Be(1);
                info.LineNumber.Should().Be(2);
                info.Column.Should().Be(2);
                info.Context.Should().Be("0,00001\tNotAHeader");
                info.FirstMatch.Should().Be("NotAHeader");
                ex.Message.Should()
                    .Be(
                        $"There are non numerical characters on another line than the first. Line: {info.LineNumber}, Row: {info.Row}, Column: {info.Column}, firstMatch: {info.FirstMatch}");
            }
        }
    }
}