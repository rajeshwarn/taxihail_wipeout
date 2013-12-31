// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//

using apcurium.MK.Booking.Mobile.Client.Controls;
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	[Register ("CreditCardAddView")]
	partial class CreditCardAddView
	{
		[Outlet]
		FormLabel lblCardCategory { get; set; }

		[Outlet]
		FormLabel lblCardNumber { get; set; }

		[Outlet]
		FormLabel lblExpMonth { get; set; }

		[Outlet]
		FormLabel lblExpYear { get; set; }

		[Outlet]
		FormLabel lblNameOnCard { get; set; }

		[Outlet]
		FormLabel lblSecurityCode { get; set; }

		[Outlet]
		FormLabel lblTypeCard { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.ModalTextField pickerCreditCardCategory { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.ModalTextField pickerCreditCardType { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.ModalTextField pickerExpirationMonth { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.ModalTextField pickerExpirationYear { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIScrollView scrollView { get; set; }

		[Outlet]
		TextField txtCardNumber { get; set; }

		[Outlet]
		TextField txtNameOnCard { get; set; }

		[Outlet]
		TextField txtSecurityCode { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (lblCardCategory != null) {
				lblCardCategory.Dispose ();
				lblCardCategory = null;
			}

			if (lblCardNumber != null) {
				lblCardNumber.Dispose ();
				lblCardNumber = null;
			}

			if (lblExpMonth != null) {
				lblExpMonth.Dispose ();
				lblExpMonth = null;
			}

			if (lblExpYear != null) {
				lblExpYear.Dispose ();
				lblExpYear = null;
			}

			if (lblNameOnCard != null) {
				lblNameOnCard.Dispose ();
				lblNameOnCard = null;
			}

			if (lblSecurityCode != null) {
				lblSecurityCode.Dispose ();
				lblSecurityCode = null;
			}

			if (lblTypeCard != null) {
				lblTypeCard.Dispose ();
				lblTypeCard = null;
			}

			if (pickerCreditCardCategory != null) {
				pickerCreditCardCategory.Dispose ();
				pickerCreditCardCategory = null;
			}

			if (pickerCreditCardType != null) {
				pickerCreditCardType.Dispose ();
				pickerCreditCardType = null;
			}

			if (pickerExpirationMonth != null) {
				pickerExpirationMonth.Dispose ();
				pickerExpirationMonth = null;
			}

			if (pickerExpirationYear != null) {
				pickerExpirationYear.Dispose ();
				pickerExpirationYear = null;
			}

			if (scrollView != null) {
				scrollView.Dispose ();
				scrollView = null;
			}

			if (txtCardNumber != null) {
				txtCardNumber.Dispose ();
				txtCardNumber = null;
			}

			if (txtNameOnCard != null) {
				txtNameOnCard.Dispose ();
				txtNameOnCard = null;
			}

			if (txtSecurityCode != null) {
				txtSecurityCode.Dispose ();
				txtSecurityCode = null;
			}
		}
	}
}
