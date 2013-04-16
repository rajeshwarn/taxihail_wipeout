using System;
using Android.Views.Animations;
using Android.Graphics;
using Android.Views;
using Android.Widget;


namespace apcurium.MK.Booking.Mobile.Client.Animations
{
    public class SlideDownAnimation: Animation
    {
        private int _mEnd;
        private int _mStart;
        private int _mChange;
        private View _mView;
        
        public SlideDownAnimation (View v, int marginStart, int marginEnd, IInterpolator i)
        {
            _mStart = marginStart;
            _mEnd = marginEnd;
            _mView = v;
            
            _mChange = _mEnd - _mStart;
            Interpolator = i;
        }
        
        
        
        protected override void ApplyTransformation (float interpolatedTime, Transformation t)
        {
            float change = _mChange * interpolatedTime;

            SetTopMargin(_mView, (int) -(_mStart + change));
            
            base.ApplyTransformation(interpolatedTime, t);
        }

        private void SetTopMargin(View v, int m)
        {         
            var p = (RelativeLayout.LayoutParams)v.LayoutParameters;
            p.TopMargin = -m;
            v.LayoutParameters = p;
            v.Invalidate();           
            
            
        }

    }
    
    
}

