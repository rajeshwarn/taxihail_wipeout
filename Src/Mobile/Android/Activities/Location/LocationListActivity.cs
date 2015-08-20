using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Mobile.ViewModels;
using TinyIoC;
using TinyMessenger;

namespace apcurium.MK.Booking.Mobile.Client.Activities.GeoLocation
{
	[Activity(Label = "@string/LocationsActivityName", Theme = "@style/MainTheme",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class LocationListActivity : BaseBindingActivity<LocationListViewModel>
    {
        private TinyMessageSubscriptionToken _closeViewToken;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

			_closeViewToken = this.Services().MessengerHub.Subscribe<CloseViewsToRoot>(m => Finish());
        }

		protected override void OnViewModelSet()
		{
			base.OnViewModelSet ();

            SetContentView(Resource.Layout.View_LocationList);
            var listView = FindViewById<ListView>(Resource.Id.LocationListView);
            listView.CacheColorHint = Color.White;
            listView.Divider = null;
            listView.DividerHeight = 0;
            listView.SetPadding(0, 0, 0, 0);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_closeViewToken != null)
            {
				this.Services().MessengerHub.Unsubscribe<CloseViewsToRoot>(_closeViewToken);
            }
        }

    }
}