using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Covid_Vaccine_Extractor
{
    class Program
    {
        private static List<string> _reportDates = new List<string>();

        public static string ContentRootPath
        {
            get
            {
                string rootDirectory = AppContext.BaseDirectory;
                if (rootDirectory.Contains("bin"))
                {
                    rootDirectory = rootDirectory.Substring(0, rootDirectory.IndexOf("bin"));
                }
                return rootDirectory;
            }
        }

        private static string DataPath => Path.Join(ContentRootPath, "data");

        private static string ResultsPath => Path.Join(DataPath, "results");

        static void Main(string[] args)
        {
            if (args.Length > 0 && args.Contains("-all"))
            {
                // Process all the files.
                Process(null);
            }
            else
            {
                // Check to find the latest one, and if exists download it and process it.
                var latestReport = new DownloadHelper(DataPath, ResultsPath).Do();
                if (!string.IsNullOrEmpty(latestReport))
                {
                    Process(latestReport);
                }
            }
        }

        private static void Process(string specificFile)
        {
            var processingAllTheFiles = false;
            string[] files;
            if (string.IsNullOrWhiteSpace(specificFile))
            {
                processingAllTheFiles = true;
                files = Directory.GetFiles($@"{ContentRootPath}\data");
            }
            else
            {
                files = new[] { specificFile };
            }

            var result = new PdfScrappingHelper().Start(files);

            foreach (var report in result)
            {
                ExportToJson(report);
            }

            var newestReportDate = result.Keys.Max();

            // Update info.json
            var epochTime = DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1, 0, 0, 0, 0).Ticks;
            File.WriteAllText(Path.Join(ResultsPath, "info.json"), "{\"lastModified\":" + Math.Round((decimal)epochTime / 10000, 0) + "}");

            // Update reports.json
            if (!processingAllTheFiles)
            {
                // Update the list
                var text = File.ReadAllText(Path.Join(ResultsPath, "reports.json"));
                var reportList = JsonConvert.DeserializeObject<List<string>>(text);
                _reportDates = new List<string>(reportList)
                {
                    newestReportDate.ToString("yyyyMMdd")
                };
            }

            File.WriteAllText(Path.Join(ResultsPath, "reports.json"), JsonConvert.SerializeObject(_reportDates.OrderBy(x => x)));

            // Create latest.json with the newest report.
            File.Copy(Path.Join(ResultsPath, $"{newestReportDate:yyyyMMdd}.json"), Path.Join(ResultsPath, "latest.json"), true);
        }

        private static void ExportToJson(KeyValuePair<DateTime, List<ReportStructure>> result)
        {
            var exportFilename = result.Key.ToString("yyyyMMdd");

            var jsonContent = JsonConvert.SerializeObject(result.Value);

            if (!Directory.Exists(ResultsPath))
            {
                Directory.CreateDirectory(ResultsPath);
            }

            File.WriteAllText(Path.Join(ResultsPath, $"{exportFilename}.json"), jsonContent);

            // Add to report list.
            _reportDates.Add(exportFilename);
        }


    }
}
