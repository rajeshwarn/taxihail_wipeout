using System;
using Android.Runtime;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Android.Views;
using Android.Content;
using Android.Util;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("apcurium.MK.Booking.Mobile.Client.Controls.Widgets.OrderStatusChangeDropOffOverlay")]
    public class OrderStatusChangeDropOffOverlay : MvxFrameControl
    {
        private ViewStates _animatedVisibility;
        private bool _isShown;

        public const int CHANGE_DROPOFF_HIDDEN_Y_OFFSET = -1000;

        public OrderStatusChangeDropOffOverlay(Context context, IAttributeSet attrs) : base(Resource.Layout.SubView_ChangeDropOffOverlay, context, attrs)
        {
            this.DelayBind(() => ((MarginLayoutParams)LayoutParameters).TopMargin = CHANGE_DROPOFF_HIDDEN_Y_OFFSET);
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

            if (((MarginLayoutParams)LayoutParameters).TopMargin != desiredHeight)
            {
                ((MarginLayoutParams)LayoutParameters).TopMargin = desiredHeight;
            }

            var animation = AnimationHelper.GetForYTranslation(this, 0);
            animation.AnimationEnd += (sender, e) => 
                {
                    // Update the map bounding box
                    ((BookingStatusViewModel)DataContext).MapCenter = ((BookingStatusViewModel)DataContext).MapCenter;
                };

            StartAnimation(animation);
        }

        private void HideIfNeeded()
        {
            if (!_isShown || Height == 0)
            {
                return;
            }
            _isShown = false;

            // If the Change DropOff Overlay already at -1000, then this animation is not needed.
            if (((MarginLayoutParams)LayoutParameters).TopMargin == CHANGE_DROPOFF_HIDDEN_Y_OFFSET)
            {
                return;
            }

            var animation = AnimationHelper.GetForYTranslation(this, -Height);

            StartAnimation(animation);
        }
    }
}