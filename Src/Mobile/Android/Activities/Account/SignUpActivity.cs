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
using TinyIoC;

using apcurium.Framework.Extensions;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Client.Validation;
using apcurium.MK.Booking.Api.Contract.Requests;
using System.Threading;
using Cirrious.MvvmCross.Views;
using Cirrious.MvvmCross.Interfaces.Views;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using TinyMessenger;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Account
{
    [Activity(Label = "Sign Up", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
	public class SignUpActivity : BaseBindingActivity<CreateAcccountViewModel> 
    {
        protected override int ViewTitleResourceId
        {
            get { return Resource.String.View_SignUp; }
        }

		protected override void OnViewModelSet()
		{
			SetContentView(Resource.Layout.View_SignUp);
		}    
	}
}
