using UIKit;
using System.Collections.Generic;
using System;

namespace apcurium.MK.Booking.Mobile.Client.Extensions.Helpers
{
    public static class UIViewHelper
    {
        public static void StackSubViews(IEnumerable<UIView> views)
        {           
            nfloat lastBottom = 0f;
            foreach (var view in views) 
            {
				if(view.Hidden)
				{
					continue;
				}

                view.Frame= view.Frame.SetY(lastBottom);
                lastBottom = view.Frame.Bottom;
            }
        }

		public static void StackSubViews(UIView thisView, float topPadding, float spaceBetweenElements)
		{           
            nfloat lastBottom = topPadding;
			foreach (var view in thisView.Subviews) 
			{
				if(view.Hidden)
				{
					continue;
				}
				view.Frame= view.Frame.SetY(lastBottom);

				lastBottom = view.Frame.Bottom + spaceBetweenElements;
			}
		}
    }
}

