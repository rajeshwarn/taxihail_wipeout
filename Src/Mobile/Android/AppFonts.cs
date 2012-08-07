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
using Android.Graphics;

namespace apcurium.MK.Booking.Mobile.Client
{
    public static class AppFonts
    {
        private static Typeface _typefaceNormal;
        private static Typeface _typefaceMedium;
        private static Typeface _typefaceBold;

        public static Typeface Regular
        {
            get
            {
                if (_typefaceNormal == null)
                {
                    _typefaceNormal = Typeface.CreateFromAsset(AppContext.Current.App.Assets, "HelveticaNeueLTPro-Roman2.otf");
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
                    _typefaceMedium = Typeface.CreateFromAsset(AppContext.Current.App.Assets, "HelveticaNeueLTPro-Md.otf");
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
                    _typefaceBold = Typeface.CreateFromAsset(AppContext.Current.App.Assets, "HelveticaNeueLTPro-Bd.otf");
                }
                return _typefaceBold;
            }
        }

    }
}