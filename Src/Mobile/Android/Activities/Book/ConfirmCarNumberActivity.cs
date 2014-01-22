using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.ViewModels;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Client.Activities;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "ConfirmCarNumberActivity", Theme = "@style/MainTheme",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class ConfirmCarNumberActivity : BaseBindingActivity<ConfirmCarNumberViewModel>
    {
        protected override int ViewTitleResourceId
        {
            get { return Resource.String.PaymentView; }
        }

        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_Payments_ConfirmCarNumber);
            ViewModel.OnViewLoaded();
        }
    }
}