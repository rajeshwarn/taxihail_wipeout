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
		MonoTouch.UIKit.UITextField CarNumberTextBox { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton ConfirmButton { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton HideKeyboardButton { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (CarNumberTextBox != null) {
				CarNumberTextBox.Dispose ();
				CarNumberTextBox = null;
			}

			if (ConfirmButton != null) {
				ConfirmButton.Dispose ();
				ConfirmButton = null;
			}

			if (HideKeyboardButton != null) {
				HideKeyboardButton.Dispose ();
				HideKeyboardButton = null;
			}
		}
	}
}
