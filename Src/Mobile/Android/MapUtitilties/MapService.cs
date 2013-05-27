using Android.GoogleMaps;
using Android.Graphics.Drawables;
using apcurium.MK.Booking.Mobile.Client.Converters;
using System;
using Android.Content;
using apcurium.MK.Common.Entity;


namespace apcurium.MK.Booking.Mobile.Client.MapUtitilties
{
    public static class MapService
    {


        public static GeoPoint SetLocationOnMap(MapView map, Address location)
        {
            var point = new GeoPoint(0, 0);
            if (location != null)
            {
                point = new GeoPoint(CoordinatesHelper.ConvertToE6(location.Latitude), CoordinatesHelper.ConvertToE6(location.Longitude));
                if (point != map.MapCenter)
                {
                    map.Controller.AnimateTo(point);
                }
            }

            return point;
        }

        public static PushPinOverlay AddPushPin(MapView map, Drawable mapPin,Address location, string title)
        {
            PushPinOverlay pushpinOverlay = null;
            if (location != null)
            {
                pushpinOverlay = AddPushPin (map, mapPin, GetGeoPoint(location.Latitude, location.Longitude), title);
            }
            return pushpinOverlay;

        }

		public static PushPinOverlay AddPushPin(MapView map, Drawable mapPin, GeoPoint point, string title)
		{
			PushPinOverlay pushpinOverlay = null;
			if (point != null)
			{
				pushpinOverlay  = new PushPinOverlay(map, mapPin, title, point);
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

        public static GeoPoint GetGeoPoint(double latitude, double longitude)
        {
            return new GeoPoint(CoordinatesHelper.ConvertToE6(latitude), CoordinatesHelper.ConvertToE6(longitude));
        }

      

    }
}

