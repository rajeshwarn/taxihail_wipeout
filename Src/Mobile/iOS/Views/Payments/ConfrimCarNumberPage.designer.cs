// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client.Views.Payments
{
	[Register ("ConfrimCarNumberPage")]
	partial class ConfrimCarNumberPage
	{
		[Outlet]
		MonoTouch.UIKit.UILabel CarNumber { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton ConfirmButton { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIView Container { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton HideKeyboardButton { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.FormLabel lblConfirmDriverInfo { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.FormLabel lblConfirmDriverNotice { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (CarNumber != null) {
				CarNumber.Dispose ();
				CarNumber = null;
			}

			if (ConfirmButton != null) {
				ConfirmButton.Dispose ();
				ConfirmButton = null;
			}

			if (Container != null) {
				Container.Dispose ();
				Container = null;
			}

			if (HideKeyboardButton != null) {
				HideKeyboardButton.Dispose ();
				HideKeyboardButton = null;
			}

			if (lblConfirmDriverInfo != null) {
				lblConfirmDriverInfo.Dispose ();
				lblConfirmDriverInfo = null;
			}

			if (lblConfirmDriverNotice != null) {
				lblConfirmDriverNotice.Dispose ();
				lblConfirmDriverNotice = null;
			}
		}
	}
}
