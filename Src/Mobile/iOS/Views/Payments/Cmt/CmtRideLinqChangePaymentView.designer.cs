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
	[Register ("CmtRideLinqChangePaymentView")]
	partial class CmtRideLinqChangePaymentView
	{
		[Outlet]
		MonoTouch.UIKit.UILabel lblCardOnFile { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblTipAmount { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatCreditCardTextField txtCreditCard { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.ModalFlatTextField txtTip { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (lblCardOnFile != null) {
				lblCardOnFile.Dispose ();
				lblCardOnFile = null;
			}

			if (lblTipAmount != null) {
				lblTipAmount.Dispose ();
				lblTipAmount = null;
			}

			if (txtCreditCard != null) {
				txtCreditCard.Dispose ();
				txtCreditCard = null;
			}

			if (txtTip != null) {
				txtTip.Dispose ();
				txtTip = null;
			}
		}
	}
}
