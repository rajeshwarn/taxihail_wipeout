using MonoTouch.UIKit;
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
		
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            NavigationController.NavigationBar.Hidden = false;
            NavigationItem.HidesBackButton = true;

            ChangeThemeOfBarStyle();
            ChangeRightBarButtonFontToBold();
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            View.BackgroundColor = UIColor.FromRGB(242, 242, 242); 

            lblTipAmount.Text = Localize.GetValue("PaymentDetails.TipAmountLabel");

            NavigationItem.LeftBarButtonItem = new UIBarButtonItem(Localize.GetValue("Cancel"), UIBarButtonItemStyle.Plain, null);
            NavigationItem.RightBarButtonItem = new UIBarButtonItem(Localize.GetValue("Done"), UIBarButtonItemStyle.Plain, null);

            txtTip.Configure(Localize.GetValue("PaymentDetails.TipAmountLabel"), () => ViewModel.PaymentPreferences.Tips, () => ViewModel.PaymentPreferences.Tip, x => ViewModel.PaymentPreferences.Tip = (int)x.Id);

			var set = this.CreateBindingSet<CmtRideLinqChangePaymentView, CmtRideLinqChangePaymentViewModel>();

            set.Bind(NavigationItem.LeftBarButtonItem)
                .For("Clicked")
                .To(vm => vm.CancelCommand);

            set.Bind(NavigationItem.RightBarButtonItem)
                .For("Clicked")
                .To(vm => vm.SaveCommand);

            set.Bind(txtTip)
                .For(v => v.Text)
                .To(vm => vm.PaymentPreferences.TipAmount);

			set.Apply();    
        }
    }
}

