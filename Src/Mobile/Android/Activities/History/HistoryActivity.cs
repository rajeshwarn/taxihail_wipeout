using Android.App;
using Android.OS;
using Android.Widget;
using TinyIoC;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Activities.History
{
    [Activity(Label = "History", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class HistoryListActivity : BaseBindingActivity<HistoryViewModel> 
    {
        private TinyMessageSubscriptionToken _closeViewToken;       
        private ListView _listView;

        protected override int ViewTitleResourceId
        {
            get { return Resource.String.View_HistoryList; }
        }
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            _closeViewToken = TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Subscribe<CloseViewsToRoot>(m => Finish());
            _listView = FindViewById<ListView>(Resource.Id.HistoryList);
            _listView.Divider = null;
            _listView.DividerHeight = 0;
            _listView.SetPadding(10, 0, 10, 0);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_closeViewToken != null)
            {
                TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Unsubscribe<CloseViewsToRoot>(_closeViewToken);
            }
        }

        protected override void OnRestart()
        {
            base.OnRestart();
        }

        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_HistoryList);
        }
    }
}