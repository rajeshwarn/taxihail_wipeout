#region

using System;
using apcurium.MK.Common.Enumeration;

#endregion

namespace apcurium.MK.Common.Extensions
{
    public static class DoubleExtensions
    {
        public static double ToRad(this double number)
        {
            return number*Math.PI/180;
        }

        public static double ToDistanceInRightUnit(this double distanceInMeters, DistanceFormat settingsDistanceFormat)
        {
            double result = 0;
            double distanceInKm = distanceInMeters / 1000;

            switch (settingsDistanceFormat)
            {
                case DistanceFormat.Mile:
                    result = Math.Round(distanceInKm / 1.609344, 1);
                    break;
                case DistanceFormat.Km:
                    result = Math.Round(distanceInKm, 1);
                    break;
                default:
                    result = Math.Round(distanceInKm, 1);
                    break;
            }

            return result;
        }
    }
}