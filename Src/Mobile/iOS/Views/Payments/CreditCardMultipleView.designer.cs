// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace apcurium.MK.Booking.Mobile.Client.Views.Payments
{
	[Register ("CreditCardMultipleView")]
	partial class CreditCardMultipleView
	{
		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnAddCard { get; set; }

		[Outlet]
		UIKit.UILabel lblPaymentMethods { get; set; }

		[Outlet]
		UIKit.UILabel lblTip { get; set; }

		[Outlet]
		UIKit.UITableView tblCreditCards { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.ModalFlatTextField txtTip { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (lblTip != null) {
				lblTip.Dispose ();
				lblTip = null;
			}

			if (txtTip != null) {
				txtTip.Dispose ();
				txtTip = null;
			}

			if (btnAddCard != null) {
				btnAddCard.Dispose ();
				btnAddCard = null;
			}

			if (lblPaymentMethods != null) {
				lblPaymentMethods.Dispose ();
				lblPaymentMethods = null;
			}

			if (tblCreditCards != null) {
				tblCreditCards.Dispose ();
				tblCreditCards = null;
			}
		}
	}
}
