// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;
using System.CodeDom.Compiler;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	[Register ("OrderReviewItem")]
	partial class OrderReviewItem
	{
		[Outlet]
		MonoTouch.UIKit.UIImageView IconImage { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel ValueLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ValueLabel != null) {
				ValueLabel.Dispose ();
				ValueLabel = null;
			}

            if (IconImage != null)
            {
				IconImage.Dispose ();
				IconImage = null;
			}
		}
	}
}
