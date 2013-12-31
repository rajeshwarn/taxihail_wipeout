using Android.Views;
using Android.Views.Animations;
using Android.Widget;

namespace apcurium.MK.Booking.Mobile.Client.Animations
{
    public sealed class SlideDownAnimation : Animation
    {
        private readonly int _mChange;
        private readonly int _mStart;
        private readonly View _mView;

        public SlideDownAnimation(View v, int marginStart, int marginEnd, IInterpolator i)
        {
            _mStart = marginStart;
            _mView = v;

            _mChange = marginEnd - _mStart;
            Interpolator = i;
        }


        protected override void ApplyTransformation(float interpolatedTime, Transformation t)
        {
            float change = _mChange*interpolatedTime;

            SetTopMargin(_mView, (int) -(_mStart + change));

            base.ApplyTransformation(interpolatedTime, t);
        }

        private void SetTopMargin(View v, int m)
        {
            var p = (RelativeLayout.LayoutParams) v.LayoutParameters;
            p.TopMargin = -m;
            v.LayoutParameters = p;
            v.Invalidate();
        }
    }
}