using System;
using Android.Views.Animations;
using Android.Views;
using Android.Widget;
using Android.App;
using Android.OS;

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
            var isAndroid23 = (int)Build.VERSION.SdkInt <= 10;
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

            if(!isAndroid23)
            {
                // after 2.3, the views are always visible, just moved away from the screen
                // otherwise it causes a flicker
                view.Visibility = ViewStates.Visible;
            }

            var willBeVisible = (desiredX.HasValue && desiredX.Value >= 0 && desiredX.Value < Application.Context.Resources.DisplayMetrics.WidthPixels)
                                || (desiredY.HasValue && desiredY.Value >= 0 && desiredY.Value < Application.Context.Resources.DisplayMetrics.HeightPixels);

            if (isAndroid23)
            {
                animation.AnimationStart += (sender, e) => 
                {
                    if(isAndroid23)
                    {
                        // show the view if it's visible at the end
                        if (willBeVisible)
                        {
                            if (view.Visibility != ViewStates.Visible)
                            {
                                view.Visibility = ViewStates.Visible;
                            }
                        }
                    }
                };
            }

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

                if(isAndroid23)
                {
                    // hide the view if it's not visible at the end
                    if (!willBeVisible)
                    {
                        if (view.Visibility != ViewStates.Gone)
                        {
                            view.Visibility = ViewStates.Gone;
                        }
                    }
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

