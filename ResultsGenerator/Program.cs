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
            var top = @"<!DOCTYPE html>
                        <html lang=""se"">
                          <head>
                            <title>SM Resultat</title>
                            <link href=""https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/css/bootstrap.min.css"" rel=""stylesheet"">
                            <link href=""style.css"" rel=""stylesheet"">
                            <meta charset=""utf-8""> 
                          <link rel=""apple-touch-icon"" sizes=""57x57"" href=""/favicon/apple-icon-57x57.png"">
                          <link rel=""apple-touch-icon"" sizes=""60x60"" href=""/favicon/apple-icon-60x60.png"">
                          <link rel=""apple-touch-icon"" sizes=""72x72"" href=""/favicon/apple-icon-72x72.png"">
                          <link rel=""apple-touch-icon"" sizes=""76x76"" href=""/favicon/apple-icon-76x76.png"">
                          <link rel=""apple-touch-icon"" sizes=""114x114"" href=""/favicon/apple-icon-114x114.png"">
                          <link rel=""apple-touch-icon"" sizes=""120x120"" href=""/favicon/apple-icon-120x120.png"">
                          <link rel=""apple-touch-icon"" sizes=""144x144"" href=""/favicon/apple-icon-144x144.png"">
                          <link rel=""apple-touch-icon"" sizes=""152x152"" href=""/favicon/apple-icon-152x152.png"">
                          <link rel=""apple-touch-icon"" sizes=""180x180"" href=""/favicon/apple-icon-180x180.png"">
                          <link rel=""icon"" type=""image/png"" sizes=""192x192""  href=""/favicon/android-icon-192x192.png"">
                          <link rel=""icon"" type=""image/png"" sizes=""32x32"" href=""/favicon/favicon-32x32.png"">
                          <link rel=""icon"" type=""image/png"" sizes=""96x96"" href=""/favicon/favicon-96x96.png"">
                          <link rel=""icon"" type=""image/png"" sizes=""16x16"" href=""/favicon/favicon-16x16.png"">
                          <meta name=""msapplication-TileColor"" content=""#ffffff"">
                          <meta name=""msapplication-TileImage"" content=""/favicon/ms-icon-144x144.png"">
                          <meta name=""theme-color"" content=""#ffffff"">
                          </head>
                          <body>
                            <div class=""container"">
                              <h1 class=""text-center"">Kvaltider Okayama</h1>
                              <table class=""table"">
                                <thead>
                                  <tr><th>Placering</th><th>Namn</th><th>Tid</th><th>Antal körda varv</th></td>
                                </thead>
                                <tbody>";

            var bottom = @"</tbody>
                        </div>
                        </body>
                        </html>";
            var records = new List<ResultsRow>();
            //convert result files to utf-8 here: https://subtitletools.com/convert-text-files-to-utf8-online

            var files = Directory.GetFiles(@"C:\Users\olsso\Documents\Projects\ResultsGenerator\Files\Okayama\", "*.csv");

            foreach(var file in files)
            {
                records = ReadFile(file, records);
            }

            records = removeDuplicates(records);

            records.Sort(delegate (ResultsRow r1, ResultsRow r2) { return r1.QualifyTime.CompareTo(r2.QualifyTime); });

            using (var file = new StreamWriter(@"C:\Users\olsso\Documents\Projects\ResultsGenerator\Files\Okayama\index.html"))
            {
                file.WriteLine(top);
                var count = 0;
                foreach (var line in records)
                {
                    file.WriteLine("<tr><td>" + ++count + "</td><td>" + line.Name + "</td><td>" + line.QualifyTime.ToString("mm:ss.fff") + "</td><td>" + line.LapsComp + "</td></tr>");
                }
                file.WriteLine(bottom);
            }

            Console.WriteLine("Done");
            Console.ReadLine();
        }


        public static List<ResultsRow> ReadFile(string filePath, List<ResultsRow> list)
        {
            using (var reader = new StreamReader(new FileStream(filePath, FileMode.Open), Encoding.UTF8))
            {
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    // Lines without results
                    csv.Read();
                    csv.Read();
                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        var qualifyTimeStr = csv.GetField("Qualify Time");
                        if (String.IsNullOrWhiteSpace(qualifyTimeStr))
                        {
                            continue;
                        }
                        var resultsRow = new ResultsRow
                        {
                            Name = csv.GetField("Name"),
                            QualifyTime = DateTime.ParseExact(qualifyTimeStr, "m:ss.fff", CultureInfo.InvariantCulture),
                            LapsComp = csv.GetField<int>("Laps Comp")
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
                        best.LapsComp += res.LapsComp;
                        toBeRemoved.Add(res);
                    } else
                    {
                        res.LapsComp += best.LapsComp;
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

        public int LapsComp { get; set; }
       
    }
}
