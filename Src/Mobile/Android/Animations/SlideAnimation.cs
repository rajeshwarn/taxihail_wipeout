using System;
using Android.Views.Animations;
using Android.Graphics;
using Android.Views;

namespace SlidingPanel
{
	public class SlideAnimation : Animation
	{
		private int _mEnd;
		private int _mStart;
		private int _mChange;
		private View _mView;

		public SlideAnimation (View v, int marginStart, int marginEnd, IInterpolator i)
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

			_mView.ScrollTo((int) -(_mStart + change), 0);

			base.ApplyTransformation(interpolatedTime, t);
		}

	}


}

