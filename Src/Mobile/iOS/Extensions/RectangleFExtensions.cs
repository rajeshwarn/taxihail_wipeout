using CoreGraphics;
using System;

namespace apcurium.MK.Booking.Mobile.Client.Extensions
{
    public static class RectangleFExtensions
    {
        public static CGRect IncrementX(this CGRect thisRectangleF, nfloat delta)
        {
            thisRectangleF.X += delta;
            return thisRectangleF;
        }

        public static CGRect IncrementY(this CGRect thisRectangleF, nfloat delta)
        {
            thisRectangleF.Y += delta;
            return thisRectangleF;
        }

        public static CGRect SetX(this CGRect thisRectangleF, nfloat value)
        {
            thisRectangleF.X = value;
            return thisRectangleF;
        }

        public static CGRect SetY(this CGRect thisRectangleF, nfloat value)
        {
            thisRectangleF.Y = value;
            return thisRectangleF;
        }

        public static CGRect SetBottom(this CGRect thisRectangleF, nfloat value)
        {
            thisRectangleF.Height = value - thisRectangleF.Y;
            return thisRectangleF;
        }

        public static CGRect SetRight(this CGRect thisRectangleF, nfloat value)
        {
            thisRectangleF.X = value - thisRectangleF.Width;
            return thisRectangleF;
        }

        public static CGRect SetHorizontalCenter(this CGRect thisRectangleF, nfloat value)
        {
            thisRectangleF.X = value - (thisRectangleF.Width/2);
            return thisRectangleF;
        }

        public static CGRect SetVerticalCenter(this CGRect thisRectangleF, nfloat value)
        {
            thisRectangleF.Y = value - (thisRectangleF.Height/2);
            return thisRectangleF;
        }

        public static CGRect Grow(this CGRect rect, nfloat numberOfPixels)
        {
            return Shrink(rect, -numberOfPixels);
        }

        public static CGRect Shrink(this CGRect rect, nfloat numberOfPixels)
        {
            rect.Y += numberOfPixels;
            rect.X += numberOfPixels;
            rect.Width -= (numberOfPixels * 2);
            rect.Height -= (numberOfPixels * 2);
            return rect;
        }

        public static CGRect Copy(this CGRect thisRectangleF)
        {
            return new CGRect(thisRectangleF.X, thisRectangleF.Y, thisRectangleF.Width, thisRectangleF.Height);
        }

        public static CGRect IncrementHeight(this CGRect thisRectangleF, nfloat delta)
        {
            thisRectangleF.Height += delta;
            return thisRectangleF;
        }

        public static CGRect IncrementWidth(this CGRect thisRectangleF, nfloat delta)
        {
            thisRectangleF.Width += delta;
            return thisRectangleF;
        }

        public static CGRect SetWidth(this CGRect thisRectangleF, nfloat value)
        {
            thisRectangleF.Width = value;
            return thisRectangleF;
        }       

        public static CGRect SetHeight(this CGRect thisRectangleF, nfloat value)
        {
            thisRectangleF.Height = value;
            return thisRectangleF;
        }
    }
}

