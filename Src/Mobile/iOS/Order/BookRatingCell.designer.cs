// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client
{
	[Register ("BookRatingCell")]
	partial class BookRatingCell
	{
		[Outlet]
		MonoTouch.UIKit.UILabel ratingTypeName { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ratingTypeName != null) {
				ratingTypeName.Dispose ();
				ratingTypeName = null;
			}
		}
	}
}
