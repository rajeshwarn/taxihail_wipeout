// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace apcurium.MK.Booking.Mobile.Client.Order
{
	[Register ("BookRatingCell")]
	partial class BookRatingCell
	{
		[Outlet]
		UIKit.UIButton btnScoreA { get; set; }

		[Outlet]
		UIKit.UIButton btnScoreB { get; set; }

		[Outlet]
		UIKit.UIButton btnScoreC { get; set; }

		[Outlet]
		UIKit.UIButton btnScoreD { get; set; }

		[Outlet]
		UIKit.UIButton btnScoreE { get; set; }

		[Outlet]
		UIKit.UILabel ratingTypeName { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ratingTypeName != null) {
				ratingTypeName.Dispose ();
				ratingTypeName = null;
			}

			if (btnScoreA != null) {
				btnScoreA.Dispose ();
				btnScoreA = null;
			}

			if (btnScoreB != null) {
				btnScoreB.Dispose ();
				btnScoreB = null;
			}

			if (btnScoreC != null) {
				btnScoreC.Dispose ();
				btnScoreC = null;
			}

			if (btnScoreD != null) {
				btnScoreD.Dispose ();
				btnScoreD = null;
			}

			if (btnScoreE != null) {
				btnScoreE.Dispose ();
				btnScoreE = null;
			}
		}
	}
}
