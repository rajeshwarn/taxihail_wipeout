using System;

namespace CMTServices.Responses
{
    public class VehicleResponse
    {
        public DateTime Timestamp { get; set; }

        public float Latitude { get; set; }

        public float Longitude { get; set; }

        public float? CompassCourse { get; set; }

        public string Medallion { get; set; }

        public int FleetId { get; set; }

        public long? Eta { get; set; }

        public long? DistanceToArrival { get; set; }

        public string DeviceName { get; set; }

        public string LegacyDispatchId { get; set; }

        public int VehicleType { get; set; }

        public string Market { get; set; }
    }
}
