// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//

using apcurium.MK.Booking.Mobile.Client.Controls;
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	[Register ("RefineAddressView")]
	partial class RefineAddressView
	{
		[Outlet]
		MonoTouch.UIKit.UILabel lblAptNumber { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblRingCode { get; set; }

		[Outlet]
		TextField txtAptNumber { get; set; }

		[Outlet]
		TextField txtRingCode { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (lblAptNumber != null) {
				lblAptNumber.Dispose ();
				lblAptNumber = null;
			}

			if (lblRingCode != null) {
				lblRingCode.Dispose ();
				lblRingCode = null;
			}

			if (txtAptNumber != null) {
				txtAptNumber.Dispose ();
				txtAptNumber = null;
			}

			if (txtRingCode != null) {
				txtRingCode.Dispose ();
				txtRingCode = null;
			}
		}
	}
}
