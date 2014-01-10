using System.Collections.Generic;
using System.Drawing;
using apcurium.MK.Booking.Mobile.Client.Binding;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;
using Cirrious.MvvmCross.Views;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Views.Payments
{
    public partial class BookPaymentSettingsView : BaseViewController<PaymentViewModel>
    {
        public BookPaymentSettingsView(MvxShowViewModelRequest request) 
            : base(request)
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

            this.AddBindings(new Dictionary<object, string> {         
                { btConfirm, "{'TouchUpInside':{'Path':'ConfirmOrderCommand'}}"},   
                { TipSlider, new B("Value","PaymentPreferences.Tip",B.Mode.TwoWay) },
                //{ TotalAmountLabel, new B("Text","Amount")},//See above
                { MeterAmountLabel, new B("Placeholder", "PlaceholderAmount")},
                { TipAmountLabel, new B("Placeholder", "PlaceholderAmount") },
				{ payPalToggle, 
					new B("PayPalSelected", "PayPalSelected", B.Mode.TwoWay)
						.Add("Hidden", "PaymentSelectorToggleIsVisible", "BoolInverter")
				},
                { btCreditCard, 
                    new B("Text","PaymentPreferences.SelectedCreditCard.FriendlyName")
                        .Add("Last4Digits","PaymentPreferences.SelectedCreditCard.Last4Digits")
                        .Add("CreditCardCompany","PaymentPreferences.SelectedCreditCard.CreditCardCompany")
                        .Add("NavigateCommand","PaymentPreferences.NavigateToCreditCardsList")
				}
            });
			   
            View.ApplyAppFont ();
        }

		public override void ViewWillAppear (bool animated)
		{
			NavigationController.NavigationBarHidden = false;
			base.ViewWillAppear (animated);
		}

    }
}

