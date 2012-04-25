using System;
using Android.GoogleMaps;
using Android.Content;
using Android.Graphics.Drawables;
using TaxiMobileApp;
using Microsoft.Practices.ServiceLocation;
using System.Linq;
using apcurium.Framework.Extensions;
using TaxiMobile.Converters;
using Android.Locations;
using Android.App;

namespace TaxiMobile.MapUtitilties
{
    public static class MapService
    {

        //public static LocationData[] ChangeLocation( MapView map, string address )
        //{
        //    var locations = ServiceLocator.Current.GetInstance<IBookingService>().SearchAddress(address);

        //    UpdateMap( map, locations );

        //    return locations;
        //}

        public static GeoPoint SetLocationOnMap(MapView map, LocationData location)
        {
            var point = new GeoPoint(0, 0);
            if ((location != null) && (location.Latitude.HasValue) && (location.Longitude.HasValue))
            {                
                point = new GeoPoint(CoordinatesConverter.ConvertToE6(location.Latitude.Value), CoordinatesConverter.ConvertToE6(location.Longitude.Value));
                if (point != map.MapCenter)
                {
                    map.Controller.AnimateTo(point);
                }
            }

            return point;
        }

        public static void AddPushPin(MapView map, Drawable mapPin, LocationData location, Activity activity, string title)
        {

                        if ( ( location != null ) && ( location.Latitude.HasValue )&& ( location.Longitude.HasValue ) )
                        {
                            var point = new GeoPoint(CoordinatesConverter.ConvertToE6(location.Latitude.Value),
                                                     CoordinatesConverter.ConvertToE6(location.Longitude.Value));

                            map.Overlays.Clear();
                            MapService.AddMyLocationOverlay(map, activity);                            
                            var pushpinOverlay = new PushPinOverlay( map , mapPin,title, point);
                            map.Overlays.Add(pushpinOverlay);
                        }
            
        }

        public static void AddMyLocationOverlay(MapView map, Activity activity)
        {
            var overlay = new MyLocationOverlay(activity, map);            
            overlay.EnableMyLocation();
            map.Overlays.Add(overlay);
        }

        public static GeoPoint GetGeoPoint(double latitude, double longitude)
        {
            return new GeoPoint(CoordinatesConverter.ConvertToE6(latitude), CoordinatesConverter.ConvertToE6(longitude));
        }

        //private static void UpdateMap( MapView map, LocationData[] locations )
        //{
        //    if (locations.Count() >= 1 && !locations[0].Address.IsNullOrEmpty() && locations[0].Longitude.HasValue && locations[0].Latitude.HasValue)
        //    {
        //        var point = new GeoPoint(CoordinatesConverter.ConvertToE6(locations[0].Latitude.Value), CoordinatesConverter.ConvertToE6(locations[0].Longitude.Value));
        //        if( point != map.MapCenter )
        //        {
        //            map.Controller.AnimateTo(point);
        //            Console.WriteLine( point.LatitudeE6.ToString() + " " + point.LongitudeE6.ToString() );
        //        }
        //    }			
        //}

    }
}

