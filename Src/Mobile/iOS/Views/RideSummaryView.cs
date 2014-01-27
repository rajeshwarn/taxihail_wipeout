using System;
using System.Collections.Generic;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Views;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Binding;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Localization;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class RideSummaryView  : BaseViewController<RideSummaryViewModel>
	{     
		public RideSummaryView() : base("RideSummaryView", null)
		{
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

            NavigationController.NavigationBar.Hidden = false;
			NavigationItem.HidesBackButton = false;
            NavigationItem.Title = Localize.GetValue("RideSummaryTitleText");
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

            View.BackgroundColor = UIColor.FromRGB(242, 242, 242);

            FlatButtonStyle.Green.ApplyTo(btnRateRide);
			FlatButtonStyle.Green.ApplyTo(btnSendReceipt);
			FlatButtonStyle.Green.ApplyTo(btnReSendConfirmation);
			FlatButtonStyle.Green.ApplyTo(btnPay);

			lblSubTitle.Text = String.Format(Localize.GetValue ("RideSummarySubTitleText"), this.Services().Settings.ApplicationName);

            btnSendReceipt.SetTitle(Localize.GetValue("SendReceipt"), UIControlState.Normal);
            btnRateRide.SetTitle(Localize.GetValue("RateRide"), UIControlState.Normal);
            btnPay.SetTitle(Localize.GetValue("PayNow"), UIControlState.Normal);
            btnReSendConfirmation.SetTitle(Localize.GetValue("ReSendConfirmation"), UIControlState.Normal);

			var set = this.CreateBindingSet<RideSummaryView, RideSummaryViewModel> ();

            set.Bind(btnSendReceipt)
				.For("TouchUpInside")
				.To(vm => vm.SendReceiptCommand);
            set.Bind(btnSendReceipt)
                .For(v => v.HiddenWithConstraints)
                .To(vm => vm.IsSendReceiptButtonShown)
                .WithConversion("BoolInverter");

            set.Bind(btnRateRide)
				.For("TouchUpInside")
				.To(vm => vm.NavigateToRatingPage);
            set.Bind(btnRateRide)
                .For(v => v.HiddenWithConstraints)
                .To(vm => vm.IsRatingButtonShown)
                .WithConversion("BoolInverter");

            set.Bind(btnPay)
				.For("TouchUpInside")
				.To(vm => vm.PayCommand);
            set.Bind(btnPay)
                .For(v => v.HiddenWithConstraints)
                .To(vm => vm.IsPayButtonShown)
                .WithConversion("BoolInverter");

            set.Bind(btnReSendConfirmation)
				.For("TouchUpInside")
				.To(vm => vm.ResendConfirmationCommand);
            set.Bind(btnReSendConfirmation)
                .For(v => v.HiddenWithConstraints)
                .To(vm => vm.IsResendConfirmationButtonShown)
                .WithConversion("BoolInverter");

			set.Apply ();

            ViewModel.PropertyChanged += (sender, e) => 
            {
                if(e.PropertyName == "ReceiptSent")
                {
                    if(ViewModel.ReceiptSent)
                    {
                        btnSendReceipt.SetTitle(Localize.GetValue("ReceiptSent"), UIControlState.Normal);
                        btnSendReceipt.Enabled = false;
                    }
                }
            };
		}
	}
}

