using System;
using System.Drawing;

namespace apcurium.MK.Booking.Mobile.Client.Extensions
{
    public static class RectangleFExtensions
    {
        public static RectangleF IncrementX(this RectangleF thisRectangleF, float delta)
        {
            thisRectangleF.X += delta;
            return thisRectangleF;
        }
        public static RectangleF IncrementY(this RectangleF thisRectangleF, float delta)
        {
            thisRectangleF.Y += delta;
            return thisRectangleF;
        }
        public static RectangleF SetX(this RectangleF thisRectangleF, float value)
        {
            thisRectangleF.X = value;
            return thisRectangleF;
        }
        public static RectangleF SetY(this RectangleF thisRectangleF, float value)
        {
            thisRectangleF.Y = value;
            return thisRectangleF;
        }
        
        public static RectangleF Shrink(this RectangleF thisRectangleF, float numberOfPixels)
        {
            thisRectangleF.Y += numberOfPixels;
            thisRectangleF.X += numberOfPixels;
            thisRectangleF.Width -= (numberOfPixels*2);
            thisRectangleF.Height -=(numberOfPixels*2);
            return thisRectangleF;
        }
        
        public static RectangleF IncrementHeight(this RectangleF thisRectangleF, float delta)
        {
            thisRectangleF.Height += delta;
            return thisRectangleF;
        }
        public static RectangleF IncrementWidth(this RectangleF thisRectangleF, float delta)
        {
            thisRectangleF.Width += delta;
            return thisRectangleF;
        }
        public static RectangleF SetWidth(this RectangleF thisRectangleF, float value)
        {
            thisRectangleF.Width = value;
            return thisRectangleF;
        }       
        public static RectangleF SetHeight(this RectangleF thisRectangleF, float value)
        {
            thisRectangleF.Height = value;
            return thisRectangleF;
        }
    }
}

