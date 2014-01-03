// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//

using apcurium.MK.Booking.Mobile.Client.Controls;
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client.Views.Payments
{
	[Register ("BookPaymentSettingsView")]
	partial class BookPaymentSettingsView
	{
		[Outlet]
		GradientButton btConfirm { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.CreditCardButton btCreditCard { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton ClearKeyboardButton { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIView Container { get; set; }

		[Outlet]
		FormLabel lblCreditCardOnFile { get; set; }

		[Outlet]
		FormLabel lblTipAmount { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField MeterAmountLabel { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel MeterAmountStringLabel { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIImageView payPalLogo { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.PaymentSelector payPalToggle { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIScrollView ScrollViewer { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField TipAmountLabel { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Views.Payments.TipSliderControl TipSlider { get; set; }

		[Outlet]
		FormLabel TipStringLabel { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel TotalAmountLabel { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel TotalStringLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (payPalLogo != null) {
				payPalLogo.Dispose ();
				payPalLogo = null;
			}

			if (btConfirm != null) {
				btConfirm.Dispose ();
				btConfirm = null;
			}

			if (btCreditCard != null) {
				btCreditCard.Dispose ();
				btCreditCard = null;
			}

			if (ClearKeyboardButton != null) {
				ClearKeyboardButton.Dispose ();
				ClearKeyboardButton = null;
			}

			if (Container != null) {
				Container.Dispose ();
				Container = null;
			}

			if (lblCreditCardOnFile != null) {
				lblCreditCardOnFile.Dispose ();
				lblCreditCardOnFile = null;
			}

			if (lblTipAmount != null) {
				lblTipAmount.Dispose ();
				lblTipAmount = null;
			}

			if (MeterAmountLabel != null) {
				MeterAmountLabel.Dispose ();
				MeterAmountLabel = null;
			}

			if (MeterAmountStringLabel != null) {
				MeterAmountStringLabel.Dispose ();
				MeterAmountStringLabel = null;
			}

			if (payPalToggle != null) {
				payPalToggle.Dispose ();
				payPalToggle = null;
			}

			if (ScrollViewer != null) {
				ScrollViewer.Dispose ();
				ScrollViewer = null;
			}

			if (TipAmountLabel != null) {
				TipAmountLabel.Dispose ();
				TipAmountLabel = null;
			}

			if (TipSlider != null) {
				TipSlider.Dispose ();
				TipSlider = null;
			}

			if (TipStringLabel != null) {
				TipStringLabel.Dispose ();
				TipStringLabel = null;
			}

			if (TotalAmountLabel != null) {
				TotalAmountLabel.Dispose ();
				TotalAmountLabel = null;
			}

			if (TotalStringLabel != null) {
				TotalStringLabel.Dispose ();
				TotalStringLabel = null;
			}
		}
	}
}
