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
using System.Text.RegularExpressions;
using apcurium.MK.Booking.Mobile.ViewModels;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Client.Validation;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Account
{
    [Activity(Label = "Password Recovery", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation=Android.Content.PM.ScreenOrientation.Portrait)]
	public class PasswordRecoveryActivity : BaseBindingActivity<ResetPasswordViewModel> 
    {
        protected override int ViewTitleResourceId
        {
            get { return Resource.String.View_PasswordRecovery_Label; }
        }

        protected override void OnViewModelSet()
		{
			SetContentView(Resource.Layout.View_PasswordRecovery);
		}
    }
}