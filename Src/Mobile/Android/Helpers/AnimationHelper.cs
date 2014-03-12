using Android.App;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;

namespace apcurium.MK.Booking.Mobile.Client.Helpers
{
    public static class AnimationHelper
    {
        public static TranslateAnimation GetForXTranslation(View view, float desiredX)
        {
            return PlatformHelper.IsAndroid23 
                ? GetAnimationFor23(view, desiredX, null)
                : GetAnimationFor(view, desiredX, null);
        }

        public static TranslateAnimation GetForYTranslation(View view, float desiredY)
        {
            return PlatformHelper.IsAndroid23
                ? GetAnimationFor23(view, null, desiredY)
                : GetAnimationFor(view, null, desiredY);
        }

        private static TranslateAnimation GetAnimationFor23(View view, float? desiredX, float? desiredY)
        {
            var layoutParams = (FrameLayout.LayoutParams)view.LayoutParameters;

            var animation = new TranslateAnimation(
                Dimension.Absolute, 0, Dimension.Absolute, 0, 
                Dimension.Absolute, 0, Dimension.Absolute, 0)
            {
                Duration = 1,
                Interpolator = new DecelerateInterpolator()
            };

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

            var willBeVisible = (desiredX.HasValue && desiredX.Value >= 0 && desiredX.Value < Application.Context.Resources.DisplayMetrics.WidthPixels)
                || (desiredY.HasValue && desiredY.Value >= 0 && desiredY.Value < Application.Context.Resources.DisplayMetrics.HeightPixels);

            if (willBeVisible)
            {
                if (view.Visibility != ViewStates.Visible)
                {
                    view.Visibility = ViewStates.Visible;
                }
            }
            else
            {
                if (view.Visibility != ViewStates.Gone)
                {
                    view.Visibility = ViewStates.Gone;
                }
            }

            if (deltaX == 0 && deltaY == 0)
            {
                // view is already at the correct position
                return animation;
            }

            animation.AnimationEnd += (sender, e) => 
            {
                if(desiredX.HasValue)
                {
                    layoutParams.LeftMargin = (int)desiredX.Value;
                }

                if(desiredY.HasValue)
                {
                    layoutParams.TopMargin = (int)desiredY.Value;
                }

                layoutParams.Gravity = GravityFlags.Top;
                view.LayoutParameters = layoutParams;

                view.RequestLayout();

                // this animation stops the view from flickering at the end of the animation
                var nullAnimation = new TranslateAnimation(0,0,0,0){ Duration = 1 };
                view.StartAnimation(nullAnimation);
            };

            return animation;
        }

        private static TranslateAnimation GetAnimationFor(View view, float? desiredX, float? desiredY)
        {
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

            // after 2.3, the views are always visible, just moved away from the screen
            // otherwise it causes a flicker
            view.Visibility = ViewStates.Visible;

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

