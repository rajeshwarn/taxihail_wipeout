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
		UIKit.UILabel Company { get; set; }

		[Outlet]
		UIKit.UILabel DateOfTransaction { get; set; }

		[Outlet]
		UIKit.UILabel IbsOrder { get; set; }

		[Outlet]
		UIKit.UILabel Last4 { get; set; }

		[Outlet]
		UIKit.UILabel lblAmountDue { get; set; }

		[Outlet]
		UIKit.UILabel lblCompany { get; set; }

		[Outlet]
		UIKit.UILabel lblDate { get; set; }

		[Outlet]
		UIKit.UILabel lblInstructions { get; set; }

		[Outlet]
		UIKit.UILabel lblLast4 { get; set; }

		[Outlet]
		UIKit.UILabel lblNoCreditCard { get; set; }

		[Outlet]
		UIKit.UILabel lblOrderId { get; set; }

		[Outlet]
		UIKit.UILabel lblSelectedCreditCard { get; set; }

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

			if (DateOfTransaction != null) {
				DateOfTransaction.Dispose ();
				DateOfTransaction = null;
			}

			if (IbsOrder != null) {
				IbsOrder.Dispose ();
				IbsOrder = null;
			}

			if (lblAmountDue != null) {
				lblAmountDue.Dispose ();
				lblAmountDue = null;
			}

			if (lblDate != null) {
				lblDate.Dispose ();
				lblDate = null;
			}

			if (lblInstructions != null) {
				lblInstructions.Dispose ();
				lblInstructions = null;
			}

			if (lblOrderId != null) {
				lblOrderId.Dispose ();
				lblOrderId = null;
			}

			if (lblTransactionId != null) {
				lblTransactionId.Dispose ();
				lblTransactionId = null;
			}

			if (TransactionId != null) {
				TransactionId.Dispose ();
				TransactionId = null;
			}

			if (lblSelectedCreditCard != null) {
				lblSelectedCreditCard.Dispose ();
				lblSelectedCreditCard = null;
			}

			if (lblNoCreditCard != null) {
				lblNoCreditCard.Dispose ();
				lblNoCreditCard = null;
			}

			if (lblCompany != null) {
				lblCompany.Dispose ();
				lblCompany = null;
			}

			if (Company != null) {
				Company.Dispose ();
				Company = null;
			}

			if (lblLast4 != null) {
				lblLast4.Dispose ();
				lblLast4 = null;
			}

			if (Last4 != null) {
				Last4.Dispose ();
				Last4 = null;
			}
		}
	}
}
