using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Views;
using apcurium.MK.Common.Entity;
using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Binding;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.ViewModels.Payment.Cmt;
using apcurium.MK.Booking.Mobile.Client.Localization;
using Cirrious.MvvmCross.Binding.BindingContext;

namespace apcurium.MK.Booking.Mobile.Client.Views.Payments
{
	public partial class CmtRideLinqChangePaymentView : BaseViewController<CmtRideLinqChangePaymentViewModel>
    {              
		public CmtRideLinqChangePaymentView() : base()
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

			var cancelbutton = new MonoTouch.UIKit.UIBarButtonItem(Localize.GetValue("CancelBoutton"), UIBarButtonItemStyle.Plain, delegate {
				ViewModel.CancelCommand.Execute();
			});

			var savebutton = new MonoTouch.UIKit.UIBarButtonItem(Localize.GetValue("DoneBoutton"), UIBarButtonItemStyle.Plain, delegate {
				ViewModel.SaveCommand.Execute();
            });

            NavigationItem.HidesBackButton = true;
			NavigationItem.LeftBarButtonItem = cancelbutton;
			NavigationItem.RightBarButtonItem = savebutton;
			NavigationItem.Title = Localize.GetValue("View_Payments_CmtRideLinqChangePaymentTitle");

			lblCreditCard.Text = Localize.GetValue("PaymentDetails.CreditCardLabel");

			var set = this.CreateBindingSet<CmtRideLinqChangePaymentView, CmtRideLinqChangePaymentViewModel>();

			set.Bind(btnCreditCard)
				.For("Text")
				.To(vm => vm.PaymentPreferences.SelectedCreditCard.FriendlyName);
			set.Bind(btnCreditCard)
				.For(v => v.Last4Digits)
				.To(vm => vm.PaymentPreferences.SelectedCreditCard.Last4Digits);
			set.Bind(btnCreditCard)
				.For("CreditCardCompany")
				.To(vm => vm.PaymentPreferences.SelectedCreditCard.CreditCardCompany);
			set.Bind(btnCreditCard)
				.For(v => v.NavigateCommand)
				.To(vm => vm.PaymentPreferences.NavigateToCreditCardsList);

			set.Bind(TipSlider)
				.For("Value")
				.To(vm => vm.PaymentPreferences.Tip);

			set.Apply();    
        }
    }
}

