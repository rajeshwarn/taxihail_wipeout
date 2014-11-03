using System;
using System.Collections.Generic;
using System.Text;
using apcurium.MK.Booking.Maps.Geo;

namespace apcurium.MK.Booking.Maps.Impl
{
    public static class PathUtility
    {
        public static string Encode(IEnumerable<Position> points)
        {
            try
            {
                var str = new StringBuilder();

                Action<int> encodeDiff = (diff) =>
                {
                    var shifted = diff << 1;
                    if (diff < 0)
                    {
                        shifted = ~shifted;
                    }

                    var rem = shifted;
                    while (rem >= 0x20)
                    {
                        str.Append((char)((0x20 | (rem & 0x1f)) + 63));
                        rem >>= 5;
                    }
                    str.Append((char)(rem + 63));
                };

                var lastLat = 0;
                var lastLng = 0;
                foreach (var point in points)
                {
                    var lat = (int)Math.Round(point.Latitude * 1E5);
                    var lng = (int)Math.Round(point.Longitude * 1E5);
                    encodeDiff(lat - lastLat);
                    encodeDiff(lng - lastLng);
                    lastLat = lat;
                    lastLng = lng;
                }
                return str.ToString();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}