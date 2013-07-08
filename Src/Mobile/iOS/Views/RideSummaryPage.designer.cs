// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	[Register ("RideSummaryPage")]
	partial class RideSummaryPage
	{
		[Outlet]
		MonoTouch.UIKit.UILabel MessageLabel { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton RateButton { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton SendRecieptButton { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel TitleLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (MessageLabel != null) {
				MessageLabel.Dispose ();
				MessageLabel = null;
			}

			if (SendRecieptButton != null) {
				SendRecieptButton.Dispose ();
				SendRecieptButton = null;
			}

			if (TitleLabel != null) {
				TitleLabel.Dispose ();
				TitleLabel = null;
			}

			if (RateButton != null) {
				RateButton.Dispose ();
				RateButton = null;
			}
		}
	}
}
