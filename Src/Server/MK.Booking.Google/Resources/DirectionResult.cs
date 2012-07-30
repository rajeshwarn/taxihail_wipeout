using System.Collections.Generic;

namespace apcurium.MK.Booking.Google.Resources
{
    public class DirectionResult
    {
        public List<Route> Routes { get; set; }
        public ResultStatus Status { get; set; }
    }
}