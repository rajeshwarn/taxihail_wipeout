#region

using System;
using apcurium.MK.Common.Extensions;

#endregion

namespace apcurium.MK.Booking.Maps.Geo
{
    public struct Position
    {
        private readonly double _latitude;
        private readonly double _longitude;

        public Position(double latitude, double longitude)
        {
            _latitude = latitude;
            _longitude = longitude;
        }

        public double Latitude
        {
            get { return _latitude; }
        }

        public double Longitude
        {
            get { return _longitude; }
        }

        public double DistanceTo(Position other)
        {
            return CalculateDistance(Latitude, Longitude, other.Latitude, other.Longitude);
        }

        public static double CalculateDistance(double latitude1, double longitude1, double latitude2, double longitude2)
        {
            const int r = 6378137;
            var d =
                Math.Acos(Math.Sin(latitude2.ToRad())*Math.Sin(latitude1.ToRad()) +
                          Math.Cos(latitude2.ToRad())*Math.Cos(latitude1.ToRad())*
                          Math.Cos(longitude1.ToRad() - longitude2.ToRad()))*r;
            return d;
        }
    }
}