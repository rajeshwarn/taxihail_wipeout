using System;
using Newtonsoft.Json;

namespace HoneyBadger.Responses
{
    public class CmtGeoContent : HoneyBadgerContent
    {
        [JsonProperty("etaInSeconds")]
        public long ETASeconds { get; set; }

        [JsonProperty("mapMatched")]
        public bool IsMapMatched { get; set; }
    }
}
