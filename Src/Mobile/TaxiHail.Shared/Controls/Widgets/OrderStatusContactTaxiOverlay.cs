using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.Views;
using System;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	[Register("apcurium.MK.Booking.Mobile.Client.Controls.Widgets.OrderStatusContactTaxiOverlay")]
	public class OrderStatusContactTaxiOverlay : MvxFrameControl
	{
		private ViewStates _animatedVisibility;
		private bool _isShown;
        public Action ActionOnAnimationEnd { get; set;}

		public const int CONTACT_TAXI_HIDDEN_Y_OFFSET = -1000;

		public OrderStatusContactTaxiOverlay(Context context, IAttributeSet attrs) : base(Resource.Layout.SubView_ContactTaxiOverlay,context, attrs)
		{
			this.DelayBind(() => ((MarginLayoutParams)LayoutParameters).TopMargin = CONTACT_TAXI_HIDDEN_Y_OFFSET);
		}

		public ViewStates AnimatedVisibility
		{
			get { return _animatedVisibility; }
			set
			{
				_animatedVisibility = value;
				if (value == ViewStates.Visible)
				{
					ShowIfNeeded();
					return;
				}
				HideIfNeeded();
			}
		}

		public void ShowWithoutAnimation()
		{
			_isShown = true;

			((MarginLayoutParams) LayoutParameters).TopMargin = 0;
		}

		private void ShowIfNeeded()
		{
			if (_isShown || Height == 0)
			{
				return;
			}
			_isShown = true;

			var desiredHeight = -Height;

			if (((MarginLayoutParams) LayoutParameters).TopMargin != desiredHeight)
			{
				((MarginLayoutParams)LayoutParameters).TopMargin = desiredHeight;
			}

            var animation = AnimationHelper.GetForYTranslation(this, 0);

            if (ActionOnAnimationEnd != null)
            {
                animation.AnimationEnd += (sender, e) =>
                {
                    ActionOnAnimationEnd();
                };
            }
            else
            {
                // If no Change Drop Off overlay, update the map bounding box
                animation.AnimationEnd += (sender, e) => 
                {
                    ((BookingStatusViewModel)DataContext).MapCenter = ((BookingStatusViewModel)DataContext).MapCenter;
                };
        }

			StartAnimation(animation);
		}

		private void HideIfNeeded()
		{
			if (!_isShown || Height == 0)
			{
				return;
			}
			_isShown = false;

			// If the Contact Taxi Overlay already at -1000, then this animation is not needed.
			if (((MarginLayoutParams)LayoutParameters).TopMargin == CONTACT_TAXI_HIDDEN_Y_OFFSET)
			{
				return;
			}

            var animation = AnimationHelper.GetForYTranslation(this, -Height);
            animation.AnimationEnd += (sender, e) => 
                {
                    ((MarginLayoutParams)LayoutParameters).TopMargin = -((MarginLayoutParams)LayoutParameters).Height;
                };

			StartAnimation(animation);
		}
	}
}