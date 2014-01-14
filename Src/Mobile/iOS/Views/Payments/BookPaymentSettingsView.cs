using System.Collections.Generic;
using System.Drawing;
using apcurium.MK.Booking.Mobile.Client.Binding;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using Cirrious.MvvmCross.Views;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Binding.BindingContext;

namespace apcurium.MK.Booking.Mobile.Client.Views.Payments
{
	[MvxViewFor(typeof(PaymentViewModel))]
	public partial class BookPaymentSettingsView : BaseViewController<PaymentViewModel>
    {
        public BookPaymentSettingsView() 
			: base("BookPaymentSettingsView", null)
        {
        }
		

        double MeterAmount{
            get{
                return CultureProvider.ParseCurrency(MeterAmountLabel.Text);
            }
            set{
                MeterAmountLabel.Text = CultureProvider.FormatCurrency(value);
                ViewModel.MeterAmount = CultureProvider.FormatCurrency(value);//Todo ugly binding done in code behind
            }
        }

        double TipAmount{
            get{
                return CultureProvider.ParseCurrency(TipAmountLabel.Text);
            }
            set{
                TipAmountLabel.Text = CultureProvider.FormatCurrency(value);
                ViewModel.TipAmount = CultureProvider.FormatCurrency(value);//Todo ugly binding done in code behind
            }
        }

        double TotalAmount{
// ReSharper disable once UnusedMember.Local
            get{
                return CultureProvider.ParseCurrency(TotalAmountLabel.Text);
            }
            set{
                TotalAmountLabel.Text = CultureProvider.FormatCurrency(value);
                ViewModel.TextAmount = TotalAmountLabel.Text;//Todo ugly binding done in code behind
            }
        }

        public void UpdateAmounts(bool hideKeyboard=true)
        {
            if(hideKeyboard)
            {
                View.ResignFirstResponderOnSubviews();
            }
            TotalAmount = TipAmount+ MeterAmount;
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

			if (!ViewModel.PaymentSelectorToggleIsVisible) {
				ScrollViewer.Frame = new RectangleF (ScrollViewer.Frame.X, ScrollViewer.Frame.Y - payPalToggle.Frame.Height, ScrollViewer.Frame.Width, ScrollViewer.Frame.Height);
			}

			ScrollViewer.ContentSize = new SizeF(ScrollViewer.ContentSize.Width, btConfirm.Frame.Bottom + 50);

			payPalLogo.Image = UIImage.FromFile("Assets/CreditCard/paypal.png");

            Container.BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));
            NavigationItem.HidesBackButton = false;
			NavigationItem.Title = Localize.GetValue("View_PaymentCreditCardsOnFile");

			AppButtons.FormatStandardButton(btConfirm, Localize.GetValue("StatusPayButton"), AppStyle.ButtonColor.Green );  

            TotalAmountLabel.TextColor = AppStyle.DarkText;
            TotalAmountLabel.Font = AppStyle.GetBoldFont (TotalAmountLabel.Font.PointSize);

            lblCreditCardOnFile.Text = Localize.GetValue("PaymentDetails.CreditCardLabel");

            ClearKeyboardButton.TouchDown+= (sender, e) => {
                UpdateAmounts();
                MeterAmount = MeterAmount; //Format
                TipAmount = TipAmount;               //Format
            };

            TipSlider.ValueChanged+= (sender, e) => {   
                TipAmount = (MeterAmount * (TipSlider.Value/100));
                UpdateAmounts();
            };

            MeterAmountLabel.EditingDidBegin += (sender, e) => {
                TipAmount = (MeterAmount * (TipSlider.Value/100));                             
                UpdateAmounts(false);
                
                TipSlider.Enabled = true;
            };

            MeterAmountLabel.EditingChanged+= (sender, e) => {
                TipAmount = (MeterAmount * (TipSlider.Value/100));                             
                UpdateAmounts(false);
                
                TipSlider.Enabled = true;
            };

            TipAmountLabel.ClearsOnBeginEditing = true;

            TipAmountLabel.EditingChanged+= (sender, e) => {
                UpdateAmounts(false);
                TipSlider.Enabled = false;
            };

			payPalToggle.ValueChanged+= (sender, e) => {   
				btCreditCard.Hidden = ((PaymentSelector)sender).PayPalSelected;
				lblCreditCardOnFile.Hidden = ((PaymentSelector)sender).PayPalSelected;
				payPalLogo.Hidden = !((PaymentSelector)sender).PayPalSelected;
			};
		
			btConfirm.TouchDown += (sender, e) =>
			{
				if ( MeterAmountLabel.IsFirstResponder )
				{
					MeterAmountLabel.ResignFirstResponder();
				}
				if ( TipAmountLabel.IsFirstResponder )
				{
					TipAmountLabel.ResignFirstResponder();
				}
				ViewModel.MeterAmount = MeterAmountLabel.Text;//Todo ugly binding done in code behind
				ViewModel.TipAmount = TipAmountLabel.Text;//Todo ugly binding done in code behind
			};

			var set = this.CreateBindingSet<BookPaymentSettingsView, PaymentViewModel>();
			set.Bind(btConfirm)
				.For("TouchUpInside")
				.To(vm => vm.ConfirmOrderCommand);

			set.Bind(TipSlider)
				.For(v => v.Value)
				.To(vm => vm.PaymentPreferences.Tip);

			set.Bind(MeterAmountLabel)
				.For(v => v.Placeholder)
				.To(vm => vm.PlaceholderAmount);

			set.Bind(TipAmountLabel)
				.For(v => v.Placeholder)
				.To(vm => vm.PlaceholderAmount);

			set.Bind(payPalToggle)
				.For(v => v.PayPalSelected)
				.To(vm => vm.PayPalSelected);
			set.Bind(payPalToggle)
				.For(v => v.Hidden)
				.To(vm => vm.PaymentSelectorToggleIsVisible)
				.WithConversion("BoolInverter");

			set.Bind(btCreditCard)
				.For("Text")
				.To(vm => vm.PaymentPreferences.SelectedCreditCard.FriendlyName);
			set.Bind(btCreditCard)
				.For(v => v.Last4Digits)
				.To(vm => vm.PaymentPreferences.SelectedCreditCard.Last4Digits);
			set.Bind(btCreditCard)
				.For("CreditCardCompany")
				.To(vm => vm.PaymentPreferences.SelectedCreditCard.CreditCardCompany);
			set.Bind(btCreditCard)
				.For(v => v.NavigateCommand)
				.To(vm => vm.PaymentPreferences.NavigateToCreditCardsList);

			set.Apply ();   

            View.ApplyAppFont ();
        }

		public override void ViewWillAppear (bool animated)
		{
			NavigationController.NavigationBarHidden = false;
			base.ViewWillAppear (animated);
		}

    }
}

