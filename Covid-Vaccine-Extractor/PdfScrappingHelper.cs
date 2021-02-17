using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Covid_Vaccine_Extractor
{
    public class PdfScrappingHelper
    {
        private static DateTime _actualReportDate = DateTime.MinValue;

        #region Configuration Fields

        private static readonly List<string> RegionNameList = new List<string> { "Hovedstaden", "Midtjylland", "Nordjylland", "Sjælland", "Syddanmark", "I alt" };
        private static bool EnableAppliedDoses => _actualReportDate > new DateTime(2021, 1, 6);
        private static bool EnablePercentagePopulationVaccinated => _actualReportDate > new DateTime(2021, 1, 6);
        private static bool EnableBothDosesApplied => _actualReportDate > new DateTime(2021, 1, 16);
        private static bool EnablePercentagePopulationBothDoses => _actualReportDate > new DateTime(2021, 1, 16);
        private static bool EnablePercentageOverDelivered => _actualReportDate > new DateTime(2021, 1, 16);
        private static string TextToLocateTable => _actualReportDate > new DateTime(2021, 2, 1) ? "Regionsniveau" : "Vaccinationer opgjort på regionsniveau";

        #endregion

        public Dictionary<DateTime, List<ReportStructure>> Start(string[] files)
        {
            var allReports = new Dictionary<DateTime , List<ReportStructure>>();
            foreach (var file in files)
            {
                try
                {
                    _actualReportDate = FileHelper.GetReportFileDato(Path.GetFileName(file));

                    var result = GetDataFromPdf(file);

                    allReports.Add(_actualReportDate, result);
                }
                catch (Exception ex)
                {

                }
            }

            return allReports;
        }
        private List<ReportStructure> GetDataFromPdf(string filename)
        {
            var results = new List<ReportStructure>();

            var pageNumber = 1;
            string pdfText;
            using (var stream = File.OpenRead(filename))
            using (UglyToad.PdfPig.PdfDocument document = UglyToad.PdfPig.PdfDocument.Open(stream))
            {
                do
                {
                    pageNumber++;
                    var page = document.GetPage(pageNumber);
                    pdfText = string.Join(" ", page.GetWords());
                } while (!pdfText.Contains(TextToLocateTable));
            }

            foreach (var regionName in RegionNameList)
            {
                var index = pdfText.IndexOf(regionName);
                var array = pdfText.Substring(index + regionName.Length + 1).Split(" ");
                results.Add(new ReportStructure
                {
                    RegionName = regionName == "I alt" ? "Total" : regionName,
                    DeliveredDoses = ConvertNumber(TryGetText(array, 0)),
                    AppliedDoses = EnableAppliedDoses ? ConvertNumber(TryGetText(array, 1)) : 0,
                    PercentagePopulationVaccinated = EnablePercentagePopulationVaccinated ? ConvertPercentage(TryGetText(array, 2)) : 0,
                    BothDosesApplied = EnableBothDosesApplied ? ConvertNumber(TryGetText(array, 3)) : 0,
                    PercentagePopulationBothDoses = EnablePercentagePopulationBothDoses ? ConvertPercentage(TryGetText(array, 4)) : 0,
                    PercentageOverDelivered = EnablePercentageOverDelivered ? ConvertPercentage(TryGetText(array, 5)) : 0
                });
            }

            return results;
        }

        #region Value Helpers

        private string TryGetText(string[] array, int position)
        {
            return array.Length >= position + 1 ? array[position] : "0";
        }

        private int ConvertNumber(string value)
        {
            try
            {
                if (value.Contains("-"))
                    return 0;

                return Convert.ToInt32(value.Replace(".", ""));
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        private double ConvertPercentage(string value)
        {
            try
            {
                if (value.Contains("-"))
                    return 0;

                return Math.Round(double.Parse(value, new CultureInfo("da-DK")) / 100, 4);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        #endregion
    }
}
