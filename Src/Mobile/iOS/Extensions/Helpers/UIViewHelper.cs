using System;
using MonoTouch.UIKit;
using System.Collections.Generic;

namespace apcurium.MK.Booking.Mobile.Client.Extensions.Helpers
{

    public class UIViewHelper
    {
        public static void StackSubViews(IEnumerable<UIView> views)
        {           
            var lastBottom = 0f;
            foreach (var view in views) {
                view.Frame= view.Frame.SetY(lastBottom);
                lastBottom = view.Frame.Bottom;
            }
        }
    }

}

