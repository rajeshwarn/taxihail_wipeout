// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
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
		MonoTouch.UIKit.UIView Container { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.FormLabel lblTipAmount { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.GradientButton btConfirm { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.FormLabel lblCreditCardOnFile { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.CreditCardButton btCreditCard { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Views.Payments.TipSliderControl TipSlider { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField MeterAmountLabel { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField TipAmountLabel { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel TotalAmountLabel { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel MeterAmountStringLabel { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.FormLabel TipStringLabel { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel TotalStringLabel { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton ClearKeyboardButton { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIScrollView ScrollViewer { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (Container != null) {
				Container.Dispose ();
				Container = null;
			}

			if (lblTipAmount != null) {
				lblTipAmount.Dispose ();
				lblTipAmount = null;
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

			if (TipSlider != null) {
				TipSlider.Dispose ();
				TipSlider = null;
			}

			if (MeterAmountLabel != null) {
				MeterAmountLabel.Dispose ();
				MeterAmountLabel = null;
			}

			if (TipAmountLabel != null) {
				TipAmountLabel.Dispose ();
				TipAmountLabel = null;
			}

			if (TotalAmountLabel != null) {
				TotalAmountLabel.Dispose ();
				TotalAmountLabel = null;
			}

			if (MeterAmountStringLabel != null) {
				MeterAmountStringLabel.Dispose ();
				MeterAmountStringLabel = null;
			}

			if (TipStringLabel != null) {
				TipStringLabel.Dispose ();
				TipStringLabel = null;
			}

			if (TotalStringLabel != null) {
				TotalStringLabel.Dispose ();
				TotalStringLabel = null;
			}

			if (ClearKeyboardButton != null) {
				ClearKeyboardButton.Dispose ();
				ClearKeyboardButton = null;
			}

			if (ScrollViewer != null) {
				ScrollViewer.Dispose ();
				ScrollViewer = null;
			}
		}
	}
}
