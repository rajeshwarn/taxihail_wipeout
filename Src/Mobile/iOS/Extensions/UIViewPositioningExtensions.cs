using MonoTouch.UIKit;
using System.Drawing;

namespace apcurium.MK.Booking.Mobile.Client.Extensions
{
    public static class UIViewPositioningExtensions
    {
        public static UIView IncrementX(this UIView thisView, float delta)
        {
            thisView.Frame = thisView.Frame.IncrementX(delta);
            return thisView;
        }
        public static UIView IncrementY(this UIView thisView, float delta)
        {
            
            thisView.Frame = thisView.Frame.IncrementY(delta);
            return thisView;
        }
        public static UIView SetX(this UIView thisView, float value)
        {            
            thisView.Frame = thisView.Frame.SetX(value);
            return thisView;
        }
        public static UIView SetY(this UIView thisView, float value)
        {            
            thisView.Frame = thisView.Frame.SetY(value);
            return thisView;
        }
        public static UIView SetBottom(this UIView thisView, float value)
        {            
            thisView.Frame = thisView.Frame.SetBottom(value);
            return thisView;
        }
        
        public static UIView IncrementHeight(this UIView thisView, float delta)
        {            
            thisView.Frame = thisView.Frame.IncrementHeight(delta);
            return thisView;
        }
        public static UIView IncrementWidth(this UIView thisView, float delta)
        {            
            thisView.Frame = thisView.Frame.IncrementWidth(delta);
            return thisView;
        }
        public static UIView SetWidth(this UIView thisView, float value)
        {            
            thisView.Frame = thisView.Frame.SetWidth(value);
            return thisView;
        }       
        public static UIView SetHeight(this UIView thisView, float value)
        {
            thisView.Frame = thisView.Frame.SetHeight(value);
            return thisView;
        }

        
        public static UIView SetFrame(this UIView thisView, RectangleF frame)
        {
            return thisView.SetFrame(frame.X,frame.Y,frame.Width,frame.Height);
        }

        public static UIView SetFrame(this UIView thisView, float x, float y, float width, float height)
        {
            thisView.Frame = new RectangleF(x,y,width,height);
            return thisView;
        }
    }
}

