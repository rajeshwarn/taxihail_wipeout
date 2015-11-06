using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using apcurium.MK.Common.Enumeration;
using Com.Mapbox.Mapboxsdk.Annotations;
using Com.Mapbox.Mapboxsdk.Views;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public class CustomMarkerPopupAdapter : Java.Lang.Object, MapView.IInfoWindowAdapter
    {
        private const int BottomMargin = 5;

        private readonly LayoutInflater _layoutInflater;
        private readonly Resources _resources;
		private readonly bool _addBottomMargin;
        private readonly string _market;

        public CustomMarkerPopupAdapter(LayoutInflater inflater, bool addBottomMargin, Resources resources, string market)
        {
            _layoutInflater = inflater;
            _resources = resources;
			_addBottomMargin = addBottomMargin;
            _market = market;
        }

        public View GetInfoWindow(Marker marker)
        {
			var customPopup = _layoutInflater.Inflate(Resource.Layout.VehicleInfoWindow, null);

            var medaillonView = customPopup.FindViewById<LinearLayout>(Resource.Id.vehicleNumberLayout);
            medaillonView.Background = GetMedaillonBackgroundColor(_market);

			var titleTextView = customPopup.FindViewById<TextView>(Resource.Id.vehicleNumberTitle);

			if (titleTextView != null)
			{
				titleTextView.Text = marker.Title;
			}

			var vehicleNumberLayout = customPopup.FindViewById<View>(Resource.Id.vehicleNumberLayout);

			var vehicleNumberMarginLayountParameters = (ViewGroup.MarginLayoutParams)vehicleNumberLayout.LayoutParameters;
			vehicleNumberMarginLayountParameters.SetMargins(vehicleNumberMarginLayountParameters.LeftMargin, vehicleNumberMarginLayountParameters.TopMargin,
                vehicleNumberMarginLayountParameters.RightMargin, _addBottomMargin ? BottomMargin : 0);
			vehicleNumberLayout.RequestLayout();

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