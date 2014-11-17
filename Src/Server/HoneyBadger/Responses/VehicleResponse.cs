using System;

namespace HoneyBadger.Responses
{
    public class VehicleResponse
    {
        public DateTime Timestamp { get; set; }

        public float Latitude { get; set; }

        public float Longitude { get; set; }

        public string Medaillon { get; set; }
    }
}
