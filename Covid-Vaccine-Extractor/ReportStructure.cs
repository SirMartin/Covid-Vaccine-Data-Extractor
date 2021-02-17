using Newtonsoft.Json;

namespace Covid_Vaccine_Extractor
{
    public class ReportStructure
    {
        [JsonProperty("region")]
        public string RegionName { get; set; }
        [JsonProperty("deliveredDoses")]
        public int DeliveredDoses { get; set; }
        [JsonProperty("appliedDoses")]
        public int AppliedDoses { get; set; }
        [JsonProperty("percentagePopulationVaccinated")]
        public double PercentagePopulationVaccinated { get; set; }
        [JsonProperty("bothDosesApplied")]
        public int BothDosesApplied { get; set; }
        [JsonProperty("percentagePopulationBothDoses")]
        public double PercentagePopulationBothDoses { get; set; }
        [JsonProperty("percentageOverDelivered")]
        public double PercentageOverDelivered { get; set; }
    }
}