using System;
using Android.Views;
using Android.Views.Animations;

namespace apcurium.MK.Booking.Mobile.Client.Animations
{
    public class ResizeAnimation : Animation
    {
        public ResizeAnimation(View view, int offsetHeight, Boolean down)
        {
            View = view;
            OriginalHeight = 0;
            TargetHeight = 0;
            OffsetHeight = offsetHeight;
            Down = down;
        }

        public ResizeAnimation(View view, int originalHeight, int targetHeight, Boolean down)
        {
            View = view;
            OriginalHeight = originalHeight;
            TargetHeight = targetHeight;
            OffsetHeight = targetHeight - originalHeight;
            Down = down;
        }

        private int OriginalHeight { get; set; }
        private int TargetHeight { get; set; }
        private int OffsetHeight { get; set; }
        private View View { get; set; }
        private Boolean Down { get; set; }

        protected override void ApplyTransformation(float interpolatedTime, Transformation t)
        {
            // int newHeight;
            if (Down)
            {
                View.LayoutParameters.Height = (int) (OffsetHeight*interpolatedTime) + OriginalHeight;
            }

            else
            {
                View.LayoutParameters.Height = (int) (OffsetHeight*(1 - interpolatedTime)) + OriginalHeight;
            }
            View.RequestLayout();
        }

        public override Boolean WillChangeBounds()
        {
            return true;
        }
    }
}