using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.Views;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	[Register("apcurium.MK.Booking.Mobile.Client.Controls.Widgets.OrderStatusContactTaxiOverlay")]
	public class OrderStatusContactTaxiOverlay : MvxFrameControl
	{
		private ViewStates _animatedVisibility;
		private bool _isShown = false;

		public OrderStatusContactTaxiOverlay(Context context, IAttributeSet attrs) : base(Resource.Layout.SubView_ContactTaxiOverlay,context, attrs)
		{
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

		public void ShowIfNeeded()
		{
			if (_isShown)
			{
				return;
			}
			_isShown = true;

			((MarginLayoutParams)LayoutParameters).TopMargin = -Height;

			var animation = AnimationHelper.GetForYTranslation(this, 0);

			StartAnimation(animation);
		}

		public void HideIfNeeded()
		{
			if (!_isShown)
			{
				return;
			}
			_isShown = false;


			var animation = AnimationHelper.GetForYTranslation(this, -Height);

			animation.AnimationEnd += (sender, args) =>
			{
				//Visibility = ViewStates.Invisible;
			};

			StartAnimation(animation);
		}
	}
}