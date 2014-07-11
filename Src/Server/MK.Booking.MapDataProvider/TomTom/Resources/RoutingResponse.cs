using System;

namespace apcurium.MK.Booking.MapDataProvider.TomTom.Resources
{
    /// <summary>
    /// See for more properties http://developer.tomtom.com/docs/read/map_toolkit/web_services/routing/Response
    /// </summary>
    public class RoutingResponse
    {
        public Route Route { get; set; }
    }

    public class Route 
    {
        public Summary Summary { get; set; }
    }

    public class Summary
    {
        public int TotalDelaySeconds { get; set; }
        public int TotalDistanceMeters { get; set; }
        public int TotalTimeSeconds { get; set; }
    }
}

