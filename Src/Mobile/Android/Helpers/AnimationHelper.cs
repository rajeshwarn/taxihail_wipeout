using System;
using Android.Views.Animations;
using Android.Views;
using Android.Widget;
using Android.App;

namespace apcurium.MK.Booking.Mobile.Client.Helpers
{
    public static class AnimationHelper
    {
        public static TranslateAnimation GetForXTranslation(View view, float desiredX)
        {
            return GetAnimationFor(view, desiredX, null);
        }

        public static TranslateAnimation GetForYTranslation(View view, float desiredY)
        {
            return GetAnimationFor(view, null, desiredY);
        }

        private static TranslateAnimation GetAnimationFor(View view, float? desiredX, float? desiredY)
        {
            view.ClearAnimation();

            var layoutParams = (LinearLayout.MarginLayoutParams)view.LayoutParameters;

            int deltaX = 0;
            if (desiredX.HasValue)
            {
                deltaX = (int)desiredX.Value - layoutParams.LeftMargin;
            }

            int deltaY = 0;
            if (desiredY.HasValue)
            {
                deltaY = (int)desiredY.Value - layoutParams.TopMargin;
            }

            var animation = new TranslateAnimation(
                Dimension.Absolute, 0, Dimension.Absolute, deltaX, 
                Dimension.Absolute, 0, Dimension.Absolute, deltaY)
            {
                Duration = 600,
                Interpolator = new DecelerateInterpolator()
            };

            animation.AnimationEnd += (sender, e) => 
            {
                if(desiredX.HasValue && deltaX != 0)
                {
                    layoutParams.LeftMargin = (int)desiredX.Value;
                }

                if(desiredY.HasValue && deltaY != 0)
                {
                    layoutParams.TopMargin = (int)desiredY.Value;
                }

                view.LayoutParameters = layoutParams;

                // this animation stops the view from flickering at the end of the animation
                var nullAnimation = new TranslateAnimation(0,0,0,0){ Duration = 1 };
                view.StartAnimation(nullAnimation);
            };

            return animation;
        }
    }
}

