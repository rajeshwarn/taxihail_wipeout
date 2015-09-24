// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	[Register ("BookingStatusControl")]
	partial class BookingStatusControl
	{
		[Outlet]
		Cirrious.MvvmCross.Binding.Touch.Views.MvxImageView driverPhoto { get; set; }

		[Outlet]
		UIKit.UILabel lblDriverName { get; set; }

		[Outlet]
		UIKit.UILabel lblOrderNumber { get; set; }

		[Outlet]
		UIKit.UILabel lblOrderStatus { get; set; }

		[Outlet]
		UIKit.UILabel lblVehicleInfos { get; set; }

		[Outlet]
		UIKit.UIView viewDriverInfos { get; set; }

		[Outlet]
		UIKit.UIView viewStatus { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (driverPhoto != null) {
				driverPhoto.Dispose ();
				driverPhoto = null;
			}

			if (lblDriverName != null) {
				lblDriverName.Dispose ();
				lblDriverName = null;
			}

			if (lblOrderNumber != null) {
				lblOrderNumber.Dispose ();
				lblOrderNumber = null;
			}

			if (lblOrderStatus != null) {
				lblOrderStatus.Dispose ();
				lblOrderStatus = null;
			}

			if (lblVehicleInfos != null) {
				lblVehicleInfos.Dispose ();
				lblVehicleInfos = null;
			}

			if (viewDriverInfos != null) {
				viewDriverInfos.Dispose ();
				viewDriverInfos = null;
			}

			if (viewStatus != null) {
				viewStatus.Dispose ();
				viewStatus = null;
			}
		}
	}
}
