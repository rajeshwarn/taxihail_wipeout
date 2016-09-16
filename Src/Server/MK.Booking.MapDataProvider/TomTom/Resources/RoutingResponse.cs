using System;
using System.Collections.Generic;

namespace apcurium.MK.Booking.MapDataProvider.TomTom.Resources
{
    /// <summary>
    /// See for more properties http://developer.tomtom.com/docs/read/map_toolkit/web_services/routing/Response
    /// </summary>
    public class RoutingResponse
    {
        public List<Route> Routes { get; set; }
    }

    public class Route
    {
        public Summary Summary { get; set; }
    }

    public class Summary
    {
        public int trafficDelayInSeconds { get; set; }
        public int lengthInMeters { get; set; }
        public int travelTimeInSeconds { get; set; }
    }

}

