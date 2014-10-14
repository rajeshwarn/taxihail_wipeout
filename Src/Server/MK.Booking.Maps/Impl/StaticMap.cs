using System;
using apcurium.MK.Booking.Maps.Geo;

namespace apcurium.MK.Booking.Maps.Impl
{
    public class StaticMap : IStaticMap
    {
        public string GetStaticMapUri(Position pickup, Position dropOff, float width, float height, float scale)
        {
            // When we decide to show the route between pickup and dropoff, use encodedPolylines
            var encodedPolylines = "";

            var dropOffString = string.Empty;
            if (dropOff.Latitude != 0
                && dropOff.Longitude != 0)
            {
                dropOffString = string.Format("&markers=color:0xFF0000|size:medium|{0}", dropOff.ToString());
            }

            return Uri.EscapeUriString(string.Format("http://maps.googleapis.com/maps/api/staticmap" +
                "?markers=color:0x1EC022|size:medium|{3}" +
                "{4}" +
                "&size={0}x{1}" +
                "&scale={2}" +
                "{5}"
                , width, height, scale, pickup.ToString(), dropOffString, encodedPolylines));
        }
    }
}