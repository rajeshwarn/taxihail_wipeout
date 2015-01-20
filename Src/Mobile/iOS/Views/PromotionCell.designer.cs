// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	[Register ("PromotionCell")]
	partial class PromotionCell
	{
		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnApplyPromo { get; set; }

		[Outlet]
		UIKit.UIImageView imgCollapsed { get; set; }

		[Outlet]
		UIKit.UIImageView imgExpanded { get; set; }

		[Outlet]
		UIKit.UILabel lblDescription { get; set; }

		[Outlet]
		UIKit.UILabel lblExpires { get; set; }

		[Outlet]
		UIKit.UILabel lblName { get; set; }

		[Outlet]
		UIKit.UILabel lblProgress { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (lblProgress != null) {
				lblProgress.Dispose ();
				lblProgress = null;
			}

			if (btnApplyPromo != null) {
				btnApplyPromo.Dispose ();
				btnApplyPromo = null;
			}

			if (imgCollapsed != null) {
				imgCollapsed.Dispose ();
				imgCollapsed = null;
			}

			if (imgExpanded != null) {
				imgExpanded.Dispose ();
				imgExpanded = null;
			}

			if (lblDescription != null) {
				lblDescription.Dispose ();
				lblDescription = null;
			}

			if (lblExpires != null) {
				lblExpires.Dispose ();
				lblExpires = null;
			}

			if (lblName != null) {
				lblName.Dispose ();
				lblName = null;
			}
		}
	}
}
