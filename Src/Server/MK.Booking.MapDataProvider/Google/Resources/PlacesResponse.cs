﻿#region

using System.Collections.Generic;

#endregion

namespace apcurium.MK.Booking.MapDataProvider.Google.Resources	
{
    public class PlacesResponse : GoogleResult
    {
        public IList<Place> Results { get; set; }
        public List<string> HtmlAttributions { get; set; }
        public string NextPageToken { get; set; }
    }
}