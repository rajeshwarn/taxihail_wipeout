// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//

using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	[Register ("TutorialView")]
	partial class TutorialView
	{
		[Outlet]
		MonoTouch.UIKit.UIView contentView { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIPageControl pageControl { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton btnClose { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIScrollView scrollview { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (contentView != null) {
				contentView.Dispose ();
				contentView = null;
			}

			if (pageControl != null) {
				pageControl.Dispose ();
				pageControl = null;
			}

			if (btnClose != null) {
				btnClose.Dispose ();
				btnClose = null;
			}

			if (scrollview != null) {
				scrollview.Dispose ();
				scrollview = null;
			}
		}
	}
}
