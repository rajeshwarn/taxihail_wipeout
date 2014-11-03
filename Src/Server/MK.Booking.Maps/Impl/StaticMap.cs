using System;
using apcurium.MK.Booking.Maps.Geo;

namespace apcurium.MK.Booking.Maps.Impl
{
    public class StaticMap : IStaticMap
    {
        public string GetStaticMapUri(Position pickup, Position dropOff, string encodedPath, float width, float height, float scale)
        {
            var encodedPolylinesParam = !string.IsNullOrWhiteSpace(encodedPath) 
                ? string.Format("&path=enc:{0}", encodedPath)
                : string.Empty;

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
                , width, height, scale, pickup.ToString(), dropOffString, encodedPolylinesParam));
        }
    }
}