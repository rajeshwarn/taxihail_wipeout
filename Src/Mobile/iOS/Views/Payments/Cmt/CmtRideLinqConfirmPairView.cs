using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Views;
using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Binding;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.ViewModels.Payment.Cmt;
using apcurium.MK.Booking.Mobile.Client.Localization;
using Cirrious.MvvmCross.Binding.BindingContext;

namespace apcurium.MK.Booking.Mobile.Client.Views.Payments
{
	public partial class CmtRideLinqConfirmPairView : BaseViewController<CmtRideLinqConfirmPairViewModel>
	{
		public CmtRideLinqConfirmPairView() : base()
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			NavigationController.NavigationBar.Hidden = false;
			View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));

			NavigationItem.HidesBackButton = true;
			NavigationItem.Title = Localize.GetValue("CmtConfirmBookingInfo");

			lblCarNumber.Text = Localize.GetValue("CmtCarNumber");
			lblCardNumber.Text = Localize.GetValue("CmtCardNumber");
			lblTip.Text = Localize.GetValue("CmtTipAmount");

			AppButtons.FormatStandardButton((GradientButton)btnConfirm, Localize.GetValue("CmtConfirmPayment"), AppStyle.ButtonColor.Green );
			AppButtons.FormatStandardButton((GradientButton)btnChangePaymentSettings, Localize.GetValue("CmtChangePaymentInfo"), AppStyle.ButtonColor.Silver );
			AppButtons.FormatStandardButton((GradientButton)btnCancel, Localize.GetValue("CmtCancelPayment"), AppStyle.ButtonColor.Red );

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

			View.ApplyAppFont ();
		}
	}
}