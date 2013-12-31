using Android.Views;
using Android.Views.Animations;

namespace apcurium.MK.Booking.Mobile.Client.Animations
{
    public sealed class SlideAnimation : Animation
    {
        private readonly int _mChange;
        private readonly int _mStart;
        private readonly View _mView;

        public SlideAnimation(View v, int marginStart, int marginEnd, IInterpolator i)
        {
            _mStart = marginStart;
            _mView = v;

            _mChange = marginEnd - _mStart;
            Interpolator = i;
        }
        protected override void ApplyTransformation(float interpolatedTime, Transformation t)
        {
            var change = _mChange*interpolatedTime;

            _mView.ScrollTo((int) -(_mStart + change), 0);

            base.ApplyTransformation(interpolatedTime, t);
        }
    }
}