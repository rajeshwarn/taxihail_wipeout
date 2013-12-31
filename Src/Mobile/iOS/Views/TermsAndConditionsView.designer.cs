// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//

using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	[Register ("TermsAndConditionsView")]
	partial class TermsAndConditionsView
	{
		[Outlet]
		MonoTouch.UIKit.UIButton btnAccept { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton btnCancel { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblAcknowledgment { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblTitle { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextView txtTermsAndConditions { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (btnAccept != null) {
				btnAccept.Dispose ();
				btnAccept = null;
			}

			if (btnCancel != null) {
				btnCancel.Dispose ();
				btnCancel = null;
			}

			if (lblTitle != null) {
				lblTitle.Dispose ();
				lblTitle = null;
			}

			if (txtTermsAndConditions != null) {
				txtTermsAndConditions.Dispose ();
				txtTermsAndConditions = null;
			}

			if (lblAcknowledgment != null) {
				lblAcknowledgment.Dispose ();
				lblAcknowledgment = null;
			}
		}
	}
}
