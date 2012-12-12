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
		MonoTouch.UIKit.UITextField txtTipPercent { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField txtCreditCard { get; set; }
		
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

			if (txtTipPercent != null) {
				txtTipPercent.Dispose ();
				txtTipPercent = null;
			}

			if (txtCreditCard != null) {
				txtCreditCard.Dispose ();
				txtCreditCard = null;
			}
		}
	}
}
