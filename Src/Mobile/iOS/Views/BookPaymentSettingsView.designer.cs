// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	[Register ("BookPaymentSettingsView")]
	partial class BookPaymentSettingsView
	{
		[Outlet]
		apcurium.MK.Booking.Mobile.Client.GradientButton btCancel { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.BottomBar bottomBar { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.GradientButton btConfirm { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.FormLabel lblCreditCardOnFile { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.CreditCardButton btCreditCard { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.TextField txtTipAmount { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.FormLabel lblTipAmount { get; set; }

		[Outlet]
		MonoTouch.UIKit.UISegmentedControl segmentTip { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblOptional { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (btCancel != null) {
				btCancel.Dispose ();
				btCancel = null;
			}

			if (bottomBar != null) {
				bottomBar.Dispose ();
				bottomBar = null;
			}

			if (btConfirm != null) {
				btConfirm.Dispose ();
				btConfirm = null;
			}

			if (lblCreditCardOnFile != null) {
				lblCreditCardOnFile.Dispose ();
				lblCreditCardOnFile = null;
			}

			if (btCreditCard != null) {
				btCreditCard.Dispose ();
				btCreditCard = null;
			}

			if (txtTipAmount != null) {
				txtTipAmount.Dispose ();
				txtTipAmount = null;
			}

			if (lblTipAmount != null) {
				lblTipAmount.Dispose ();
				lblTipAmount = null;
			}

			if (segmentTip != null) {
				segmentTip.Dispose ();
				segmentTip = null;
			}

			if (lblOptional != null) {
				lblOptional.Dispose ();
				lblOptional = null;
			}
		}
	}
}
