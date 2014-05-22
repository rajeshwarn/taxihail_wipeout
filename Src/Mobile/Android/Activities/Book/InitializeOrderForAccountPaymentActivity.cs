using System;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using Android.App;
using Android.Content.PM;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
	[Activity(Label = "InitializeOrderForAccountPaymentActivity", Theme = "@style/MainTheme", ScreenOrientation = ScreenOrientation.Portrait)]
	public class InitializeOrderForAccountPaymentActivity : BaseBindingActivity<InitializeOrderForAccountPaymentViewModel>
	{
		protected override void OnViewModelSet()
		{
			base.OnViewModelSet ();
			SetContentView(Resource.Layout.View_InitializeOrderForAccountPayment);
		}
	}
}

