// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	[Register ("PaymentPreferenceView")]
	partial class PaymentPreferenceView
	{
		[Outlet]
		MonoTouch.UIKit.UILabel txtCreditCards { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel txtTipAmount { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel txtOptional { get; set; }

		[Outlet]
		MonoTouch.UIKit.UISegmentedControl sgmtPercentOrValue { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField editTipAmount { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton btnConfirm { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (txtCreditCards != null) {
				txtCreditCards.Dispose ();
				txtCreditCards = null;
			}

			if (txtTipAmount != null) {
				txtTipAmount.Dispose ();
				txtTipAmount = null;
			}

			if (txtOptional != null) {
				txtOptional.Dispose ();
				txtOptional = null;
			}

			if (sgmtPercentOrValue != null) {
				sgmtPercentOrValue.Dispose ();
				sgmtPercentOrValue = null;
			}

			if (editTipAmount != null) {
				editTipAmount.Dispose ();
				editTipAmount = null;
			}

			if (btnConfirm != null) {
				btnConfirm.Dispose ();
				btnConfirm = null;
			}
		}
	}
}
