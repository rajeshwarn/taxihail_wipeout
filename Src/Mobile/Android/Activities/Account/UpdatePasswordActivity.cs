
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
	[Activity (Label = "UpdatePasswordActivity", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]	
	public class UpdatePasswordActivity :  BaseBindingActivity<UpdatePasswordViewModel> 
	{
		protected override int ViewTitleResourceId
		{
			get { return Resource.String.View_UpdatePassword; }
		}
		
		protected override void OnViewModelSet()
		{
			SetContentView(Resource.Layout.View_UpdatePassword);
		}
	}
}

