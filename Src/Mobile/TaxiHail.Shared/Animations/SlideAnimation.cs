using Android.Views;
using Android.Views.Animations;

namespace apcurium.MK.Booking.Mobile.Client.Animations
{
    public sealed class SlideAnimation : Animation
    {
        private readonly int _mChangeX;
        private readonly int _mStartX;
        private readonly View _mView;

        public SlideAnimation(View v, int marginStartX, int marginEndX)
        {
            _mStartX = marginStartX;
            _mView = v;

            _mChangeX = marginEndX - _mStartX;
            Interpolator = new DecelerateInterpolator(0.9f);
        }

        protected override void ApplyTransformation(float interpolatedTime, Transformation t)
        {
            var changeX = _mChangeX * interpolatedTime;

            _mView.ScrollTo((int) -(_mStartX + changeX), 0);

            base.ApplyTransformation(interpolatedTime, t);
        }
    }
}