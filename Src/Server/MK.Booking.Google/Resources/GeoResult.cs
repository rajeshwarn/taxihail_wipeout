﻿#region

using System.Collections.Generic;

#endregion

namespace apcurium.MK.Booking.Google.Resources
{
    public class GeoResult
    {
        public ResultStatus Status { get; set; }
        public List<GeoObj> Results { get; set; }
    }
}