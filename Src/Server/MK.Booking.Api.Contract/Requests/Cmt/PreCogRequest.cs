using System;
using System.ComponentModel;

namespace apcurium.MK.Booking.Api.Contract.Requests.Cmt
{
    public class PreCogRequest
    {
        public PreCogType Type { get; set; }
        public bool Init { get; set; }
        public DateTime LocTime { get; set; }
        public double? LocLat { get; set; }
        public double? LocLon { get; set; }
        public string LocDesc { get; set; }
        public string LinkedVehiculeId { get; set; }
        public double? GpsLat { get; set; }
        public double? GpsLon { get; set; }
        public double? GpsSpeed { get; set; }
        public double? GpsBearing { get; set; }
        public double? GpsAccuracy { get; set; }
        public double? GpsAltitude { get; set; }
        public double? DestLat { get; set; }
        public double? DestLon { get; set; }
        public string DestDesc { get; set; }
    }

    public enum PreCogType
    {
        Status,
        Ehail,
        [Description("Guide-Share")]
        Guide,
        Connect,
        [Description("Cancel-Ehail")]
        CancelEhail,
        Broadcast
    }
}