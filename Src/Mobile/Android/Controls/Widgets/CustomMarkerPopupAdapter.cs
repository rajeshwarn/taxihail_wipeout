using Android.Views;
using Android.Widget;
using Google.Android.M4b.Maps;
using Google.Android.M4b.Maps.Model;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public class CustomMarkerPopupAdapter : Java.Lang.Object, GoogleMap.IInfoWindowAdapter
    {
        private readonly LayoutInflater _layoutInflater;

        public CustomMarkerPopupAdapter(LayoutInflater inflater)
        {
            _layoutInflater = inflater;
        }

        public View GetInfoWindow(Marker marker)
        {
            return null;
        }

        public View GetInfoContents(Marker marker)
        {
            var customPopup = _layoutInflater.Inflate(Resource.Layout.VehicleInfoWindow, null);

            var titleTextView = customPopup.FindViewById<TextView>(Resource.Id.vehicleNumberTitle);
            if (titleTextView != null)
            {
                titleTextView.Text = marker.Title;
            }

            return customPopup;
        }
    }
}