using UIKit;
using CoreGraphics;
using System;

namespace apcurium.MK.Booking.Mobile.Client.Extensions
{
    public static class UIViewPositioningExtensions
    {
        public static UIView IncrementX(this UIView thisView, nfloat delta)
        {
            thisView.Frame = thisView.Frame.IncrementX(delta);
            return thisView;
        }

        public static UIView IncrementY(this UIView thisView, nfloat delta)
        {
            thisView.Frame = thisView.Frame.IncrementY(delta);
            return thisView;
        }

        public static UIView SetX(this UIView thisView, nfloat value)
        {            
            thisView.Frame = thisView.Frame.SetX(value);
            return thisView;
        }

        public static UIView SetY(this UIView thisView, nfloat value)
        {            
            thisView.Frame = thisView.Frame.SetY(value);
            return thisView;
        }

        public static UIView SetBottom(this UIView thisView, nfloat value)
        {            
            thisView.Frame = thisView.Frame.SetBottom(value);
            return thisView;
        }

        public static UIView SetRight(this UIView thisView, nfloat value)
        {
            thisView.Frame = thisView.Frame.SetRight(value);
            return thisView;
        }

        public static UIView SetHorizontalCenter(this UIView thisView, nfloat value)
        {
            thisView.Frame = thisView.Frame.SetHorizontalCenter(value);
            return thisView;
        }

        public static UIView SetVerticalCenter(this UIView thisView, nfloat value)
        {
            thisView.Frame = thisView.Frame.SetVerticalCenter(value);
            return thisView;
        }
        
        public static UIView IncrementHeight(this UIView thisView, nfloat delta)
        {            
            thisView.Frame = thisView.Frame.IncrementHeight(delta);
            return thisView;
        }

        public static UIView IncrementWidth(this UIView thisView, nfloat delta)
        {            
            thisView.Frame = thisView.Frame.IncrementWidth(delta);
            return thisView;
        }

        public static UIView SetWidth(this UIView thisView, nfloat value)
        {            
            thisView.Frame = thisView.Frame.SetWidth(value);
            return thisView;
        }   

        public static UIView SetHeight(this UIView thisView, nfloat value)
        {
            thisView.Frame = thisView.Frame.SetHeight(value);
            return thisView;
        }

        public static UIView SetFrame(this UIView thisView, CGRect frame)
        {
            return thisView.SetFrame(frame.X, frame.Y, frame.Width, frame.Height);
        }

        public static UIView SetFrame(this UIView thisView, nfloat x, nfloat y, nfloat width, nfloat height)
        {
            thisView.Frame = new CGRect(x,y,width,height);
            return thisView;
        }
    }
}

