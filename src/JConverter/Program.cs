﻿using System;
using JConverter.Properties;

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
                    new MplusConverter(fn, config).ProcessFile();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Press a key to exit");
                Console.ReadKey();
            }
        }
    }
}