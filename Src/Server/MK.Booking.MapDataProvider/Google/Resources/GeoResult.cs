#region

using System.Collections.Generic;

#endregion

namespace apcurium.MK.Booking.MapDataProvider.Google.Resources	
{
    public class GeoResult : GoogleResult
    {
        public List<GeoObj> Results { get; set; }
    }
}