
using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Views;
using System.Collections.Generic;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Binding;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Mobile.Client.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Views
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
			NavigationItem.Title = Resources.GetValue("View_PaymentCreditCardsOnFile");

			AppButtons.FormatStandardButton((GradientButton)btConfirm, Resources.GetValue("StatusPayButton"), AppStyle.ButtonColor.Green );  

            TotalAmountLabel.TextColor = AppStyle.DarkText;
            TotalAmountLabel.Font = AppStyle.GetBoldFont (TotalAmountLabel.Font.PointSize);

            lblCreditCardOnFile.Text = Resources.GetValue("PaymentDetails.CreditCardLabel");

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
		
            this.AddBindings(new Dictionary<object, string>() {         
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
			   
            this.View.ApplyAppFont ();
        }

		public override void ViewWillAppear (bool animated)
		{
			this.NavigationController.NavigationBarHidden = false;
			base.ViewWillAppear (animated);
		}

    }
}

