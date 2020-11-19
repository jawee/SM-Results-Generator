using CsvHelper;
using System.IO;
using System.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ResultsGenerator
{
    public class Program
    {
        static void Main(string[] args)
        {
            var records = new List<ResultsRow>();
            //convert to utf-8 here: https://subtitletools.com/convert-text-files-to-utf8-online
            var file1 = @"C:\Users\olsso\Downloads\tmp\eventresult_35674144.csv";
            var file2 = @"C:\Users\olsso\Downloads\tmp\eventresult_35701388.csv";

            records = ReadFile(file1, records);
            records = ReadFile(file2, records);

            records = removeDuplicates(records);

            records.Sort(delegate (ResultsRow r1, ResultsRow r2) { return r1.QualifyTime.CompareTo(r2.QualifyTime); });

            using (var file = new StreamWriter(@"C:\Users\olsso\Downloads\tmp\res.csv"))

            {
                var count = 0;
                foreach (var line in records)
                {
                    file.WriteLine("<tr><td>" + ++count + "</td><td>" + line.Name + "</td><td>" + line.QualifyTime.ToString("mm:ss.fff") + "</td></tr>");
                }
            }
            
        }


        public static List<ResultsRow> ReadFile(string filePath, List<ResultsRow> list)
        {
            using (var reader = new StreamReader(new FileStream(filePath, FileMode.Open), Encoding.UTF8))
            {
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Read();
                    csv.Read();
                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        var qualifyTimeStr = "0" + csv.GetField("Qualify Time");
                        if (qualifyTimeStr == "0")
                        {
                            continue;
                        }
                        var resultsRow = new ResultsRow
                        {
                            Name = csv.GetField("Name"),
                            QualifyTime = DateTime.ParseExact("0" + csv.GetField("Qualify Time"), "mm:ss.fff", CultureInfo.InvariantCulture)
                        };
                        list.Add(resultsRow);
                    }
                }
            }

            return list;
        }
        public static List<ResultsRow> removeDuplicates(List<ResultsRow> input)
        {
            var duplicates = input.GroupBy(x => x.Name).Where(g => g.Count() > 1).Select(x => x);

            foreach(var dupl in duplicates)
            {
                ResultsRow best = null;
                var toBeRemoved = new List<ResultsRow>();

                foreach(var res in dupl)
                {
                    if(best == null)
                    {
                        best = res;
                        continue;
                    }
                    if(res.QualifyTime > best.QualifyTime)
                    {
                        toBeRemoved.Add(res);
                    } else
                    {
                        toBeRemoved.Add(best);
                        best = res;
                    }
                }

                foreach(var rem in toBeRemoved)
                {
                    input.Remove(rem);
                }
            }

            return input;
        }
    }


    public class ResultsRow
    {
        public string Name { get; set; }
        public DateTime QualifyTime { get; set; }
       
    }
}
