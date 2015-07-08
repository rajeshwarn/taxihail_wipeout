using Newtonsoft.Json;

namespace HoneyBadger.Responses
{
    public class CmtGeoResponse
    {
        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("vehicles")]
        public HoneyBadgerContent[] Entities { get; set; }
    }
}
