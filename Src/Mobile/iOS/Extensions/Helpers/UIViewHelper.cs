using MonoTouch.UIKit;
using System.Collections.Generic;

namespace apcurium.MK.Booking.Mobile.Client.Extensions.Helpers
{

    public static class UIViewHelper
    {
        public static void StackSubViews(IEnumerable<UIView> views)
        {           
            var lastBottom = 0f;
            foreach (var view in views) {
				
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
			var lastBottom = topPadding;

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

