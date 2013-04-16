
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
using apcurium.MK.Booking.Mobile.Client.Controls;

namespace apcurium.MK.Booking.Mobile.Client
{
	[Activity(Label = "BookPaymentActivity", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
	public class BookPaymentActivity : BaseBindingActivity<PaymentViewModel>
	{
		protected override int ViewTitleResourceId
		{
			get { return Resource.String.View_PaymentCreditCardsOnFile; }
		}

		TextView _tipAmountTextView;
		EditText _meterAmountTextView;
		TextView _totalAmountTextView;
		TipSlider _tipSlider;

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
			_tipSlider = FindViewById<TipSlider>(Resource.Id.tipSlider);
			_tipAmountTextView = FindViewById<TextView>(Resource.Id.tipAmountTextView);
			_meterAmountTextView = FindViewById<EditText>(Resource.Id.meterAmountTextView);
			_totalAmountTextView =  FindViewById<TextView>(Resource.Id.totalAmountTextView);

			UpdateAmounts();



			_meterAmountTextView.TextChanged += (object sender, Android.Text.TextChangedEventArgs e) =>  {
				UpdateAmounts();
			};


			_meterAmountTextView.EditorAction+= (sender, e) => {
				e.Handled = false;
				MeterAmount = MeterAmount;//format				
			};

			_tipSlider.PercentChanged += (object sender, EventArgs e) => 
			{
				UpdateAmounts();
			};

		}

		public void UpdateAmounts()
		{
			TipAmount = MeterAmount * ((((double)_tipSlider.Percent)/100.00));			
			TotalAmount = MeterAmount + TipAmount;
		}

		protected override void OnViewModelSet()
		{            
			SetContentView(Resource.Layout.View_Payments_BookPayment);
			ViewModel.Load();		
		}
	}
}

