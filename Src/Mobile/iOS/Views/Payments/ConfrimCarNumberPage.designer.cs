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

		[Outlet]
		MonoTouch.UIKit.UIView Container { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel TextLine1 { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel TextLine2 { get; set; }
		
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

			if (Container != null) {
				Container.Dispose ();
				Container = null;
			}

			if (TextLine1 != null) {
				TextLine1.Dispose ();
				TextLine1 = null;
			}

			if (TextLine2 != null) {
				TextLine2.Dispose ();
				TextLine2 = null;
			}
		}
	}
}
