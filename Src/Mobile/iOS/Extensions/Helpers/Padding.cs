using System.Drawing;

namespace apcurium.MK.Booking.Mobile.Client.Extensions.Helpers
{
    public class Padding
    {
		public float Left { get; set; }
		public float Right { get; set; }
		public float Bottom { get; set; }
		public float Top { get; set; }
        
        public Padding (float value =0)
        {
            Left = value;
            Right = value;
            Top = value;
            Bottom = value;
        }
        public Padding (float horizontal, float vertical)
        {
            Left = horizontal;
            Right = horizontal;
            Top = vertical;
            Bottom = vertical;
        }
        public Padding (float left, float right,float top,float bottom)
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }
        
        public RectangleF ShrinkRectangle (RectangleF rect)
        {
            rect.X = rect.X + Left;
            rect.Y = rect.Y + Top;
            
            rect.Width = rect.Width - Left - Right;
            rect.Height = rect.Height - Top - Bottom;
            return rect;
        }
    }
}

