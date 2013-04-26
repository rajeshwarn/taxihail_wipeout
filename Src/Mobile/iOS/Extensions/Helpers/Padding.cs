using System;

namespace apcurium.MK.Booking.Mobile.Client.Extensions.Helpers
{
    public class Padding
    {
        float _left;
        float _right;
        float _bottom;
        float _top;
        
        public Padding (float value =0)
        {
            _left = value;
            _right = value;
            _top = value;
            _bottom = value;
        }
        public Padding (float horizontal, float vertical)
        {
            _left = horizontal;
            _right = horizontal;
            _top = vertical;
            _bottom = vertical;
        }
        public Padding (float left, float right,float top,float bottom)
        {
            _left = left;
            _right = right;
            _top = top;
            _bottom = bottom;
        }
        
        public System.Drawing.RectangleF ShrinkRectangle (System.Drawing.RectangleF rect)
        {
            rect.X = rect.X + _left;
            rect.Y = rect.Y + _top;
            
            rect.Width = rect.Width - _left - _right;
            rect.Height = rect.Height - _top - _bottom;
            return rect;
        }
    }
}

