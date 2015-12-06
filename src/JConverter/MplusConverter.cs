﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace JConverter
{
    internal class MplusConverter
    {
        private static readonly Regex NonNumerical = new Regex(@"[^\d,.-]+", RegexOptions.Compiled);
        private readonly Config _config;
        private string[] _data;

        public MplusConverter(string inFile, Config config)
        {
            _config = config;
            InFile = inFile;
            OutDatFile = inFile + ".dat";
            OutInpFile = inFile + ".inp";
        }

        public string OutInpFile { get; }

        public string OutDatFile { get; }

        public string InFile { get; }

        public List<string> ColumnHeaders { get; private set; } = new List<string>();

        public IEnumerable<string> GetTooLongHeaders() => ColumnHeaders.Where(x => x.Length > _config.MaxHeaderLength);

        public void ProcessFile()
        {
            ConfirmInputFileExists();
            ConfirmOutFilesDontExist();
            ReadInputFile();
            CreateTransformedDatFile();
            CreateInpFile();
        }

        private void ConfirmInputFileExists()
        {
            if (!File.Exists(InFile)) throw new Exception("The file does not exist: " + InFile);
        }

        private void ConfirmOutFilesDontExist()
        {
            if (File.Exists(OutDatFile))
                throw new Exception("The .dat file already exists, please delete it first: " + OutDatFile);
            if (File.Exists(OutInpFile))
                throw new Exception("The .inp file already exists, please delete it first: " + OutInpFile);
        }

        private void ReadInputFile()
        {
            _data = File.ReadAllLines(InFile);
        }

        private void CreateTransformedDatFile()
        {
            File.WriteAllText(OutDatFile, TransformData());
        }


        private void CreateInpFile()
        {
            File.WriteAllText(OutInpFile, GenerateInpData());
        }

        private string GenerateInpData()
        {
            var sb = new StringBuilder();

            AddTooLongVariablesInfo(sb);
            AddDataInfo(sb);
            AddVariablesInfo(sb);
            AddAnalysisInfo(sb);

            return sb.ToString();
        }

        private void AddTooLongVariablesInfo(StringBuilder sb)
        {
            var tooLongHeaders = GetTooLongHeaders().ToArray();
            if (!tooLongHeaders.Any()) return;
            sb.AppendLine(
                $"! The following headers are too long, you should make them shorter:\n {SplitWhenLonger(string.Join(", ", tooLongHeaders), "!      ")}");
            sb.AppendLine();
        }

        private void AddDataInfo(StringBuilder sb)
        {
            sb.AppendLine($"DATA:    FILE IS {new FileInfo(OutDatFile).Name};");
            sb.AppendLine();
        }

        private void AddVariablesInfo(StringBuilder sb)
        {
            if (!ColumnHeaders.Any() && !HasEmptyReplacement())
                return;

            sb.Append("VARIABLE:    ");
            if (ColumnHeaders.Any())
            {
                sb.AppendLine($"NAMES ARE {SplitWhenLonger(JoinHeaders())};");
                sb.AppendLine($"IDVARIABLE IS {ColumnHeaders.First()};");
            }

            if (HasEmptyReplacement())
                sb.AppendLine($"MISSING ARE ALL ({_config.EmptyReplacement});");

            sb.AppendLine();
        }

        private string JoinHeaders() => string.Join(" ", ColumnHeaders);

        private string SplitWhenLonger(string input, string prefix = "", int length = 80)
            => string.Join(_config.NewLineCharacters, SplitWhenLongerInternal(input, length).Select(x => prefix + x));

        private static IEnumerable<string> SplitWhenLongerInternal(string input, int length = 80)
            => Regex.Split(input, @"(.{1," + length + @"})(?:\s|$)")
                .Where(x => x.Length > 0);

        private void AddAnalysisInfo(StringBuilder sb)
        {
            sb.AppendLine($"ANALYSIS: TYPE IS {_config.AnalysisType};");
            sb.AppendLine();
        }

        private bool HasEmptyReplacement()
        {
            return _config.EmptyReplacement != null;
        }

        private string TransformData()
        {
            foreach (var lInfo in _data.Select((x, i) => Tuple.Create(i, x)))
                _data[lInfo.Item1] = TransformLine(lInfo);
            return string.Join(_config.NewLineCharacters, _data.Where(x => x != null));
        }

        private string TransformLine(Tuple<int, string> line)
        {
            var columns = line.Item2.Split('\t');
            if (columns.Any(x => NonNumerical.IsMatch(x)))
            {
                if (line.Item1 != 0)
                    throw new NotSupportedException(
                        "There are non numerical characters on another line than the first. Line: " + line.Item1);
                ColumnHeaders = columns.ToList();
                return null;
            }
            foreach (var entry in columns.Select((x, i) => Tuple.Create(i, x)))
                columns[entry.Item1] = ProcessEntry(entry);
            return string.Join("\t", columns);
        }

        private string ProcessEntry(Tuple<int, string> column)
        {
            var r = column.Item2;
            r = ReplaceReplacements(r);
            r = ReplaceEmpty(r);
            return r;
        }

        private string ReplaceEmpty(string column)
        {
            if (HasEmptyReplacement() && string.IsNullOrWhiteSpace(column))
                return _config.EmptyReplacement;
            return column;
        }

        private string ReplaceReplacements(string column)
        {
            if (_config.Replacements == null) return column;
            return _config.Replacements.Aggregate(column,
                (current, replacement) => current.Replace(replacement.Key, replacement.Value));
        }

        internal class Config
        {
            public IDictionary<string, string> Replacements { get; } = new Dictionary<string, string> {{".", ","}};
            public string EmptyReplacement { get; } = "-999";
            public int MaxHeaderLength { get; set; } = 8;
            public string AnalysisType { get; set; } = "BASIC";
            public string NewLineCharacters { get; set; } = Environment.NewLine;
        }
    }
}