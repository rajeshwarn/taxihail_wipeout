// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
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
		apcurium.MK.Booking.Mobile.Client.TextField txtExpMonth { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.TextField txtExpYear { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.FormLabel lblSecurityCode { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.TextField txtSecurityCode { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.FormLabel lblZipCode { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.TextField txtZipCode { get; set; }
		
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

			if (txtExpMonth != null) {
				txtExpMonth.Dispose ();
				txtExpMonth = null;
			}

			if (txtExpYear != null) {
				txtExpYear.Dispose ();
				txtExpYear = null;
			}

			if (lblSecurityCode != null) {
				lblSecurityCode.Dispose ();
				lblSecurityCode = null;
			}

			if (txtSecurityCode != null) {
				txtSecurityCode.Dispose ();
				txtSecurityCode = null;
			}

			if (lblZipCode != null) {
				lblZipCode.Dispose ();
				lblZipCode = null;
			}

			if (txtZipCode != null) {
				txtZipCode.Dispose ();
				txtZipCode = null;
			}
		}
	}
}
