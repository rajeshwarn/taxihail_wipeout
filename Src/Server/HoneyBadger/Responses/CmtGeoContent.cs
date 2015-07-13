﻿using Newtonsoft.Json;

namespace CMTServices.Responses
{
    public class CmtGeoContent : BaseAvailableVehicleContent
    {
        [JsonProperty("etaInSeconds")]
        public long? ETASeconds { get; set; }

        [JsonProperty("mapMatched")]
        public bool IsMapMatched { get; set; }
    }
}
