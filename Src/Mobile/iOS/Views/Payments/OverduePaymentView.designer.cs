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
	[Register ("OverduePaymentView")]
	partial class OverduePaymentView
	{
		[Outlet]
		UIKit.UILabel AmountDue { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnAddNewCard { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnRetry { get; set; }

		[Outlet]
		UIKit.UILabel DateOfTransaction { get; set; }

		[Outlet]
		UIKit.UILabel IbsOrder { get; set; }

		[Outlet]
		UIKit.UILabel lblAmountDue { get; set; }

		[Outlet]
		UIKit.UILabel lblDate { get; set; }

		[Outlet]
		UIKit.UILabel lblInstructions { get; set; }

		[Outlet]
		UIKit.UILabel lblOrderId { get; set; }

		[Outlet]
		UIKit.UILabel lblTransactionId { get; set; }

		[Outlet]
		UIKit.UILabel TransactionId { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (AmountDue != null) {
				AmountDue.Dispose ();
				AmountDue = null;
			}

			if (btnAddNewCard != null) {
				btnAddNewCard.Dispose ();
				btnAddNewCard = null;
			}

			if (btnRetry != null) {
				btnRetry.Dispose ();
				btnRetry = null;
			}

			if (lblOrderId != null) {
				lblOrderId.Dispose ();
				lblOrderId = null;
			}

			if (IbsOrder != null) {
				IbsOrder.Dispose ();
				IbsOrder = null;
			}

			if (lblInstructions != null) {
				lblInstructions.Dispose ();
				lblInstructions = null;
			}

			if (DateOfTransaction != null) {
				DateOfTransaction.Dispose ();
				DateOfTransaction = null;
			}

			if (lblAmountDue != null) {
				lblAmountDue.Dispose ();
				lblAmountDue = null;
			}

			if (lblDate != null) {
				lblDate.Dispose ();
				lblDate = null;
			}

			if (lblTransactionId != null) {
				lblTransactionId.Dispose ();
				lblTransactionId = null;
			}

			if (TransactionId != null) {
				TransactionId.Dispose ();
				TransactionId = null;
			}
		}
	}
}
