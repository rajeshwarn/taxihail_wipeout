// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client.Views.Payment
{
	[Register ("PaymentDetails")]
	partial class PaymentDetails
	{
		[Outlet]
		MonoTouch.UIKit.UILabel lblTipAmount { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblCreditCard { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField txtTipAmount { get; set; }

		[Outlet]
		MonoTouch.UIKit.UISegmentedControl sgmtPercentOrValue { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel btnCreditCard { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lnlNoCreditCard { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblOptional { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (lblTipAmount != null) {
				lblTipAmount.Dispose ();
				lblTipAmount = null;
			}

			if (lblCreditCard != null) {
				lblCreditCard.Dispose ();
				lblCreditCard = null;
			}

			if (txtTipAmount != null) {
				txtTipAmount.Dispose ();
				txtTipAmount = null;
			}

			if (sgmtPercentOrValue != null) {
				sgmtPercentOrValue.Dispose ();
				sgmtPercentOrValue = null;
			}

			if (btnCreditCard != null) {
				btnCreditCard.Dispose ();
				btnCreditCard = null;
			}

			if (lnlNoCreditCard != null) {
				lnlNoCreditCard.Dispose ();
				lnlNoCreditCard = null;
			}

			if (lblOptional != null) {
				lblOptional.Dispose ();
				lblOptional = null;
			}
		}
	}
}
