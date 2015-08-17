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
		UIKit.UILabel lblOrderNumber { get; set; }

		[Outlet]
		UIKit.UILabel lblOrderStatus { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (lblOrderNumber != null) {
				lblOrderNumber.Dispose ();
				lblOrderNumber = null;
			}

			if (lblOrderStatus != null) {
				lblOrderStatus.Dispose ();
				lblOrderStatus = null;
			}
		}
	}
}
