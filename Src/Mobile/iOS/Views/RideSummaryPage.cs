using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Localization;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Views;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Binding;
using System.Collections.Generic;
using Cirrious.MvvmCross.Binding.BindingContext;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	[MvxViewFor(typeof(RideSummaryViewModel))]
	public partial class RideSummaryPage  : BaseViewController<RideSummaryViewModel>
	{     
		public RideSummaryPage() 
			: base("RideSummaryPage", null)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			TitleLabel.TextColor = AppStyle.GreyText;
			TitleLabel.Font = AppStyle.GetBoldFont(TitleLabel.Font.PointSize);
			MessageLabel.TextColor = AppStyle.GreyText;
			MessageLabel.Font = AppStyle.GetNormalFont (MessageLabel.Font.PointSize);

			AppButtons.FormatStandardButton (SendRecieptButton, Localize.GetValue ("SendReceipt"), AppStyle.ButtonColor.Green);
			AppButtons.FormatStandardButton ((GradientButton)RateButton, Localize.GetValue ("RateBtn"), AppStyle.ButtonColor.Green);
			AppButtons.FormatStandardButton (PayButton, Localize.GetValue ("StatusPayButton"), AppStyle.ButtonColor.Green);
            AppButtons.FormatStandardButton (ReSendConfirmationButton, Localize.GetValue ("ReSendConfirmationButton"), AppStyle.ButtonColor.Green);

			ButtonHolderView.BackgroundColor = UIColor.Clear;

			var set = this.CreateBindingSet<RideSummaryPage, RideSummaryViewModel> ();

			set.Bind(TitleLabel)
				.For(v => v.Text)
				.To(vm => vm.ThankYouTitle);

			set.Bind(MessageLabel)
				.For(v => v.Text)
				.To(vm => vm.ThankYouMessage);

			set.Bind(SendRecieptButton)
				.For("TouchUpInside")
				.To(vm => vm.SendReceiptCommand);
			set.Bind(SendRecieptButton)
				.For(v => v.Hidden)
				.To(vm => vm.IsSendReceiptButtonShown)
				.WithConversion("BoolInverter");

			set.Bind(RateButton)
				.For("TouchUpInside")
				.To(vm => vm.NavigateToRatingPage);
			set.Bind(RateButton)
				.For(v => v.Hidden)
				.To(vm => vm.IsRatingButtonShown)
				.WithConversion("BoolInverter");

			set.Bind(ReSendConfirmationButton)
				.For("TouchUpInside")
				.To(vm => vm.ResendConfirmationCommand);
			set.Bind(ReSendConfirmationButton)
				.For(v => v.Hidden)
				.To(vm => vm.IsResendConfirmationButtonShown)
				.WithConversion("BoolInverter");

			set.Bind(PayButton)
				.For("TouchUpInside")
				.To(vm => vm.PayCommand);
			set.Bind(PayButton)
				.For(v => v.Hidden)
				.To(vm => vm.IsPayButtonShown)
				.WithConversion("BoolInverter");

			set.Apply ();

            ViewModel.PropertyChanged += (sender, e) => 
            {
                if(ViewModel.ReceiptSent)
                {
					AppButtons.FormatStandardButton (SendRecieptButton, Localize.GetValue("HistoryViewSendReceiptSuccess"), AppStyle.ButtonColor.Grey);
                    SendRecieptButton.Enabled = false;
                }

                ButtonHolderView.StackSubViews(0, 6);
            };
		}
	}
}

