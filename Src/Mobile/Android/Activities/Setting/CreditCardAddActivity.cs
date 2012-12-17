
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
using apcurium.MK.Booking.Mobile.Client.Activities;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client
{
	[Activity(Label = "CreditCardAddActivity", Theme = "@android:style/Theme.NoTitleBar", WindowSoftInputMode = SoftInput.AdjustPan, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
	public class CreditCardAddActivity : BaseBindingActivity<CreditCardAddViewModel>
	{
		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			
			// Create your application here
		}

		protected override void OnViewModelSet()
		{
			ViewModel.CreditCardCompanies[0].Image = Resource.Drawable.visa.ToString();
			ViewModel.CreditCardCompanies[1].Image = Resource.Drawable.mastercard.ToString();
			ViewModel.CreditCardCompanies[2].Image = Resource.Drawable.amex.ToString();
			SetContentView(Resource.Layout.View_CreditCardAdd);
		}
		
		protected override int ViewTitleResourceId
		{
			get { return Resource.String.CreditCardsAddTitle; }
		}
	}
}

