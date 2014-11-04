using System;
using System.Collections.Generic;
using System.Text;
using apcurium.MK.Booking.Maps.Geo;

namespace apcurium.MK.Booking.Maps.Impl
{
    public static class PathUtility
    {
        public static string GetEncodedPolylines(IEnumerable<Position> points)
        {
            try
            {
                var str = new StringBuilder();

                var lastLat = 0;
                var lastLng = 0;
                foreach (var point in points)
                {
                    var lat = (int)Math.Round(point.Latitude * 1E5);
                    var lng = (int)Math.Round(point.Longitude * 1E5);
                    EncodeSignedValue(lat - lastLat, str);
                    EncodeSignedValue(lng - lastLng, str);
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

        /// <summary>
        /// Encode using https://developers.google.com/maps/documentation/utilities/polylinealgorithm
        /// </summary>
        private static void EncodeSignedValue(int value, StringBuilder str)
        {
            var shifted = value << 1;
            if (value < 0)
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
        }
    }
}