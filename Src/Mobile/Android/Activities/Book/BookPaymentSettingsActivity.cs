
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

namespace apcurium.MK.Booking.Mobile.Client
{
	[Activity(Label = "BookPaymentSettingsActivity", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
	public class BookPaymentSettingsActivity : BaseBindingActivity<BookPaymentSettingsViewModel>
	{
		protected override int ViewTitleResourceId
		{
			get { return Resource.String.View_PaymentCreditCardsOnFile; }
		}

		protected override void OnViewModelSet()
		{            
			SetContentView(Resource.Layout.View_BookPaymentSettings);
			ViewModel.OnViewLoaded();
		}
	}
}

