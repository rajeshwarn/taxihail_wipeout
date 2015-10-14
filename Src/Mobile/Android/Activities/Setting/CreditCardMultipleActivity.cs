using System;
using Android.App;
using Android.OS;
using Android.Content.PM;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Setting
{
    [Activity(Label = "@string/CreditCardMultipleActivityName", 
        Theme = "@style/MainTheme",
        ScreenOrientation = ScreenOrientation.Portrait
    )]
    public class CreditCardMultipleActivity : BaseBindingActivity<CreditCardMultipleViewModel>
    {
        protected override async void OnViewModelSet()
        {
            base.OnViewModelSet ();

            SetContentView(Resource.Layout.View_Payments_CreditCardMultiple);
        }
    }
}

