using System;
using CMTServices.Converters;
using CMTServices.Enums;
using Newtonsoft.Json;

namespace CMTServices.Responses
{
    public class  BaseAvailableVehicleContent
    {
        [JsonProperty("dt")]
        public DeviceTypes DeviceType { get; set; }

        [JsonProperty("dn")]
        public string DeviceName { get; set; }

        [JsonProperty("tm")]
        [JsonConverter(typeof(EpochDateTimeConverter))]
        public DateTime TimeStamp { get; set; }

        [JsonProperty("lt")]
        public float Latitude { get; set; }

        [JsonProperty("lg")]
        public float Longitude { get; set; }

        [JsonProperty("md")]
        public string Medallion { get; set; }

        [JsonProperty("ms")]
        public MeterStates MeterState { get; set; }

        [JsonProperty("ls")]
        public LogonStates LogonState { get; set; }

        [JsonProperty("es")]
        public EHailStates EHailState { get; set; }

        [JsonProperty("fn")]
        public string FleetName { get; set; }

        [JsonProperty("fi")]
        public int FleetId { get; set; }

        [JsonProperty("vl")]
        public int VehicleType { get; set; }

        [JsonProperty("mk")]
        public string Market { get; set; }
    }
}
