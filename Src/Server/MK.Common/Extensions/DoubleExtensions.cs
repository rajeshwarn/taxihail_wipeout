using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Common.Extensions
{
    public static class DoubleExtensions
    {
        public static double ToRad(this double number)
        {
            return number * Math.PI / 180;
        }
        
        
        public static string ToDollars(this double number)
        {
            return number.ToString("C");
        }
    }
}