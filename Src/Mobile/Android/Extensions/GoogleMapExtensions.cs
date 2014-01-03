using System;
using Android.Gms.Maps.Model;

namespace Android.Gms.Maps
{
	public static class GoogleMapExtensions
    {
		public static void MoveTo(this GoogleMap map, CameraUpdate cameraUpdate, bool animate = true)
		{
			if (map == null)
			{
				throw new ArgumentNullException("map"); 
			}

			if (animate)
			{
				map.AnimateCamera(cameraUpdate);
			}
			else
			{
				map.MoveCamera(cameraUpdate);
			}
		}

		public static void MoveTo(this GoogleMap map, double lat, double lng, float zoom, bool animate = true)
		{
			MoveTo(map, CameraUpdateFactory.NewLatLngZoom(new LatLng(lat, lng), zoom), animate);
		}

		public static void MoveTo(this GoogleMap map, double lat, double lng, bool animate = true)
		{
			MoveTo(map, CameraUpdateFactory.NewLatLng(new LatLng(lat, lng)), animate);
		}
    }
}

