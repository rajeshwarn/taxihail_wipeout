using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Mobile.ViewModels;
using TinyIoC;
using TinyMessenger;
using Cirrious.MvvmCross.Binding.Droid.Views;

namespace apcurium.MK.Booking.Mobile.Client.Activities.GeoLocation
{
    [Activity(Label = "Locations",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class LocationListActivity : BaseBindingActivity<MyLocationsViewModel>
    {
        private TinyMessageSubscriptionToken _closeViewToken;
        private ListView _listView;


        protected override int ViewTitleResourceId
        {
            get { return Resource.String.View_LocationList; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            _closeViewToken =
                TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Subscribe<CloseViewsToRoot>(m => Finish());
            UpdateUi();
        }

        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_LocationList);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_closeViewToken != null)
            {
                TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Unsubscribe<CloseViewsToRoot>(_closeViewToken);
            }
        }

        private void SetAdapter()
        {
            var adapter = new MvxAdapter(this);

            RunOnUiThread(() =>
            {
                ((MvxListView) _listView).Adapter = adapter;
                _listView.Divider = null;
                _listView.DividerHeight = 0;
                _listView.SetPadding(0, 0, 0, 0);
            });
        }

        private void UpdateUi()
        {
            _listView = FindViewById<ListView>(Resource.Id.LocationListView);
			_listView.CacheColorHint = Color.White;
        }

        protected override void OnResume()
        {
            base.OnResume();
            SetAdapter();
        }
    }
}