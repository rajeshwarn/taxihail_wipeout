
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
using apcurium.MK.Booking.Mobile.Client.Controls;
using Android.Views.InputMethods;
using Extensions;

namespace apcurium.MK.Booking.Mobile.Client
{
	[Activity(Label = "BookPaymentActivity", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
	public class BookPaymentActivity : BaseBindingActivity<PaymentViewModel>
	{
		protected override int ViewTitleResourceId
		{
			get { return Resource.String.View_PaymentCreditCardsOnFile; }
		}

		EditText _tipAmountTextView;
		EditText _meterAmountTextView;
		TextView _totalAmountTextView;
		TipSlider _tipSlider;

		LinearLayout _layoutRoot;

		public double TipAmount {
			get{
				return CultureProvider.ParseCurrency(_tipAmountTextView.Text);
			}
			set{

				_tipAmountTextView.Text = CultureProvider.FormatCurrency(value);
			}
		}

		public double MeterAmount {
			get{
				return CultureProvider.ParseCurrency(_meterAmountTextView.Text);
			}
			set{
				_meterAmountTextView.Text = CultureProvider.FormatCurrency (value);
			}
		}

		public double TotalAmount {
			get{
						return CultureProvider.ParseCurrency(_totalAmountTextView.Text);
			}
			set{
				_totalAmountTextView.Text = CultureProvider.FormatCurrency (value);
			}
		}

		protected override void OnStart ()
		{
			base.OnStart ();
			_tipSlider = FindViewById<TipSlider>(Resource.Id.tipSlider);
			_tipAmountTextView = FindViewById<EditText>(Resource.Id.tipAmountTextView);
			_meterAmountTextView = FindViewById<EditText>(Resource.Id.meterAmountTextView);
			_totalAmountTextView =  FindViewById<TextView>(Resource.Id.totalAmountTextView);
			_layoutRoot = FindViewById<LinearLayout> (Resource.Id.layoutRoot);

			UpdateAmounts();

			_tipAmountTextView.TextChanged += (sender, e) => {
				UpdateAmounts();
			};
			_meterAmountTextView.FocusChange+= (sender, e) => {
				if(!e.HasFocus)
				{
					MeterAmount = MeterAmount;
				}
			};

			_meterAmountTextView.TextChanged += (object sender, Android.Text.TextChangedEventArgs e) =>  {
				TipAmount = MeterAmount * ((((double)_tipSlider.Percent)/100.00));
				UpdateAmounts();
			};

			_meterAmountTextView.FocusChange += (sender, e) => {
				if(!e.HasFocus)
				{
					TipAmount = MeterAmount * ((((double)_tipSlider.Percent)/100.00));
					UpdateAmounts();
				}
			};

			_tipAmountTextView.EditorAction+= (sender, e) => {
				e.Handled = false;
				TipAmount = TipAmount;//format	
			};
			_tipAmountTextView.FocusChange+= (sender, e) => {
				if(!e.HasFocus)
				{
					TipAmount = TipAmount;//format	
				}
			};

			_meterAmountTextView.EditorAction+= (sender, e) => {
				e.Handled = false;
				MeterAmount = MeterAmount;//format				
			};

			_tipSlider.PercentChanged += (object sender, EventArgs e) => 
			{
				TipAmount = MeterAmount * ((((double)_tipSlider.Percent)/100.00));	

				_tipAmountTextView.HideKeyboard(this);

				_layoutRoot.RequestFocus();


				UpdateAmounts();
			};

		}

		public void UpdateAmounts()
		{	
			TotalAmount = MeterAmount + TipAmount;
		}

		protected override void OnViewModelSet()
		{            
			SetContentView(Resource.Layout.View_Payments_BookPayment);
			ViewModel.Load();		
		}
	}
}

