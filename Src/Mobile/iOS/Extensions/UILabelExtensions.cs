using System;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;

namespace apcurium.MK.Booking.Mobile.Client.Extensions
{
	public static class UILabelExtensions
	{
		public static void TextAlignment(this UILabel label)
		{
			if (UIHelper.IsOS7orHigher) 
			{
				label.TextAlignment = UITextAlignment.Natural;

			} 
			else 
			{
				if (label.Services ().Localize.IsRightToLeft)
				{
					label.TextAlignment = UITextAlignment.Right;
				}
			}
		}
	}
}

