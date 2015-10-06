using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using apcurium.MK.Common.Enumeration;
using Google.Android.M4b.Maps;
using Google.Android.M4b.Maps.Model;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public class CustomMarkerPopupAdapter : Java.Lang.Object, GoogleMap.IInfoWindowAdapter
    {
        private readonly LayoutInflater _layoutInflater;
        private readonly Resources _resources;
        private readonly string _market;

        public CustomMarkerPopupAdapter(LayoutInflater inflater, Resources resources, string market)
        {
            _layoutInflater = inflater;
            _resources = resources;
            _market = market;
        }

        public View GetInfoWindow(Marker marker)
        {
			var customPopup = _layoutInflater.Inflate(Resource.Layout.VehicleInfoWindow, null);

            var medaillonView = customPopup.FindViewById<LinearLayout>(Resource.Id.medaillonView);
            medaillonView.Background = GetMedaillonBackgroundColor(_market);

			var titleTextView = customPopup.FindViewById<TextView>(Resource.Id.vehicleNumberTitle);

			if (titleTextView != null)
			{
				titleTextView.Text = marker.Title;
			}

			return customPopup;
        }

        public View GetInfoContents(Marker marker)
        {
			return null;
        }

        private Drawable GetMedaillonBackgroundColor(string market)
        {
            if (!market.HasValue())
            {
                // Default grey color
                return _resources.GetDrawable(Resource.Drawable.default_vehicle_medaillon_icon);
            }

            switch (market.ToLower())
            {
                case AssignedVehicleMarkets.NYC:
                    return _resources.GetDrawable(Resource.Drawable.yellow_vehicle_medaillon_icon); // Yellow
                case AssignedVehicleMarkets.NYSHL:
                    return _resources.GetDrawable(Resource.Drawable.green_vehicle_medaillon_icon); // Green
                default:
                    return _resources.GetDrawable(Resource.Drawable.default_vehicle_medaillon_icon); // Grey
            }
        }
    }
}