
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
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.Client
{
	[Activity(Label = "ConfirmCarNumberActivity", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
	public class ConfirmCarNumberActivity : BaseBindingActivity<ConfirmCarNumberViewModel>
	{
		protected override int ViewTitleResourceId
		{
			get { return Resource.String.View_PaymentCreditCardsOnFile; }
		}
		
		
		
		protected override void OnStart ()
		{
			base.OnStart ();
			
		}
		
		protected override void OnViewModelSet()
		{            
			SetContentView(Resource.Layout.View_Payments_ConfirmCarNumber);
			ViewModel.Load();		
		}
	}
}

