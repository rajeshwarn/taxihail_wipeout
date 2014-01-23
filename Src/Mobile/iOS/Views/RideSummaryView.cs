using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Localization;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Views;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Binding;
using System.Collections.Generic;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using System;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Client.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Views
{

	public partial class RideSummaryView  : BaseViewController<RideSummaryViewModel>
	{     
		public RideSummaryView() 
			: base("RideSummaryView", null)
		{
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			NavigationItem.HidesBackButton = false;
			NavigationItem.Title = Localize.GetValue("View_RideSummary");
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			View.BackgroundColor = UIColor.FromRGB(239, 239, 239);

			FlatButtonStyle.Green.ApplyTo(btnRateRide);
			FlatButtonStyle.Green.ApplyTo(btnSendReceipt);
			FlatButtonStyle.Green.ApplyTo(btnReSendConfirmation);
			FlatButtonStyle.Green.ApplyTo(btnPay);

			lblTitle.Text = Localize.GetValue ("RideSummaryTitleText");
			lblSubTitle.Text = String.Format(Localize.GetValue ("RideSummarySubTitleText"), this.Services().Settings.ApplicationName);

			if (ViewModel.IsSendReceiptButtonShown)
				btnSendReceipt.RemoveFromSuperview();
			else
				btnSendReceipt.SetTitle(Localize.GetValue("SendReceipt"), UIControlState.Normal);

			if (ViewModel.IsRatingButtonShown)
				btnRateRide.RemoveFromSuperview();
			else
				btnRateRide.SetTitle(Localize.GetValue("RateRide"), UIControlState.Normal);

			if (ViewModel.IsPayButtonShown)
				btnPay.RemoveFromSuperview();
			else
				btnPay.SetTitle(Localize.GetValue("PayNow"), UIControlState.Normal);

			if (ViewModel.IsResendConfirmationButtonShown)
				btnReSendConfirmation.RemoveFromSuperview();
			else
				btnReSendConfirmation.SetTitle(Localize.GetValue("ReSendConfirmation"), UIControlState.Normal);

			var set = this.CreateBindingSet<RideSummaryView, RideSummaryViewModel> ();

			set.BindSafe(btnSendReceipt)
				.For("TouchUpInside")
				.To(vm => vm.SendReceiptCommand);

			set.BindSafe(btnRateRide)
				.For("TouchUpInside")
				.To(vm => vm.NavigateToRatingPage);

			set.BindSafe(btnPay)
				.For("TouchUpInside")
				.To(vm => vm.PayCommand);

			set.BindSafe(btnReSendConfirmation)
				.For("TouchUpInside")
				.To(vm => vm.ResendConfirmationCommand);

			set.Apply ();

            ViewModel.PropertyChanged += (sender, e) => 
            {
                if(ViewModel.ReceiptSent)
                {
					btnSendReceipt.SetTitle(Localize.GetValue("ReceiptSent"), UIControlState.Normal);
					btnSendReceipt.Enabled = false;
                }
            };
		}
	}
}

