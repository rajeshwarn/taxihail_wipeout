using Android.App;
using Android.Util;
using apcurium.MK.Booking.Mobile.Style;
using Android.Content.Res;
using Android.Graphics;
using Android.Widget;

namespace apcurium.MK.Callbox.Mobile.Client.Helpers
{
    public static class DrawHelper
    {

        public static int ToPixels(this int dip)
        {
            return GetPixels(dip);
        }

        public static  int GetPixels(float dipValue)
        {
            var px = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, dipValue , Application.Context.Resources.DisplayMetrics); // getDisplayMetrics());
            return px;
        }

        public static  int GetPixelsFromPt(float ptValue)
        {
            var px = (int)TypedValue.ApplyDimension(ComplexUnitType.Pt, ptValue , Application.Context.Resources.DisplayMetrics); // getDisplayMetrics());
            return px;
        }

        public static Color ConvertToColor( this ColorDefinition colorDef )
        {
            return new Color( colorDef.Red, colorDef.Green, colorDef.Blue , colorDef.Alpha );
        }

		public static void SupportLoginTextColor(TextView textView)
		{
			var states = new int[1][];
			states[0] = new int[0];
			var colors = new[] { (int)GetTextColorForBackground(textView.Resources.GetColor(Resource.Color.login_color)) };
			var colorList = new ColorStateList(states, colors);
			textView.SetTextColor(colorList);
		}

		public static Color GetTextColorForBackground(Color backgroundColor)
		{
			var darknessScore = (((backgroundColor.R) * 299) + ((backgroundColor.G) * 587) + ((backgroundColor.B) * 114)) / 1000;

			return darknessScore >= 125 
				? Color.Black
				: Color.White;
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