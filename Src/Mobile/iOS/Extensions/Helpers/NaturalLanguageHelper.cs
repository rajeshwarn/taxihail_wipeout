using System;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using apcurium.MK.Booking.Mobile.Infrastructure;
using Cirrious.CrossCore;

namespace apcurium.MK.Booking.Mobile.Client.Extensions.Helpers
{
	public static class NaturalLanguageHelper
	{
        /** 
        * we can't use UITextAlignment.Natural on iOS6  
        * (NSInvalidArgumentException Reason: textAlignment does not accept NSTextAlignmentNatural)
        * we detect arabic, since it's currently the only RTL language we support
        **/ 
        public static UITextAlignment GetTextAlignment()
        {
            if (UIHelper.IsOS7orHigher) 
            {
                return UITextAlignment.Natural;
            } 
            else 
            {
                if (Mvx.Resolve<ILocalization> ().IsRightToLeft)
                {
                    return UITextAlignment.Right;
                }
                else
                {
                    return UITextAlignment.Left;
                }
            }
        }
	}
}

