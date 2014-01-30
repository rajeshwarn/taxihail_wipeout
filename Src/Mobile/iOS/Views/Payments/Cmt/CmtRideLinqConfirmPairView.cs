using System;
using System.Collections.Generic;
using System.Drawing;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Views;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.ViewModels.Payment.Cmt;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Mobile.Client.Binding;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using apcurium.MK.Booking.Mobile.Client.Extensions;
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
            btnChangePaymentSettings.SetTitle(Localize.GetValue("CmtChangePaymentInfo"), UIControlState.Normal);
            btnCancel.SetTitle(Localize.GetValue("CmtCancelPayment"), UIControlState.Normal);

            FlatButtonStyle.Green.ApplyTo(btnConfirm);
            FlatButtonStyle.Silver.ApplyTo(btnChangePaymentSettings);
            FlatButtonStyle.Red.ApplyTo(btnCancel);

			var set = this.CreateBindingSet<CmtRideLinqConfirmPairView, CmtRideLinqConfirmPairViewModel>();

			set.Bind(btnConfirm)
				.For("TouchUpInside")
				.To(vm => vm.ConfirmPayment);

			set.Bind(btnChangePaymentSettings)
				.For("TouchUpInside")
				.To(vm => vm.ChangePaymentInfo);

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