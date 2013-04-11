
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

		protected override void OnStart ()
		{
			base.OnStart ();
			var x= FindViewById<SeekBar>(Resource.Id.seek);
			//SeekBar.IOnSeekBarChangeListener
			x.ProgressChanged += (object sender, SeekBar.ProgressChangedEventArgs e) => {
				x.Progress = (int)(Math.Round(e.Progress/5.0)*5);
			};
			var u =0;
		}

		protected override void OnViewModelSet()
		{            
			SetContentView(Resource.Layout.View_BookPaymentSettings);
			ViewModel.Load();

		
		}
	}
}

