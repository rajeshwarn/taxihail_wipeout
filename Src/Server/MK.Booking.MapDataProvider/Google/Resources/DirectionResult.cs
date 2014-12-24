#region

using System.Collections.Generic;

#endregion

namespace apcurium.MK.Booking.MapDataProvider.Google.Resources	
{
    public class DirectionResult : GoogleResult
    {
        public List<Route> Routes { get; set; }
    }
}