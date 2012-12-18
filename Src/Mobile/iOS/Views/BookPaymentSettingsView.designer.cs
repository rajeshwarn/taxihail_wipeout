// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
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
		apcurium.MK.Booking.Mobile.Client.GradientButton btCancel { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.BottomBar bottomBar { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.GradientButton btConfirm { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (btCancel != null) {
				btCancel.Dispose ();
				btCancel = null;
			}

			if (bottomBar != null) {
				bottomBar.Dispose ();
				bottomBar = null;
			}

			if (btConfirm != null) {
				btConfirm.Dispose ();
				btConfirm = null;
			}
		}
	}
}
