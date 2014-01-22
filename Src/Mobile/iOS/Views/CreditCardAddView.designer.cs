// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;
using System.CodeDom.Compiler;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	[Register ("CreditCardAddView")]
	partial class CreditCardAddView
	{
		[Outlet]
		MonoTouch.UIKit.UILabel lblCardNumber { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblCategory { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblCvv { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblExpMonth { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblExpYear { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblNameOnCard { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatTextField txtCardNumber { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.ModalFlatTextField txtCategory { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatTextField txtCvv { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.ModalFlatTextField txtExpMonth { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.ModalFlatTextField txtExpYear { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatTextField txtNameOnCard { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (lblNameOnCard != null) {
				lblNameOnCard.Dispose ();
				lblNameOnCard = null;
			}

			if (lblCardNumber != null) {
				lblCardNumber.Dispose ();
				lblCardNumber = null;
			}

			if (lblCategory != null) {
				lblCategory.Dispose ();
				lblCategory = null;
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

			if (txtCardNumber != null) {
				txtCardNumber.Dispose ();
				txtCardNumber = null;
			}

			if (txtCategory != null) {
				txtCategory.Dispose ();
				txtCategory = null;
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
		}
	}
}
