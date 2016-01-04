using Newtonsoft.Json;

namespace CMTServices.Responses
{
    public class CmtGeoContent : BaseAvailableVehicleContent
    {
        [JsonProperty("etaInSeconds")]
        public long? ETASeconds { get; set; }

        [JsonProperty("etaInMeters")]
        public long? ETAMeters { get; set; }

        [JsonProperty("mapMatched")]
        public bool IsMapMatched { get; set; }

		[JsonProperty("cc")]
		public float CompassCourse { get; set; }

        [JsonProperty("ldi")]
        public string LegacyDispatchId { get; set; }
    }
}
