using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.Interfaces.Views;
using Cirrious.MvvmCross.Views;
using System;
using System.Collections.Generic;
using TinyIoC;
using TinyMessenger;

namespace apcurium.MK.Booking.Mobile.Client.Activities.History
{
    [Activity(Label = "History Details", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
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
            _closeViewToken = TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Subscribe<CloseViewsToRoot>(m => Finish());
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
