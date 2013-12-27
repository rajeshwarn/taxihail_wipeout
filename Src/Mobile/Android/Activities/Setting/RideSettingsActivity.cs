using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Client.ListViewStructure;
using apcurium.MK.Booking.Mobile.Client.ListViewCell;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Client.Controls;
using Android.Text.Method;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Setting
{
    [Activity(Label = "RideSettingsActivity", Theme = "@android:style/Theme.NoTitleBar", WindowSoftInputMode = SoftInput.AdjustPan, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
	public class RideSettingsActivity : BaseBindingActivity<RideSettingsViewModel>
    {
        protected override int ViewTitleResourceId
        {
            get { return Resource.String.View_RideSettings; }
        }

		protected override void OnViewModelSet ()
		{
			SetContentView (Resource.Layout.View_RideSettings);
			var txtPassword = this.FindViewById<EditTextNavigate> (Resource.Id.rideSettingsPassword);
			txtPassword.TransformationMethod = PasswordTransformationMethod.Instance;
			txtPassword.Text = "my secure password"; /* fake password for display only */

			if (!ViewModel.ShouldDisplayCreditCards) {
				FindViewById<TextView>(Resource.Id.lblCC).Visibility = ViewStates.Gone;          
				FindViewById<CreditCardButton>(Resource.Id.btCC).Visibility = ViewStates.Gone;
			}

			if (!ViewModel.ShouldDisplayTipSlider) {
				FindViewById<TextView>(Resource.Id.tipAmountLabel).Visibility = ViewStates.Gone;     
				FindViewById<TipSlider>(Resource.Id.tipSlider).Visibility = ViewStates.Gone;
			}


		}
    }
}

