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
	[Register ("RideSummaryView")]
	partial class RideSummaryView
	{
		[Outlet]
		MonoTouch.UIKit.NSLayoutConstraint constraintRatingTableHeight { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblSubTitle { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITableView tableRatingList { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (constraintRatingTableHeight != null) {
				constraintRatingTableHeight.Dispose ();
				constraintRatingTableHeight = null;
			}

			if (lblSubTitle != null) {
				lblSubTitle.Dispose ();
				lblSubTitle = null;
			}

			if (tableRatingList != null) {
				tableRatingList.Dispose ();
				tableRatingList = null;
			}
		}
	}
}
