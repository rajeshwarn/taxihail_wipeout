// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	[Register ("CreditCardAddView")]
	partial class CreditCardAddView
	{
		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnDeleteCard { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnLinkPayPal { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnSaveCard { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnScanCard { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnUnlinkPayPal { get; set; }

		[Outlet]
		UIKit.UILabel lblCardNumber { get; set; }

		[Outlet]
		UIKit.UILabel lblCvv { get; set; }

		[Outlet]
		UIKit.UILabel lblExpMonth { get; set; }

		[Outlet]
		UIKit.UILabel lblExpYear { get; set; }

		[Outlet]
		UIKit.UILabel lblInstructions { get; set; }

		[Outlet]
		UIKit.UILabel lblNameOnCard { get; set; }

		[Outlet]
		UIKit.UILabel lblPayPalLinkedInfo { get; set; }

		[Outlet]
		UIKit.UILabel lblTip { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatTextField txtCardNumber { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatTextField txtCvv { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.ModalFlatTextField txtExpMonth { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.ModalFlatTextField txtExpYear { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatTextField txtNameOnCard { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.ModalFlatTextField txtTip { get; set; }

		[Outlet]
		UIKit.UIView viewCreditCard { get; set; }

		[Outlet]
		UIKit.UIView viewPayPal { get; set; }

		[Outlet]
		UIKit.UIView viewPayPalIsLinkedInfo { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (btnDeleteCard != null) {
				btnDeleteCard.Dispose ();
				btnDeleteCard = null;
			}

			if (btnLinkPayPal != null) {
				btnLinkPayPal.Dispose ();
				btnLinkPayPal = null;
			}

			if (btnSaveCard != null) {
				btnSaveCard.Dispose ();
				btnSaveCard = null;
			}

			if (btnScanCard != null) {
				btnScanCard.Dispose ();
				btnScanCard = null;
			}

			if (btnUnlinkPayPal != null) {
				btnUnlinkPayPal.Dispose ();
				btnUnlinkPayPal = null;
			}

			if (lblCardNumber != null) {
				lblCardNumber.Dispose ();
				lblCardNumber = null;
			}

			if (lblCvv != null) {
				lblCvv.Dispose ();
				lblCvv = null;
			}

			if (lblExpMonth != null) {
				lblExpMonth.Dispose ();
				lblExpMonth = null;
			}

			if (lblExpYear != null) {
				lblExpYear.Dispose ();
				lblExpYear = null;
			}

			if (lblInstructions != null) {
				lblInstructions.Dispose ();
				lblInstructions = null;
			}

			if (lblNameOnCard != null) {
				lblNameOnCard.Dispose ();
				lblNameOnCard = null;
			}

			if (lblPayPalLinkedInfo != null) {
				lblPayPalLinkedInfo.Dispose ();
				lblPayPalLinkedInfo = null;
			}

			if (txtCardNumber != null) {
				txtCardNumber.Dispose ();
				txtCardNumber = null;
			}

			if (txtCvv != null) {
				txtCvv.Dispose ();
				txtCvv = null;
			}

			if (txtExpMonth != null) {
				txtExpMonth.Dispose ();
				txtExpMonth = null;
			}

			if (txtExpYear != null) {
				txtExpYear.Dispose ();
				txtExpYear = null;
			}

			if (txtNameOnCard != null) {
				txtNameOnCard.Dispose ();
				txtNameOnCard = null;
			}

			if (viewCreditCard != null) {
				viewCreditCard.Dispose ();
				viewCreditCard = null;
			}

			if (viewPayPal != null) {
				viewPayPal.Dispose ();
				viewPayPal = null;
			}

			if (viewPayPalIsLinkedInfo != null) {
				viewPayPalIsLinkedInfo.Dispose ();
				viewPayPalIsLinkedInfo = null;
			}

			if (lblTip != null) {
				lblTip.Dispose ();
				lblTip = null;
			}

			if (txtTip != null) {
				txtTip.Dispose ();
				txtTip = null;
			}
		}
	}
}
