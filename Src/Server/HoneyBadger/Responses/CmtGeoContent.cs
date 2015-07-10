using System;
using Newtonsoft.Json;

namespace HoneyBadger.Responses
{
    public class CmtGeoContent : HoneyBadgerContent
    {
        [JsonProperty("etaInSeconds")]
        public Int64 ETASeconds { get; set; }

        [JsonProperty("mapMatched")]
        public Boolean IsMapMatched { get; set; }
    }
}
