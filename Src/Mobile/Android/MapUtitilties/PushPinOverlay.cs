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

namespace apcurium.MK.Booking.Mobile.Client.MapUtitilties
{
    public class PushPinOverlay : ItemizedOverlay
    {
        private OverlayItem _item;
        private string _title;
        private MapView _owner;

        public PushPinOverlay(MapView owner, Drawable imagePushPint, string title, GeoPoint point)
            : base(imagePushPint)
        {
            _owner = owner;
			_item = new OverlayItem(point, title, null);
   			_title = title;
			BoundCenterBottom(imagePushPint);
            Populate();
        }

        public string Title { get { return _item.Title; } }

        protected override Java.Lang.Object CreateItem(int i)
        {
            return _item;
        }

        public override int Size()
        {
            return 1;
        }

        private LinearLayout _noteBaloon;
        private void RemoveAllBalloons(MapView mapView)
        {
            if (mapView.Overlays != null)
            {
                foreach (var i in mapView.Overlays.OfType<PushPinOverlay>())
                {
                    i.RemoveBaloon();
                }
            }
        }

        public void RemoveBaloon()
        {
            if (_noteBaloon != null)
            {
                _noteBaloon.Visibility = ViewStates.Gone;
            }
        }

		public override void Draw (Android.Graphics.Canvas canvas, MapView mapView, bool shadow)
		{
			base.Draw (canvas, mapView, false);
		}
    }
}
