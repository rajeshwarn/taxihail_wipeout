using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace MK.Common.Android.Extensions
{
    public static class DoubleExtensions
    {
        public static double ToRad(this double number)
        {
            return number * Math.PI / 180;
        }
    }
}