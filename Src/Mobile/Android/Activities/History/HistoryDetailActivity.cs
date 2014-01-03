using Android.App;
using Android.Content.PM;
using Android.OS;

using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Mobile.ViewModels;
using TinyIoC;
using TinyMessenger;

namespace apcurium.MK.Booking.Mobile.Client.Activities.History
{
    [Activity(Label = "History Details", Theme = "@android:style/Theme.NoTitleBar",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class HistoryDetailActivity : BaseBindingActivity<HistoryDetailViewModel>
    {
        private TinyMessageSubscriptionToken _closeViewToken;

        protected override int ViewTitleResourceId
        {
            get { return Resource.String.View_HistoryDetail; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            _closeViewToken =
                TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Subscribe<CloseViewsToRoot>(m => Finish());
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_closeViewToken != null)
            {
                TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Unsubscribe<CloseViewsToRoot>(_closeViewToken);
            }
        }

        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_HistoryDetail);
            ViewModel.Load();
        }
    }
}