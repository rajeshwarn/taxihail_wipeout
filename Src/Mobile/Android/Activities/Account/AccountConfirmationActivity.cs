
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
using Android.Content.PM;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Account
{
	[Activity(Label = "Account Confirmation", Theme = "@style/LoginTheme", ScreenOrientation = ScreenOrientation.Portrait)]
	public class AccountConfirmationActivity : BaseBindingActivity<AccountConfirmationViewModel>
	{
		protected override void OnViewModelSet()
		{
			base.OnViewModelSet ();
			SetContentView (Resource.Layout.View_AccountConfirmation);
		}
	}
}

