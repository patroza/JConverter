using System;
using System.IO;
using JConverter.Properties;
using NDepend.Path;

namespace JConverter
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var config = new MplusConverter.Config
            {
                EmptyReplacement = Settings.Default.MissingValue,
                IgnoreNonNumerical = Settings.Default.IgnoreNonNumericalOnOtherLines
            };
            try
            {
                foreach (var fn in args)
                    using (var l = new SessionLogger((fn + ".log").ToAbsoluteFilePath()))
                        new MplusConverter(fn.ToAbsoluteFilePath(), config, l).ProcessFile();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Press a key to exit");
                Console.ReadKey();
            }
        }
    }

    public class SessionLogger : ILogger, IDisposable
    {
        private readonly StreamWriter _stream;

        public SessionLogger(IAbsoluteFilePath logFile)
        {
            _stream = new StreamWriter(logFile.ToString());
            Write($"Session start: {DateTime.Now}");
        }

        public void Dispose()
        {
            Write($"Session end: {DateTime.Now}");
            _stream.Flush();
            _stream.Dispose();
        }

        public void Write(string data) => _stream.WriteLine(data);
    }
}