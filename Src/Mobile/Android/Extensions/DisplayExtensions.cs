using Android.Graphics;
using Android.Views;

namespace apcurium.MK.Booking.Mobile.Client.Extensions
{
    public static class DisplayExtensions
    {
        private const int ReferenceWidth = 640;
        private const int ReferenceHeight = 1136;

        public static float GetHorizontalScale(this Display screenSize)
        {
            return screenSize.Width/(float) ReferenceWidth;
        }

        public static float GetVerticalScale(this Display screenSize)
        {
            return screenSize.Height/(float) ReferenceHeight;
        }


        public static PointF GetScale(this Display screenSize)
        {
            return new PointF(screenSize.GetHorizontalScale(), screenSize.GetVerticalScale());
        }
    }
}