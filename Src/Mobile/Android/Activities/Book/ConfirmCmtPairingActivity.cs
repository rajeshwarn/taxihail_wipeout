using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Activities;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Mobile;

namespace apcurium.MK.Booking.Mobile.Client
{
    [Activity(Label = "ConfirmCmtPairingActivity", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class ConfirmCmtPairingActivity : BaseBindingActivity<ConfirmCmtPairingViewModel>
    {
        protected override void OnStart()
        {
            base.OnStart();
        }

        protected override int ViewTitleResourceId
        {
            get { return Resource.String.View_Payments_ConfirmCmtPairTitle; }
        }


        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_Payments_ConfirmCmtPair);
            ViewModel.Load();
        }
    }
}