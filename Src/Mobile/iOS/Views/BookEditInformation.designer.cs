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
	[Register ("BookEditInformation")]
	partial class BookEditInformation
	{
		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.StackView contentStack { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.FormLabel lblApartment { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.FormLabel lblChargeType { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.FormLabel lblEntryCode { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.FormLabel lblLargeBags { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.FormLabel lblName { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.FormLabel lblPassengers { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.FormLabel lblPhone { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.FormLabel lblVehiculeType { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.ModalTextField pickerChargeType { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.ModalTextField pickerVehicleType { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIScrollView scrollView { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.TextField txtAprtment { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.TextField txtEntryCode { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.TextField txtLargeBags { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.TextField txtName { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.TextField txtNbPassengers { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.TextField txtPhone { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (contentStack != null) {
				contentStack.Dispose ();
				contentStack = null;
			}

			if (scrollView != null) {
				scrollView.Dispose ();
				scrollView = null;
			}

			if (lblEntryCode != null) {
				lblEntryCode.Dispose ();
				lblEntryCode = null;
			}

			if (lblChargeType != null) {
				lblChargeType.Dispose ();
				lblChargeType = null;
			}

			if (lblVehiculeType != null) {
				lblVehiculeType.Dispose ();
				lblVehiculeType = null;
			}

			if (lblPhone != null) {
				lblPhone.Dispose ();
				lblPhone = null;
			}

			if (lblPassengers != null) {
				lblPassengers.Dispose ();
				lblPassengers = null;
			}

			if (lblLargeBags != null) {
				lblLargeBags.Dispose ();
				lblLargeBags = null;
			}

			if (txtAprtment != null) {
				txtAprtment.Dispose ();
				txtAprtment = null;
			}

			if (txtEntryCode != null) {
				txtEntryCode.Dispose ();
				txtEntryCode = null;
			}

			if (txtNbPassengers != null) {
				txtNbPassengers.Dispose ();
				txtNbPassengers = null;
			}

			if (txtLargeBags != null) {
				txtLargeBags.Dispose ();
				txtLargeBags = null;
			}

			if (txtName != null) {
				txtName.Dispose ();
				txtName = null;
			}

			if (pickerVehicleType != null) {
				pickerVehicleType.Dispose ();
				pickerVehicleType = null;
			}

			if (pickerChargeType != null) {
				pickerChargeType.Dispose ();
				pickerChargeType = null;
			}

			if (lblName != null) {
				lblName.Dispose ();
				lblName = null;
			}

			if (txtPhone != null) {
				txtPhone.Dispose ();
				txtPhone = null;
			}

			if (lblApartment != null) {
				lblApartment.Dispose ();
				lblApartment = null;
			}
		}
	}
}
