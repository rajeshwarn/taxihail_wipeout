using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Localization;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Views;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Binding;
using System.Collections.Generic;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	[MvxViewFor(typeof(RideSummaryViewModel))]
	public partial class RideSummaryPage  : BaseViewController
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


			AppButtons.FormatStandardButton (SendRecieptButton, Resources.GetValue ("HistoryDetailSendReceiptButton"), AppStyle.ButtonColor.Green);
			AppButtons.FormatStandardButton ((GradientButton)RateButton, Resources.GetValue ("RateBtn"), AppStyle.ButtonColor.Green);
			AppButtons.FormatStandardButton (PayButton, Resources.GetValue ("StatusPayButton"), AppStyle.ButtonColor.Green);
            AppButtons.FormatStandardButton (ReSendConfirmationButton, Resources.GetValue ("ReSendConfirmationButton"), AppStyle.ButtonColor.Green);


			this.AddBindings(new Dictionary<object, string>{
				{ TitleLabel, new B("Text","ThankYouTitle")},
				{ MessageLabel, new B("Text","ThankYouMessage")},
				{ SendRecieptButton, new B("TouchUpInside","SendReceiptCommand")
					.Add("Hidden", "IsSendReceiptButtonShown", "BoolInverter") },
				{ RateButton, new B("TouchUpInside","NavigateToRatingPage")
					.Add("Hidden", "IsRatingButtonShown", "BoolInverter")  },
                { ReSendConfirmationButton, new B("TouchUpInside","ResendConfirmationCommand")
                    .Add("Hidden", "IsResendConfirmationButtonShown", "BoolInverter") },
				{ PayButton, new B("TouchUpInside","PayCommand")
					.Add("Hidden", "IsPayButtonShown", "BoolInverter")  },
			});

			ButtonHolderView.BackgroundColor = UIColor.Clear;

            ViewModel.PropertyChanged += (sender, e) => 
            {
                if(ViewModel.ReceiptSent)
                {
					AppButtons.FormatStandardButton (SendRecieptButton, Resources.GetValue("HistoryViewSendReceiptSuccess"), AppStyle.ButtonColor.Grey);
                    SendRecieptButton.Enabled = false;
                }

                ButtonHolderView.StackSubViews(0, 6);
            };
		}
	}
}

