using System;
using System.Drawing;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using System.Linq;

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

        public static UIView FindFirstResponder (this UIView view)
        {
            if (view.IsFirstResponder)
            {
                return view;
            }
            foreach (var subView in view.Subviews) {
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
                    return view.Superview.FindSuperviewOfType(stopAt, type);
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
                    return view.Superview.FindSuperviewOfType<T>(stopAt);
            }
            
            return null;
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
            foreach (var view in thisView.Subviews) {               
                view.ResignFirstResponderOnSubviews();              
            }
        }
        
        
        public static void AddToBottom (this UIView thisView, UIView toAdd)
        {
            if (thisView.Subviews.Any()) {
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
            if (borderColor != null) {
                thisButton.Layer.BorderColor = borderColor.CGColor;
            }
            if (borderThickness > 0) {
// ReSharper disable once CompareOfFloatsByEqualityOperator
                if (borderThickness == 1f && UIHelper.IsRetinaDisplay) {
                    borderThickness = .5f;
                } 
                thisButton.Layer.BorderWidth = borderThickness;
            }
            
            thisButton.Layer.CornerRadius = radius;
        }

	}
}

