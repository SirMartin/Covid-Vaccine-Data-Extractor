using System;
using System.IO;

namespace Covid_Vaccine_Extractor
{
    public class FileHelper
    {
        public static DateTime GetReportFileDato(string filename)
        {
            var dato = Path.GetFileName(filename).Substring("Vaccinationstilslutning-".Length, 8);

            return DateTime.ParseExact(dato, "ddMMyyyy", null);
        }

        public static string GetReportFormattedName(string filename)
        {
            return GetReportFileDato(filename).ToString("yyyyMMdd");
        }
    }
}
