
using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Views;
using System.Collections.Generic;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public partial class BookPaymentSettingsView : BaseViewController<BookPaymentSettingsViewModel>
    {
        #region Constructors
        
        public BookPaymentSettingsView () 
            : base(new MvxShowViewModelRequest<BookPaymentSettingsViewModel>( null, true, new Cirrious.MvvmCross.Interfaces.ViewModels.MvxRequestedBy()   ) )
        {
        }
        
        public BookPaymentSettingsView(MvxShowViewModelRequest request) 
            : base(request)
        {
        }
        
        public BookPaymentSettingsView(MvxShowViewModelRequest request, string nibName, NSBundle bundle) 
            : base(request, nibName, bundle)
        {
        }
        
        #endregion
		
        	
        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();
            View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));
            NavigationItem.HidesBackButton = false;
            NavigationItem.Title = Resources.GetValue("ChargeTypeCreditCardFile");

            AppButtons.FormatStandardButton((GradientButton)btCancel, Resources.CancelBoutton, AppStyle.ButtonColor.Red );
            AppButtons.FormatStandardButton((GradientButton)btConfirm, Resources.ConfirmButton, AppStyle.ButtonColor.Green );  

            lblCreditCardOnFile.Text = Resources.GetValue("PaymentDetails.CreditCardLabel");
            lblTipAmount.Text = Resources.GetValue("PaymentDetails.TipAmountLabel");
            lblOptional.Text= Resources.GetValue("PaymentDetails.Optional");

            base.DismissKeyboardOnReturn(txtTipAmount);

            segmentTip.SelectedSegment = ViewModel.PaymentPreferences.IsTipInPercent ? 0 : 1;
            segmentTip.ValueChanged += HandleValueChanged;

            this.AddBindings(new Dictionary<object, string>() {
                { btCancel, "{'TouchUpInside':{'Path':'CancelOrderCommand'}}"},                
                { btConfirm, "{'TouchUpInside':{'Path':'ConfirmOrderCommand'}}"},          
                { txtTipAmount, "{'Text': {'Path': 'PaymentPreferences.Tip'}}" },
                { btCreditCard, "{'Text': {'Path': 'PaymentPreferences.SelectedCreditCard.FriendlyName'}, 'Last4Digits': {'Path': 'PaymentPreferences.SelectedCreditCard.Last4Digits'}, 'CreditCardCompany': {'Path': 'PaymentPreferences.SelectedCreditCard.CreditCardCompany'}, 'NavigateCommand': {'Path': 'PaymentPreferences.NavigateToCreditCardsList'}}" }
            });
			

            View.BringSubviewToFront( bottomBar );    
            this.View.ApplyAppFont ();
        }

        void HandleValueChanged (object sender, EventArgs e)
        {
            ViewModel.PaymentPreferences.IsTipInPercent = (segmentTip.SelectedSegment == 0);
        }
		
    }
}

