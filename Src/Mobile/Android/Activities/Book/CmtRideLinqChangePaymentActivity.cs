using Android.App;
using Android.Content.PM;
using Android.Widget;
using Android.Views;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.BindingContext;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;
using CrossUI.Droid.Dialog.Elements;
using CrossUI.Droid.Dialog;
using CrossUI.Droid;
using apcurium.MK.Booking.Mobile.ViewModels.Payment.Cmt;

namespace apcurium.MK.Booking.Mobile.Client.Activities
{
    [Activity(Label = "CmtRideLinqChangePaymentActivity", Theme = "@style/MainTheme", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class CmtRideLinqChangePaymentActivity : BaseBindingActivity<CmtRideLinqChangePaymentViewModel>
    {
		protected override void OnViewModelSet()
		{
			base.OnViewModelSet ();

            SetContentView(Resource.Layout.View_Payments_CmtRideLinqChangePayment);
        }
    }
}