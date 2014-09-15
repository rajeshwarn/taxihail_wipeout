using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using MonoTouch.CoreAnimation;
using MonoTouch.Foundation;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using apcurium.MK.Booking.Mobile.Client.Localization;

namespace apcurium.MK.Booking.Mobile.Client
{
	public static class UIViewExtensions
	{
		public static void SetPosition(this UIView view, float? x = null, float? y = null )
		{
			view.Frame = new RectangleF(x ?? view.Frame.X, y ?? view.Frame.Y, view.Frame.Width, view.Frame.Height);
		}

		public static void SetDimensions(this UIView view, float? width = null, float? height = null )
		{
			view.Frame = new RectangleF(view.Frame.X, view.Frame.Y, width ?? view.Frame.Width, height ?? view.Frame.Height);
		}

        public static void SetRoundedCorners(this UIView view, UIRectCorner corners, float radius)
        {
            var roundedRect = UIBezierPath.FromRoundedRect(new RectangleF(0, 0, view.Frame.Width, view.Frame.Height), corners, new SizeF(radius, radius));
            var maskLayer = new CAShapeLayer() { Frame = view.Bounds, Path = roundedRect.CGPath };
            view.Layer.Mask = maskLayer;
        }

        public static UIView FindFirstResponder (this UIView view)
        {
            if (view.IsFirstResponder)
            {
                return view;
            }

            foreach (var subView in view.Subviews) 
            {
                var firstResponder = subView.FindFirstResponder();
                if (firstResponder != null)
                    return firstResponder;
            }
            return null;
        }
        
        public static UIView FindSuperviewOfType(this UIView view, UIView stopAt, Type type)
        {
            if (view.Superview != null)
            {
                if (type.IsAssignableFrom(view.Superview.GetType()))
                {
                    return view.Superview;
                }
                
                if (view.Superview != stopAt)
                {
                    return view.Superview.FindSuperviewOfType(stopAt, type);
                }
            }
            
            return null;
        }

        public static T FindSuperviewOfType<T>(this UIView view, UIView stopAt) where T:UIView
        {
            if (view.Superview != null)
            {
                if (view.Superview is T)
                {
                    return (T)view.Superview;
                }
                
                if (view.Superview != stopAt)
                {
                    return view.Superview.FindSuperviewOfType<T>(stopAt);
                }
            }
            
            return null;
        }

        public static IEnumerable<T> FindSubviewsOfType<T>(this UIView view)
        {
            var allSubviews = view.Subviews.SelectMany(FindSubviewsRecursive);
            return allSubviews.OfType<T>();
        }

        private static IEnumerable<UIView> FindSubviewsRecursive(UIView view)
        {
            return view.Subviews.SelectMany(FindSubviewsRecursive).Concat(new [] { view });
        }

        public static void RemoveDelay(this UIView view)
        {
            foreach (var subView in view.Subviews) 
            {
                var scroll = subView as UIScrollView;
                if (scroll != null)
                {
                    scroll.DelaysContentTouches = false;
                }
            }
        }

        public static void StackSubViews (this UIView thisView)
        {
            UIViewHelper.StackSubViews (thisView.Subviews);
        }

		public static void StackSubViews (this UIView thisView, float topPadding, float spaceBetweenElements)
		{
			UIViewHelper.StackSubViews (thisView, topPadding, spaceBetweenElements);
		}

        public static void ResignFirstResponderOnSubviews (this UIView thisView)
        {
            thisView.ResignFirstResponder();                
            foreach (var view in thisView.Subviews) 
            {               
                view.ResignFirstResponderOnSubviews();              
            }
        }

        public static void AddToBottom (this UIView thisView, UIView toAdd)
        {
            if (thisView.Subviews.Any()) 
            {
                var bottom = thisView.Subviews.Max (v => v.Frame.Bottom);
                toAdd.Frame = toAdd.Frame.IncrementY (bottom);
            }
            thisView.AddSubview (toAdd);
        }
        
        public static void AddToViewBottom (this UIView thisView, UIView toAddTo)
        {
            toAddTo.AddToBottom (thisView);
        }
        
        public static void AddToView (this UIView thisView, UIView toAddTo)
        {
            toAddTo.Add (thisView);
        }
        
        public static void AddPadding(this UIView thisView, Padding margin)
        {
            thisView.Frame = margin.ShrinkRectangle (thisView.Frame);
        }

        public static void RoundCorners(this UIView thisButton,float radius=2, float borderThickness = 0, UIColor borderColor = null)
        {
            if (borderColor != null) 
            {
                thisButton.Layer.BorderColor = borderColor.CGColor;
            }

            if (borderThickness > 0) 
            {
                if (borderThickness == 1f && UIHelper.IsRetinaDisplay) 
                {
                    borderThickness = .5f;
                } 
                thisButton.Layer.BorderWidth = borderThickness;
            }
            
            thisButton.Layer.CornerRadius = radius;
        }

        public static SizeF GetSizeThatFits(this UIView view, string text, UIFont font)
        {
            if (UIHelper.IsOS7orHigher)
            {
                return new NSString (text)
                    .GetSizeUsingAttributes (new UIStringAttributes { Font = font });
            }
            else
            {
                return view.StringSize (text, font);
            }
        }

        public static void ShowCloseButtonOnKeyboard(this UITextView text, Action onClosePressed = null)
        {
            Action<object, EventArgs> onClose = (sender, e) =>
            {
                text.ResignFirstResponder ();
                if (onClosePressed != null)
                {
                    onClosePressed ();
                }
            };

            text.InputAccessoryView = CreateAccessoryViewWithCloseButton (onClose);
        }

        public static void ShowCloseButtonOnKeyboard(this UITextField text, Action onClosePressed = null)
        {
            Action<object, EventArgs> onClose = (sender, e) =>
            {
                text.ResignFirstResponder ();
                if (onClosePressed != null)
                {
                    onClosePressed ();
                }
            };

            text.InputAccessoryView = CreateAccessoryViewWithCloseButton (onClose);
        }

        private static UIView CreateAccessoryViewWithCloseButton(Action<object, EventArgs> onClose)
        {
            var accessoryView = new UIView { Frame = new RectangleF(0, 0, 320, 44), BackgroundColor = UIColor.FromRGB(251, 253, 253) };

            var closeButton = new FlatButton();
            var closeButtonText = Localize.GetValue ("OkButtonText");
            closeButton.SetTitle(closeButtonText, UIControlState.Normal);           
            closeButton.TranslatesAutoresizingMaskIntoConstraints = false;
            FlatButtonStyle.Green.ApplyTo(closeButton);
            accessoryView.AddSubview(closeButton);

            var widthOfText = closeButton.GetSizeThatFits (closeButtonText, closeButton.Font).Width;
            var totalTextPaddingInButton = 30f;

            closeButton.AddConstraints(new [] {
                NSLayoutConstraint.Create(closeButton, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1, 36f),
                NSLayoutConstraint.Create(closeButton, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1, widthOfText + totalTextPaddingInButton)
            });

            accessoryView.AddConstraints(new [] {
                NSLayoutConstraint.Create(closeButton, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, accessoryView, NSLayoutAttribute.Trailing, 1, -8f),
                NSLayoutConstraint.Create(closeButton, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, accessoryView, NSLayoutAttribute.CenterY, 1, 0),
            });

            closeButton.TouchUpInside += (sender, e) => onClose(sender, e);

            return accessoryView;
        }
	}
}

