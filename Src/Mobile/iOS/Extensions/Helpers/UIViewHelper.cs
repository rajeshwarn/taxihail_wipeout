using UIKit;
using System.Collections.Generic;
using System;
using Foundation;
using CoreGraphics;

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

        public static void ReactToKeyboardWillShowNotification(UIView notifiedView, UIView activeView, bool isCalledFromChildView, NSNotification notification)
        {
            var topView = isCalledFromChildView ? notifiedView.Superview : notifiedView;

            if (activeView == null)
            {
                return;
            }

            var scrollView = activeView.FindSuperviewOfType(notifiedView, typeof(UIScrollView)) as UIScrollView;
            if (scrollView == null)
            {
                return;
            }

            // find the topmost scrollview (fix problem with RootElement)
            var nextSuperView = scrollView;
            while(nextSuperView != null)
            {
                scrollView = nextSuperView;
                nextSuperView = scrollView.FindSuperviewOfType(notifiedView, typeof(UIScrollView)) as UIScrollView;
            }

            var keyboardBounds = ((NSValue)notification.UserInfo.ValueForKey(UIKeyboard.FrameEndUserInfoKey)).RectangleFValue;

            var contentInsets = new UIEdgeInsets(0.0f, 0.0f, keyboardBounds.Size.Height + topView.Frame.Y, 0.0f);
            scrollView.ContentInset = contentInsets;
            scrollView.ScrollIndicatorInsets = contentInsets;

            // If activeField is hidden by keyboard, scroll it so it's visible
            var viewRectAboveKeyboard = new CGRect(topView.Frame.Location, new CGSize(topView.Frame.Width, topView.Frame.Size.Height - keyboardBounds.Size.Height));

            var activeFieldAbsoluteFrame = activeView.Superview.ConvertRectToView(activeView.Frame, topView);
            // activeFieldAbsoluteFrame is relative to topView so does not include any scrollView.ContentOffset
            activeFieldAbsoluteFrame.Y = activeFieldAbsoluteFrame.Y + topView.Frame.Y;

            // change width of control before checking because we only check vertically
            activeFieldAbsoluteFrame.Width = 10f;

            // Check if the activeField will be partially or entirely covered by the keyboard
            if (!viewRectAboveKeyboard.Contains(activeFieldAbsoluteFrame))
            {
                // Scroll to the activeField Y position + activeField.Height + current scrollView.ContentOffset.Y - the keyboard Height
                var scrollPoint = new CGPoint(0.0f, activeFieldAbsoluteFrame.Location.Y + activeFieldAbsoluteFrame.Height + scrollView.ContentOffset.Y - viewRectAboveKeyboard.Height);
                scrollView.SetContentOffset(scrollPoint, true);
            }
        }

        public static void ReactToKeyboardWillHideNotification(UIView notifiedView, UIView activeView, bool isCalledFromChildView, NSNotification notification)
        {
            if (activeView == null)
            {
                return;
            }

            var scrollView = activeView.FindSuperviewOfType (notifiedView, typeof(UIScrollView)) as UIScrollView;
            if (scrollView == null)
            {
                return;
            }

            // find the topmost scrollview (fix problem with RootElement)
            var nextSuperView = scrollView;
            while(nextSuperView != null)
            {
                scrollView = nextSuperView;
                nextSuperView = scrollView.FindSuperviewOfType(isCalledFromChildView ? notifiedView.Superview : notifiedView, typeof(UIScrollView)) as UIScrollView;
            }

            // Reset the content inset of the scrollView and animate using the current keyboard animation duration
            var animationDuration = UIKeyboard.AnimationDurationFromNotification(notification);
            var contentInsets = new UIEdgeInsets(0.0f, 0.0f, 0.0f, 0.0f);
            UIView.Animate(animationDuration, delegate{
                scrollView.ContentInset = contentInsets;
                scrollView.ScrollIndicatorInsets = contentInsets;
            });
        }
    }
}

