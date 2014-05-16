#region

using System.Collections.Generic;

#endregion

namespace apcurium.MK.Booking.MapDataProvider.Google.Resources	
{
    public class Route
    {
        public Bounds Bounds { get; set; }
        public string Copyrights { get; set; }
        public List<Leg> Legs { get; set; }
        public OverviewPolyline Overview_polyline { get; set; }
        public string Summary { get; set; }
    }
}