// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//

using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client.Order
{
	[Register ("BookRatingCell")]
	partial class BookRatingCell
	{
		[Outlet]
		MonoTouch.UIKit.UILabel ratingTypeName { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton madBtn { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton unhappyBtn { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton neutralBtn { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton happyBtn { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton ecstaticBtn { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ratingTypeName != null) {
				ratingTypeName.Dispose ();
				ratingTypeName = null;
			}

			if (madBtn != null) {
				madBtn.Dispose ();
				madBtn = null;
			}

			if (unhappyBtn != null) {
				unhappyBtn.Dispose ();
				unhappyBtn = null;
			}

			if (neutralBtn != null) {
				neutralBtn.Dispose ();
				neutralBtn = null;
			}

			if (happyBtn != null) {
				happyBtn.Dispose ();
				happyBtn = null;
			}

			if (ecstaticBtn != null) {
				ecstaticBtn.Dispose ();
				ecstaticBtn = null;
			}
		}
	}
}
