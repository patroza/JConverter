using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace JConverter2
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                foreach (var fn in args)
                    ProcessFile(fn);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Press a key to exit");
                Console.ReadKey();
            }
        }

        private static void ProcessFile(string fn)
        {
            var outFn = fn + "-conv.dat";
            if (File.Exists(outFn))
                throw new NotSupportedException($"{outFn} already exists, please delete it first");

            var data = File.ReadAllLines(fn);
            var rx = new Regex(@"[\s\t]+");

            foreach (var l in data.Select((x, i) => new {x, i}))
            {
                data[l.i] = rx.Replace(l.x, "\t").Trim('\t', ' ');
            }

            File.WriteAllText(outFn, string.Join(Environment.NewLine, data));
        }
    }
}