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
	[Register ("LocationDetailView")]
	partial class LocationDetailView
	{
		[Outlet]
		GradientButton btnBook { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton btnDelete { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton btnSave { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblName { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField txtAddress { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField txtAptNumber { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField txtName { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField txtRingCode { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (btnBook != null) {
				btnBook.Dispose ();
				btnBook = null;
			}

			if (btnDelete != null) {
				btnDelete.Dispose ();
				btnDelete = null;
			}

			if (btnSave != null) {
				btnSave.Dispose ();
				btnSave = null;
			}

			if (lblName != null) {
				lblName.Dispose ();
				lblName = null;
			}

			if (txtAddress != null) {
				txtAddress.Dispose ();
				txtAddress = null;
			}

			if (txtAptNumber != null) {
				txtAptNumber.Dispose ();
				txtAptNumber = null;
			}

			if (txtName != null) {
				txtName.Dispose ();
				txtName = null;
			}

			if (txtRingCode != null) {
				txtRingCode.Dispose ();
				txtRingCode = null;
			}
		}
	}
}
