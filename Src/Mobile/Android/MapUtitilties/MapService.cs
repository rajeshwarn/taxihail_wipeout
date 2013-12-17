using Android.GoogleMaps;
using Android.Graphics.Drawables;
using apcurium.MK.Booking.Mobile.Client.Converters;
using System;
using Android.Content;
using apcurium.MK.Common.Entity;
using Android.Gms.Maps.Model;


namespace apcurium.MK.Booking.Mobile.Client.MapUtitilties
{
    public static class MapService
    {
		
		public static PushPinOverlay AddPushPin(MapView map, Drawable mapPin, Address location, string title)
        {
            PushPinOverlay pushpinOverlay = null;
            if (location != null)
            {
				//pushpinOverlay = AddPushPin (map, mapPin, new LatLng(location.Latitude, location.Longitude), title);
            }
            return pushpinOverlay;

        }

		public static PushPinOverlay AddPushPin(MapView map, Drawable mapPin, LatLng point, string title)
		{
			PushPinOverlay pushpinOverlay = null;
			if (point != null)
			{
				//pushpinOverlay  = new PushPinOverlay(map, mapPin, title, point);
				map.Overlays.Add(pushpinOverlay);
			}
			return pushpinOverlay;

		}

        public static void AddMyLocationOverlay(MapView map, Context context, Func<bool> needToRunOnFirstFix)
        {
            var overlay = new MyLocationOverlay(context, map);
            var func = needToRunOnFirstFix;
            overlay.RunOnFirstFix(() =>
                {
                    
                    if (( func != null )  && (func()) )
                    {
                        map.Controller.SetZoom(15);
                        map.Controller.AnimateTo(overlay.MyLocation);
                    }
                });   
            overlay.EnableMyLocation();
            map.Overlays.Add(overlay);
        }

    }
}

