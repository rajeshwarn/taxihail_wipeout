using Android.Views;
using Android.Widget;
using Google.Android.M4b.Maps;
using Google.Android.M4b.Maps.Model;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public class CustomMarkerPopupAdapter : Java.Lang.Object, GoogleMap.IInfoWindowAdapter
    {
        private readonly LayoutInflater _layoutInflater;
		private readonly int _marginBottomOffset;

        public CustomMarkerPopupAdapter(LayoutInflater inflater, int marginBottomOffset)
        {
            _layoutInflater = inflater;
			_marginBottomOffset = marginBottomOffset;
        }

        public View GetInfoWindow(Marker marker)
        {
			var customPopup = _layoutInflater.Inflate(Resource.Layout.VehicleInfoWindow, null);
			
			var titleTextView = customPopup.FindViewById<TextView>(Resource.Id.vehicleNumberTitle);

			if (titleTextView != null)
			{
				titleTextView.Text = marker.Title;
			}

			var vehicleNumberLayout = customPopup.FindViewById<View>(Resource.Id.vehicleNumberLayout);

			var vehicleNumberMarginLayountParameters = (ViewGroup.MarginLayoutParams)vehicleNumberLayout.LayoutParameters;
			vehicleNumberMarginLayountParameters.SetMargins(vehicleNumberMarginLayountParameters.LeftMargin, vehicleNumberMarginLayountParameters.TopMargin, vehicleNumberMarginLayountParameters.RightMargin, _marginBottomOffset);
			vehicleNumberLayout.RequestLayout();

			return customPopup;
        }

        public View GetInfoContents(Marker marker)
        {
			return null;
        }
    }
}