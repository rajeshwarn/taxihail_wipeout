// WARNING
//
// This file has been generated automatically by Xamarin Studio Enterprise to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	[Register ("TutorialView")]
	partial class TutorialView
	{
		[Outlet]
		UIKit.UIButton btnClose { get; set; }

		[Outlet]
		UIKit.UIButton btnCloseTarget { get; set; }

		[Outlet]
		UIKit.UIView closeTouchTarget { get; set; }

		[Outlet]
		UIKit.UIView contentView { get; set; }

		[Outlet]
		UIKit.UIPageControl pageControl { get; set; }

		[Outlet]
		UIKit.UIScrollView scrollview { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (btnClose != null) {
				btnClose.Dispose ();
				btnClose = null;
			}

			if (closeTouchTarget != null) {
				closeTouchTarget.Dispose ();
				closeTouchTarget = null;
			}

			if (contentView != null) {
				contentView.Dispose ();
				contentView = null;
			}

			if (pageControl != null) {
				pageControl.Dispose ();
				pageControl = null;
			}

			if (scrollview != null) {
				scrollview.Dispose ();
				scrollview = null;
			}

			if (btnCloseTarget != null) {
				btnCloseTarget.Dispose ();
				btnCloseTarget = null;
			}
		}
	}
}
