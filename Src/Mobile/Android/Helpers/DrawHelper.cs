using Android.App;
using Android.Graphics;
using Android.Util;
using apcurium.MK.Booking.Mobile.Style;

namespace apcurium.MK.Booking.Mobile.Client.Helpers
{
    public static class DrawHelper
    {
        public static int ToPixels(this float dip)
        {
            return GetPixels(dip);
        }

        public static int ToPixels(this int dip)
        {
            return GetPixels(dip);
        }

        public static int GetPixels(float dipValue)
        {
            var px =
                (int)
                    TypedValue.ApplyDimension(ComplexUnitType.Dip, dipValue,
                        Application.Context.Resources.DisplayMetrics); // getDisplayMetrics());
            return px;
        }

        public static int GetPixelsFromPt(float ptValue)
        {
            var px =
                (int)
                    TypedValue.ApplyDimension(ComplexUnitType.Pt, ptValue, Application.Context.Resources.DisplayMetrics);
                // getDisplayMetrics());
            return px;
        }

        public static Color ConvertToColor(this ColorDefinition colorDef)
        {
            return new Color(colorDef.Red, colorDef.Green, colorDef.Blue, colorDef.Alpha);
        }

//        public static int[] ConvertToIntArray(this ColorDefinition[] colorDef ) 
//        {
//             
//            // Store integer 182
//            int decValue = 182;
//            // Convert integer 182 as a hex in a string variable
//            string hexValue = decValue.ToString("X");
//            // Convert the hex string back to the number
//            int decAgain = int.Parse(hexValue, System.Globalization.NumberStyles.HexNumber);
//        }
//
//        public static int ConvertToIntArray(this ColorDefinition colorDef ) 
//        {
//            
//            // Store integer 182
//            int decValue = 182;
//            // Convert integer 182 as a hex in a string variable
//            string hexValue = decValue.ToString("X");
//            // Convert the hex string back to the number
//            int decAgain = int.Parse(hexValue, System.Globalization.NumberStyles.HexNumber);
//        }
//
    }
}