using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.GoogleMaps;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Controls;
using Android.Gms.Maps.Model;

namespace apcurium.MK.Booking.Mobile.Client.MapUtitilties
{
	public class PushPinOverlay : ItemizedOverlay
    {
        private OverlayItem _item;

		public PushPinOverlay(MapView owner, Drawable imagePushPint, string title, GeoPoint point)
            : base(imagePushPint)
        {
			_item = new OverlayItem(point, title, null);
        }

        public string Title { get { return _item.Title; } }

        public override int Size()
        {
            return 1;
        }

		protected override Java.Lang.Object CreateItem(int i)
		{
			return _item;
		}

    }
}
