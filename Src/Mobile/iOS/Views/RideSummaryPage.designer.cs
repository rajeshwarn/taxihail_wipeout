// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//

using apcurium.MK.Booking.Mobile.Client.Controls;
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	[Register ("RideSummaryPage")]
	partial class RideSummaryPage
	{
		[Outlet]
		MonoTouch.UIKit.UIView ButtonHolderView { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel MessageLabel { get; set; }

		[Outlet]
		GradientButton PayButton { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton RateButton { get; set; }

		[Outlet]
		GradientButton ReSendConfirmationButton { get; set; }

		[Outlet]
		GradientButton SendRecieptButton { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel TitleLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ButtonHolderView != null) {
				ButtonHolderView.Dispose ();
				ButtonHolderView = null;
			}

			if (MessageLabel != null) {
				MessageLabel.Dispose ();
				MessageLabel = null;
			}

			if (PayButton != null) {
				PayButton.Dispose ();
				PayButton = null;
			}

			if (RateButton != null) {
				RateButton.Dispose ();
				RateButton = null;
			}

			if (ReSendConfirmationButton != null) {
				ReSendConfirmationButton.Dispose ();
				ReSendConfirmationButton = null;
			}

			if (SendRecieptButton != null) {
				SendRecieptButton.Dispose ();
				SendRecieptButton = null;
			}

			if (TitleLabel != null) {
				TitleLabel.Dispose ();
				TitleLabel = null;
			}
		}
	}
}
