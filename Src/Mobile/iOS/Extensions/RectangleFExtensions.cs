using CoreGraphics;

namespace apcurium.MK.Booking.Mobile.Client.Extensions
{
    public static class RectangleFExtensions
    {
        public static CGRect IncrementX(this CGRect thisRectangleF, float delta)
        {
            thisRectangleF.X += delta;
            return thisRectangleF;
        }

        public static CGRect IncrementY(this CGRect thisRectangleF, float delta)
        {
            thisRectangleF.Y += delta;
            return thisRectangleF;
        }

        public static CGRect SetX(this CGRect thisRectangleF, float value)
        {
            thisRectangleF.X = value;
            return thisRectangleF;
        }

        public static CGRect SetY(this CGRect thisRectangleF, float value)
        {
            thisRectangleF.Y = value;
            return thisRectangleF;
        }

        public static CGRect SetBottom(this CGRect thisRectangleF, float value)
        {
            thisRectangleF.Height = value - thisRectangleF.Y;
            return thisRectangleF;
        }

        public static CGRect SetRight(this CGRect thisRectangleF, float value)
        {
            thisRectangleF.X = value - thisRectangleF.Width;
            return thisRectangleF;
        }

        public static CGRect SetHorizontalCenter(this CGRect thisRectangleF, float value)
        {
            thisRectangleF.X = value - (thisRectangleF.Width/2);
            return thisRectangleF;
        }

        public static CGRect SetVerticalCenter(this CGRect thisRectangleF, float value)
        {
            thisRectangleF.Y = value - (thisRectangleF.Height/2);
            return thisRectangleF;
        }

        public static CGRect Grow(this CGRect rect, float numberOfPixels)
        {
            return Shrink(rect, -numberOfPixels);
        }

        public static CGRect Shrink(this CGRect rect, float numberOfPixels)
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

        public static CGRect IncrementHeight(this CGRect thisRectangleF, float delta)
        {
            thisRectangleF.Height += delta;
            return thisRectangleF;
        }

        public static CGRect IncrementWidth(this CGRect thisRectangleF, float delta)
        {
            thisRectangleF.Width += delta;
            return thisRectangleF;
        }

        public static CGRect SetWidth(this CGRect thisRectangleF, float value)
        {
            thisRectangleF.Width = value;
            return thisRectangleF;
        }       

        public static CGRect SetHeight(this CGRect thisRectangleF, float value)
        {
            thisRectangleF.Height = value;
            return thisRectangleF;
        }
    }
}

