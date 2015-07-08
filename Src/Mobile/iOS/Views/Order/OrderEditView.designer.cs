// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace apcurium.MK.Booking.Mobile.Client.Views.Order
{
	[Register ("OrderEditView")]
	partial class OrderEditView
	{
		[Outlet]
		UIKit.UILabel lblApartment { get; set; }

		[Outlet]
		UIKit.UILabel lblChargeType { get; set; }

		[Outlet]
		UIKit.UILabel lblEntryCode { get; set; }

		[Outlet]
		UIKit.UILabel lblLargeBags { get; set; }

		[Outlet]
		UIKit.UILabel lblName { get; set; }

		[Outlet]
		UIKit.UILabel lblPassengers { get; set; }

		[Outlet]
		UIKit.UILabel lblPhone { get; set; }

		[Outlet]
        apcurium.MK.Booking.Mobile.Client.Controls.Widgets.CountrySelector lblPhoneDialCode { get; set; }

		[Outlet]
		UIKit.UIView phoneNumberView { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatTextField txtApartment { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.ModalFlatTextField txtChargeType { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatTextField txtEntryCode { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatTextField txtLargeBags { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatTextField txtName { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatTextField txtPassengers { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatTextField txtPhone { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (lblApartment != null) {
				lblApartment.Dispose ();
				lblApartment = null;
			}

			if (lblChargeType != null) {
				lblChargeType.Dispose ();
				lblChargeType = null;
			}

			if (lblEntryCode != null) {
				lblEntryCode.Dispose ();
				lblEntryCode = null;
			}

			if (lblLargeBags != null) {
				lblLargeBags.Dispose ();
				lblLargeBags = null;
			}

			if (lblName != null) {
				lblName.Dispose ();
				lblName = null;
			}

			if (lblPassengers != null) {
				lblPassengers.Dispose ();
				lblPassengers = null;
			}

			if (lblPhone != null) {
				lblPhone.Dispose ();
				lblPhone = null;
			}

			if (txtApartment != null) {
				txtApartment.Dispose ();
				txtApartment = null;
			}

			if (txtChargeType != null) {
				txtChargeType.Dispose ();
				txtChargeType = null;
			}

			if (txtEntryCode != null) {
				txtEntryCode.Dispose ();
				txtEntryCode = null;
			}

			if (txtLargeBags != null) {
				txtLargeBags.Dispose ();
				txtLargeBags = null;
			}

			if (txtName != null) {
				txtName.Dispose ();
				txtName = null;
			}

			if (txtPassengers != null) {
				txtPassengers.Dispose ();
				txtPassengers = null;
			}

			if (txtPhone != null) {
				txtPhone.Dispose ();
				txtPhone = null;
			}

			if (lblPhoneDialCode != null) {
				lblPhoneDialCode.Dispose ();
				lblPhoneDialCode = null;
			}

			if (phoneNumberView != null) {
				phoneNumberView.Dispose ();
				phoneNumberView = null;
			}
		}
	}
}
