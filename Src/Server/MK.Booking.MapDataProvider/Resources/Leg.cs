#region

using System.Collections.Generic;

#endregion

namespace apcurium.MK.Booking.MapDataProvider.Resources
{
    public class Leg
    {
        public Distance Distance { get; set; }
        public Duration Duration { get; set; }
        public string End_address { get; set; }
        public Location End_location { get; set; }
        public string Start_address { get; set; }
        public Location Start_location { get; set; }
        public List<Step> Steps { get; set; }
    }
}