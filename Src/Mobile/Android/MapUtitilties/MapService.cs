using Android.App;
using Android.GoogleMaps;
using Android.Graphics.Drawables;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Client.Converters;
using apcurium.MK.Booking.Api.Contract.Resources;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;
using System;


namespace apcurium.MK.Booking.Mobile.Client.MapUtitilties
{
    public static class MapService
    {


        public static GeoPoint SetLocationOnMap(MapView map, Address location)
        {
            var point = new GeoPoint(0, 0);
            if (location != null)
            {
                point = new GeoPoint(CoordinatesConverter.ConvertToE6(location.Latitude), CoordinatesConverter.ConvertToE6(location.Longitude));
                if (point != map.MapCenter)
                {
                    map.Controller.AnimateTo(point);
                }
            }

            return point;
        }

        public static void AddPushPin(MapView map, Drawable mapPin, Address location, Activity activity, string title)
        {

            if (location != null)
            {
                var point = new GeoPoint(CoordinatesConverter.ConvertToE6(location.Latitude),
                                         CoordinatesConverter.ConvertToE6(location.Longitude));

                map.Overlays.Clear();
                MapService.AddMyLocationOverlay(map, activity, null);
                var pushpinOverlay = new PushPinOverlay(map, mapPin, title, point);
                map.Overlays.Add(pushpinOverlay);
            }

        }

        public static void AddMyLocationOverlay(MapView map, Activity activity, Func<bool> needToRunOnFirstFix )
        {
            var overlay = new MyLocationOverlay(activity, map);
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
            return new GeoPoint(CoordinatesConverter.ConvertToE6(latitude), CoordinatesConverter.ConvertToE6(longitude));
        }

      

    }
}

