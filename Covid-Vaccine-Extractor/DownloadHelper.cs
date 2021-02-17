using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Covid_Vaccine_Extractor
{
    public class DownloadHelper
    {
        private readonly string _dataPath;
        private readonly string _resultsPath;

        public DownloadHelper(string dataPath, string resultsPath)
        {
            _dataPath = dataPath;
            _resultsPath = resultsPath;
        }

        public string Do()
        {
            var pdfUrl = FindPdfUrl();

            if (pdfUrl == null)
            {
                throw new Exception("SSI website is down.");
            }

            // Check if already we have this report.
            if (CheckForDownloadNewReport(new Uri(pdfUrl)))
            {
                return DownloadReport(new Uri(pdfUrl));
            }

            return null;
        }

        private bool CheckForDownloadNewReport(Uri uri)
        {
            // Get the newest report processed.
            var text = File.ReadAllText(Path.Join(_resultsPath, "reports.json"));
            var reportList = JsonConvert.DeserializeObject<List<string>>(text);

            // Get the report name for the newest in the website.
            var filename = Path.GetFileName(uri.AbsolutePath);
            var reportName = FileHelper.GetReportFormattedName(filename);

            return !reportList.Contains(reportName);
        }

        private string FindPdfUrl()
        {
            var urlAddress = "https://covid19.ssi.dk/overvagningsdata/vaccinationstilslutning";

            var request = (HttpWebRequest)WebRequest.Create(urlAddress);
            var response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (string.IsNullOrWhiteSpace(response.CharacterSet))
                    readStream = new StreamReader(receiveStream);
                else
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));

                var data = readStream.ReadToEnd();

                response.Close();
                readStream.Close();

                // Find in the html the latest PDF url.
                var ma = Regex.Matches(data, "<a.+?href=['\"](?<href>.*?)['\"].*?>(?<txt>.+?)</a>", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

                var links = ma.OfType<Match>()
                    .Select(m => new {Href = m.Groups["href"].Value, Text = m.Groups["txt"].Value});

                var latestPdf = links.First(x => x.Text == "Download her").Href;

                return latestPdf;
            }

            return null;
        }

        private string DownloadReport(Uri uri)
        {
            var filename = Path.GetFileName(uri.AbsolutePath);

            if (!filename.EndsWith(".pdf"))
            {
                filename += ".pdf";
            }

            var webClient = new WebClient();
            
            var downloadPath = Path.Join(_dataPath, filename);
            webClient.DownloadFile(uri, downloadPath);

            return downloadPath;
        }
    }
}
