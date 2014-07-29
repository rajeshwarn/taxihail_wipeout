// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;
using System.CodeDom.Compiler;

namespace apcurium.MK.Booking.Mobile.Client.Views.Payments
{
	[Register ("PaymentView")]
	partial class PaymentView
	{
		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnConfirm { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton ClearKeyboardButton { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIImageView imgPayPal { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblMeterAmount { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblTip { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblTipAmount { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblTotal { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblTotalValue { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.PaymentSelector payPalToggle { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatTextField txtMeterAmount { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.ModalFlatTextField txtTip { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatTextField txtTipAmount { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (btnConfirm != null) {
				btnConfirm.Dispose ();
				btnConfirm = null;
			}

			if (ClearKeyboardButton != null) {
				ClearKeyboardButton.Dispose ();
				ClearKeyboardButton = null;
			}

			if (imgPayPal != null) {
				imgPayPal.Dispose ();
				imgPayPal = null;
			}

			if (lblMeterAmount != null) {
				lblMeterAmount.Dispose ();
				lblMeterAmount = null;
			}

			if (lblTip != null) {
				lblTip.Dispose ();
				lblTip = null;
			}

			if (lblTipAmount != null) {
				lblTipAmount.Dispose ();
				lblTipAmount = null;
			}

			if (lblTotal != null) {
				lblTotal.Dispose ();
				lblTotal = null;
			}

			if (lblTotalValue != null) {
				lblTotalValue.Dispose ();
				lblTotalValue = null;
			}

			if (payPalToggle != null) {
				payPalToggle.Dispose ();
				payPalToggle = null;
			}

			if (txtMeterAmount != null) {
				txtMeterAmount.Dispose ();
				txtMeterAmount = null;
			}

			if (txtTip != null) {
				txtTip.Dispose ();
				txtTip = null;
			}

			if (txtTipAmount != null) {
				txtTipAmount.Dispose ();
				txtTipAmount = null;
			}
		}
	}
}
