// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	[Register ("CreditCardAddView")]
	partial class CreditCardAddView
	{
		[Outlet]
		MonoTouch.UIKit.UIScrollView scrollView { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.FormLabel lblNameOnCard { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.TextField txtNameOnCard { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.FormLabel lblCardNumber { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.TextField txtCardNumber { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.FormLabel lblCardCategory { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.ModalTextField pickerCreditCardType { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.ModalTextField pickerCreditCardCategory { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.FormLabel lblTypeCard { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.FormLabel lblExpMonth { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.FormLabel lblExpYear { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.ModalTextField pickerExpirationMonth { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.ModalTextField pickerExpirationYear { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.FormLabel lblSecurityCode { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.TextField txtSecurityCode { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (scrollView != null) {
				scrollView.Dispose ();
				scrollView = null;
			}

			if (lblNameOnCard != null) {
				lblNameOnCard.Dispose ();
				lblNameOnCard = null;
			}

			if (txtNameOnCard != null) {
				txtNameOnCard.Dispose ();
				txtNameOnCard = null;
			}

			if (lblCardNumber != null) {
				lblCardNumber.Dispose ();
				lblCardNumber = null;
			}

			if (txtCardNumber != null) {
				txtCardNumber.Dispose ();
				txtCardNumber = null;
			}

			if (lblCardCategory != null) {
				lblCardCategory.Dispose ();
				lblCardCategory = null;
			}

			if (pickerCreditCardType != null) {
				pickerCreditCardType.Dispose ();
				pickerCreditCardType = null;
			}

			if (pickerCreditCardCategory != null) {
				pickerCreditCardCategory.Dispose ();
				pickerCreditCardCategory = null;
			}

			if (lblTypeCard != null) {
				lblTypeCard.Dispose ();
				lblTypeCard = null;
			}

			if (lblExpMonth != null) {
				lblExpMonth.Dispose ();
				lblExpMonth = null;
			}

			if (lblExpYear != null) {
				lblExpYear.Dispose ();
				lblExpYear = null;
			}

			if (pickerExpirationMonth != null) {
				pickerExpirationMonth.Dispose ();
				pickerExpirationMonth = null;
			}

			if (pickerExpirationYear != null) {
				pickerExpirationYear.Dispose ();
				pickerExpirationYear = null;
			}

			if (lblSecurityCode != null) {
				lblSecurityCode.Dispose ();
				lblSecurityCode = null;
			}

			if (txtSecurityCode != null) {
				txtSecurityCode.Dispose ();
				txtSecurityCode = null;
			}
		}
	}
}
