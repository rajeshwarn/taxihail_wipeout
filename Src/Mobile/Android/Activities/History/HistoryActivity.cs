using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Widget;

using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Activities.History
{
    [Activity(Label = "History", Theme = "@android:style/Theme.NoTitleBar",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class HistoryListActivity : BaseBindingActivity<HistoryViewModel>
    {
        private ListView _listView;

        protected override int ViewTitleResourceId
        {
            get { return Resource.String.View_HistoryList; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            _listView = FindViewById<ListView>(Resource.Id.HistoryList);
            _listView.Divider = null;
            _listView.DividerHeight = 0;
            _listView.SetPadding(10, 0, 10, 0);
        }

        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_HistoryList);
        }
    }
}