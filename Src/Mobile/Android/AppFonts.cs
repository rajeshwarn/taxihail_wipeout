using Android.App;
using Android.Graphics;

namespace apcurium.MK.Booking.Mobile.Client
{
    public static class AppFonts
    {
        private static Typeface _typefaceNormal;
        private static Typeface _typefaceMedium;
        private static Typeface _typefaceBold;
        private static Typeface _typefaceItalic;

        public static Typeface Regular
        {
            get
            {
                if (_typefaceNormal == null)
                {
                    _typefaceNormal = Typeface.CreateFromAsset(Application.Context.Assets, "AppFont_Regular.otf");
                }
                return _typefaceNormal;
            }
        }

        public static Typeface Medium
        {
            get
            {
                if (_typefaceMedium == null)
                {
                    _typefaceMedium = Typeface.CreateFromAsset(Application.Context.Assets, "AppFont_Medium.otf");
                }
                return _typefaceMedium;
            }
        }


        public static Typeface Bold
        {
            get
            {
                if (_typefaceBold == null)
                {
                    _typefaceBold = Typeface.CreateFromAsset(Application.Context.Assets, "AppFont_Bold.otf");
                }
                return _typefaceBold;
            }
        }

        public static Typeface Italic
        {
            get
            {
                if (_typefaceItalic == null)
                {
                    _typefaceItalic = Typeface.CreateFromAsset(Application.Context.Assets, "AppFont_Italic.otf");
                }
                return _typefaceItalic;
            }
        }
    }
}