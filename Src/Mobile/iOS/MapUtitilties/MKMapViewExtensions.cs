using System;
using MonoTouch.CoreLocation;
using MonoTouch.Foundation;
using apcurium.MK.Booking.Mobile.Client.MapUtitilties;

namespace MonoTouch.MapKit
{
    public static class MKMapViewExtensions
    {
        const double MERCATOR_OFFSET = 268435456;
        const double MERCATOR_RADIUS = 85445659.44705395;

        /// <summary>
        /// Sets the map region to given Google Maps zoom level
        /// Will convert the zoom level to the correct MKCoordinateSpan
        /// Original Objective-C code found here: http://troybrant.net/blog/2010/01/set-the-zoom-level-of-an-mkmapview/
        /// </summary>
        public static void SetCenterCoordinate(this MKMapView mapView, CLLocationCoordinate2D centerCoordinate, float zoomLevel, bool animated)
        {
            // clamp large numbers to 28
            zoomLevel = Math.Min(zoomLevel, 28);

            // use the zoom level to compute the region
            var span = CoordinateSpanWithMapView(mapView, centerCoordinate, zoomLevel);
            var region = new MKCoordinateRegion(centerCoordinate, span);

            // set the region like normal
            mapView.SetRegion(region, animated);
        }

        public static void ChangeZoomLevel(this MKMapView mapView, bool increase)
        {
            var currentZoomLevel = GetZoomLevelFromRegion (mapView, mapView.Region);

            if (increase)
            {
                currentZoomLevel += 1;
            }
            else
            {
                currentZoomLevel -= 1;
            }

            // zoom level is between 0 and 20
            currentZoomLevel = (float)Math.Round(Math.Max(0f, Math.Min(20f, currentZoomLevel)), 0);

            SetCenterCoordinate (mapView, mapView.CenterCoordinate, currentZoomLevel, true);
        }

        public static void ChangeRegionSpanDependingOnPinchScale(this MKMapView mapView, MKCoordinateSpan originalSpan, float scale)
        {
            var centerPixelX = LongitudeToPixelSpaceX (mapView.Region.Center.Longitude);
            var centerPixelY = LatitudeToPixelSpaceY (mapView.Region.Center.Latitude);

            var topLeftLongitude = mapView.Region.Center.Longitude - (originalSpan.LongitudeDelta / 2);
            var topLeftLatitude = mapView.Region.Center.Latitude - (originalSpan.LatitudeDelta / 2);

            var topLeftPixelX = LongitudeToPixelSpaceX (topLeftLongitude);
            var topLeftPixelY = LatitudeToPixelSpaceY (topLeftLatitude);

            var deltaPixelX = Math.Abs(centerPixelX - topLeftPixelX);
            var deltaPixelY = Math.Abs(centerPixelY - topLeftPixelY);

            var scaledDeltaPixelX = deltaPixelX / scale;
            var scaledDeltaPixelY = deltaPixelY / scale;

            var newLongitudeForTopLeft = PixelSpaceXToLongitude (centerPixelX - scaledDeltaPixelX);
            var newLatitudeForTopLeft = PixelSpaceYToLatitude (centerPixelY - scaledDeltaPixelY);

            var newDeltaLongitude = Math.Abs(newLongitudeForTopLeft - mapView.Region.Center.Longitude) * 2;
            var newDeltaLatitude = Math.Abs(newLatitudeForTopLeft - mapView.Region.Center.Latitude) * 2;

            var region = new MKCoordinateRegion(mapView.Region.Center, new MKCoordinateSpan (newDeltaLatitude, newDeltaLongitude));

            mapView.SetRegion (region, false);
        }

        private static double LongitudeToPixelSpaceX(double longitude)
        {
            return Math.Round(MERCATOR_OFFSET + MERCATOR_RADIUS * longitude * Math.PI / 180.0);
        }

        private static double LatitudeToPixelSpaceY(double latitude)
        {
            return Math.Round(MERCATOR_OFFSET - MERCATOR_RADIUS * Math.Log((1 + Math.Sin(latitude * Math.PI / 180.0)) / (1 - Math.Sin(latitude * Math.PI / 180.0))) / 2.0);
        }

        private static double PixelSpaceXToLongitude(double pixelX)
        {
            return ((Math.Round(pixelX) - MERCATOR_OFFSET) / MERCATOR_RADIUS) * 180.0 / Math.PI;
        }

        private static double PixelSpaceYToLatitude(double pixelY)
        {
            return (Math.PI / 2.0 - 2.0 * Math.Atan(Math.Exp((Math.Round(pixelY) - MERCATOR_OFFSET) / MERCATOR_RADIUS))) * 180.0 / Math.PI;
        }

        private static MKCoordinateSpan CoordinateSpanWithMapView(MKMapView mapView, CLLocationCoordinate2D centerCoordinate, float zoomLevel)
        {
            // convert center coordiate to pixel space
            double centerPixelX = LongitudeToPixelSpaceX(centerCoordinate.Longitude);
            double centerPixelY = LatitudeToPixelSpaceY(centerCoordinate.Latitude);

            // determine the scale value from the zoom level
            var zoomExponent = 20 - zoomLevel;
            double zoomScale = Math.Pow(2, zoomExponent);

            // scale the map’s size in pixel space
            var mapSizeInPixels = mapView.Bounds.Size;
            double scaledMapWidth = mapSizeInPixels.Width * zoomScale;
            double scaledMapHeight = mapSizeInPixels.Height * zoomScale;

            // figure out the position of the top-left pixel
            double topLeftPixelX = centerPixelX - (scaledMapWidth / 2);
            double topLeftPixelY = centerPixelY - (scaledMapHeight / 2);

            // find delta between left and right longitudes
            double minLng = PixelSpaceXToLongitude(topLeftPixelX);
            double maxLng = PixelSpaceXToLongitude(topLeftPixelX + scaledMapWidth);
            double longitudeDelta = maxLng - minLng;

            // find delta between top and bottom latitudes
            double minLat = PixelSpaceYToLatitude(topLeftPixelY);
            double maxLat = PixelSpaceYToLatitude(topLeftPixelY + scaledMapHeight);
            double latitudeDelta = -1 * (maxLat - minLat);

            // create and return the lat/lng span
            var span = new MKCoordinateSpan(latitudeDelta, longitudeDelta);
            return span;
        }

        private static float GetZoomLevelFromRegion(MKMapView mapView, MKCoordinateRegion region)
        {
            var centerPixelX = LongitudeToPixelSpaceX (region.Center.Longitude);
            var x = LongitudeToPixelSpaceX (region.Center.Longitude - region.Span.LongitudeDelta);

            var topLeftPixelX = (x + centerPixelX) / 2;

            var scaledMapWidth = (centerPixelX - topLeftPixelX) * 2;

            var zoomScale = scaledMapWidth / mapView.Bounds.Size.Width;

            var zoomExponent = Math.Log(zoomScale, 2);

            return 20 - (int)zoomExponent;
        }
    }
}

