using Cirrious.MvvmCross.Binding.BindingContext;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ViewModels.Payment.Cmt;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using apcurium.MK.Booking.Mobile.Client.Localization;

namespace apcurium.MK.Booking.Mobile.Client.Views.Payments
{
	public partial class CmtRideLinqConfirmPairView : BaseViewController<CmtRideLinqConfirmPairViewModel>
	{
		public CmtRideLinqConfirmPairView() : base()
		{
		}

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            NavigationController.NavigationBar.Hidden = false;
            NavigationItem.HidesBackButton = true;
            NavigationItem.Title = Localize.GetValue("CmtRideLinqConfirmPairView");

            ChangeThemeOfBarStyle();
        }

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
            
            View.BackgroundColor = UIColor.FromRGB(242, 242, 242);

            lblConfirmPairDetail.Text = Localize.GetValue("CmtRideLinqConfirmPairViewConfirmPairDetailText");
            lblCarNumber.Text = Localize.GetValue("CmtCarNumber");
            lblCardNumber.Text = Localize.GetValue("CmtCardNumber");
            lblTip.Text = Localize.GetValue("CmtTipAmount");
            btnConfirm.SetTitle(Localize.GetValue("CmtConfirmPayment"), UIControlState.Normal);
			btnCancel.SetTitle(Localize.GetValue("PayInCar"), UIControlState.Normal);

            FlatButtonStyle.Green.ApplyTo(btnConfirm);
			FlatButtonStyle.Silver.ApplyTo(btnCancel);

			var set = this.CreateBindingSet<CmtRideLinqConfirmPairView, CmtRideLinqConfirmPairViewModel>();

			set.Bind(btnConfirm)
				.For("TouchUpInside")
				.To(vm => vm.ConfirmPayment);

			set.Bind(btnCancel)
				.For("TouchUpInside")
				.To(vm => vm.CancelPayment);

			set.Bind(lblCarNumberValue)
				.For(v => v.Text)
				.To(vm => vm.CarNumber);

			set.Bind(lblCardNumberValue)
				.For(v => v.Text)
				.To(vm => vm.CardNumber);

			set.Bind(lblTipValue)
				.For(v => v.Text)
				.To(vm => vm.TipAmountInPercent);

			set.Apply ();
		}
	}
}