using Newtonsoft.Json;

namespace CMTServices.Responses
{
    public class CmtGeoContent : HoneyBadgerContent
    {
        [JsonProperty("etaInSeconds")]
        public long ETASeconds { get; set; }

        [JsonProperty("mapMatched")]
        public bool IsMapMatched { get; set; }
    }
}
