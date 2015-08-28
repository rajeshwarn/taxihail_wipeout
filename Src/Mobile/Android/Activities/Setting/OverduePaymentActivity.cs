
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
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using Android.Content.PM;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Setting
{
	[Activity(Label = "@string/OverduePaymentActivityName",
		Theme = "@style/MainTheme",
		ScreenOrientation = ScreenOrientation.Portrait)]			
	public class OverduePaymentActivity : BaseBindingActivity<OverduePaymentViewModel>
	{
		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			SetContentView(Resource.Layout.View_OverduePayment);
		}
	}
}

