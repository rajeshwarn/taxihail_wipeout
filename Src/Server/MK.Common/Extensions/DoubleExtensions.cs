#region

using System;

#endregion

namespace apcurium.MK.Common.Extensions
{
    public static class DoubleExtensions
    {
        public static double ToRad(this double number)
        {
            return number*Math.PI/180;
        }
    }
}