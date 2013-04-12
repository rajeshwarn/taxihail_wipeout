
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
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.Client
{
	[Activity(Label = "BookPaymentSettingsActivity", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
	public class BookPaymentSettingsActivity : BaseBindingActivity<BookPaymentSettingsViewModel>
	{
		protected override int ViewTitleResourceId
		{
			get { return Resource.String.View_PaymentCreditCardsOnFile; }
		}

		TextView _tipAmountTextView;
		TextView _meterAmountTextView;
		TextView _totalAmountTextView;
		SeekBar _seekBar;

		public double TipAmount {
			get{
				return _tipAmountTextView.Text.FromDollars();
			}
			set{
				_tipAmountTextView.Text = value.ToDollars();
			}
		}

		public double MeterAmount {
			get{
				return _meterAmountTextView.Text.FromDollars();
			}
			set{
				_meterAmountTextView.Text = value.ToDollars();
			}
		}

		public double TotalAmount {
			get{
				return _totalAmountTextView.Text.FromDollars();
			}
			set{
				_totalAmountTextView.Text = value.ToDollars();
			}
		}

		protected override void OnStart ()
		{
			base.OnStart ();
			_seekBar= FindViewById<SeekBar>(Resource.Id.seekBar);
			_tipAmountTextView = FindViewById<TextView>(Resource.Id.tipAmountTextView);
			_meterAmountTextView = FindViewById<EditText>(Resource.Id.meterAmountTextView);
			_totalAmountTextView =  FindViewById<TextView>(Resource.Id.totalAmountTextView);

			UpdateAmounts();

			_meterAmountTextView.TextChanged += (sender, e) => {
				UpdateAmounts();
			};

			_seekBar.ProgressChanged += (object sender, SeekBar.ProgressChangedEventArgs e) => {

				_seekBar.Progress = (int)(Math.Round(e.Progress/5.0)*5);

				UpdateAmounts();
			};

		}

		public void UpdateAmounts()
		{
			TipAmount = MeterAmount * ((((double)_seekBar.Progress)/100.00));			
			TotalAmount = MeterAmount + TipAmount;
		}

		protected override void OnViewModelSet()
		{            
			SetContentView(Resource.Layout.View_BookPaymentSettings);
			ViewModel.Load();		
		}
	}
}

