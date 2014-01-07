using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Views;
using Cirrious.MvvmCross.Dialog.Touch.Dialog;
using apcurium.MK.Common.Entity;
using Cirrious.MvvmCross.Dialog.Touch.Dialog.Elements;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;
using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Binding;
using apcurium.MK.Booking.Mobile.Client.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Views.Payments
{
	public partial class CmtRideLinqChangePaymentView : BaseViewController<CmtRideLinqChangePaymentViewModel>
    {              
		public CmtRideLinqChangePaymentView(MvxShowViewModelRequest request) 
            : base(request)
        {
        }    
		
        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            NavigationController.NavigationBar.Hidden = false;
            Container.BackgroundColor =  UIColor.Clear;
			scrollView.BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));

            View.BackgroundColor = UIColor.Clear; 

            scrollView.AutoSize ();

			var cancelbutton = new MonoTouch.UIKit.UIBarButtonItem(Resources.CancelBoutton, UIBarButtonItemStyle.Plain, delegate {
				ViewModel.CancelCommand.Execute();
			});

			var savebutton = new MonoTouch.UIKit.UIBarButtonItem(Resources.DoneButton, UIBarButtonItemStyle.Plain, delegate {
				ViewModel.SaveCommand.Execute();
            });

            NavigationItem.HidesBackButton = true;
			NavigationItem.LeftBarButtonItem = cancelbutton;
			NavigationItem.RightBarButtonItem = savebutton;
			NavigationItem.Title = Resources.GetValue("View_Payments_CmtRideLinqChangePaymentTitle");

            lblCreditCard.Text = Resources.GetValue("PaymentDetails.CreditCardLabel");
            this.AddBindings(new Dictionary<object, string>(){
                { btnCreditCard, 
                    new B("Text","PaymentPreferences.SelectedCreditCard.FriendlyName")
                    .Add("Last4Digits","PaymentPreferences.SelectedCreditCard.Last4Digits")
                    .Add("CreditCardCompany","PaymentPreferences.SelectedCreditCard.CreditCardCompany")
                    .Add("NavigateCommand","PaymentPreferences.NavigateToCreditCardsList") },
                { TipSlider, new B("Value","PaymentPreferences.Tip",B.Mode.TwoWay) }
            });         
        }
    }
}

