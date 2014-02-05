using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Droid.Views;
using TinyIoC;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Activities.GeoLocation
{
	[Activity(Label = "Locations", Theme = "@style/MainTheme",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class LocationListActivity : BaseBindingActivity<LocationListViewModel>
    {
        private TinyMessageSubscriptionToken _closeViewToken;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            _closeViewToken = TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Subscribe<CloseViewsToRoot>(m => Finish());
        }

        protected override void OnViewModelSet()
        {
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
                TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Unsubscribe<CloseViewsToRoot>(_closeViewToken);
            }
        }

    }
}