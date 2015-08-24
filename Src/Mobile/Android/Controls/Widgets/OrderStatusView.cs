using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.ViewModels;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.Views;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	[Register("apcurium.MK.Booking.Mobile.Client.Controls.Widgets.OrderStatusView")]
	public class OrderStatusView : MvxFrameControl
	{
		private OrderStatusContactTaxiOverlay _contactTaxiOverlay;

		private bool _isShown;
		private ViewStates _animatedVisibility;


		public OrderStatusView(Context context, IAttributeSet attrs) : base(Resource.Layout.SubView_BookingStatus, context, attrs)
		{
			this.DelayBind(() =>
			{
				_contactTaxiOverlay = FindViewById<OrderStatusContactTaxiOverlay>(Resource.Id.ContactTaxiOverlay);

				_contactTaxiOverlay.Visibility = ViewStates.Invisible;
				((MarginLayoutParams)_contactTaxiOverlay.LayoutParameters).TopMargin = -1000;
				
				var set = this.CreateBindingSet<OrderStatusView, BookingStatusViewModel>();

				set.Bind(_contactTaxiOverlay)
					.For("DataContext")
					.To(vm => vm);

				set.Bind(_contactTaxiOverlay)
					.For(v => v.AnimatedVisibility)
					.To(vm => vm.IsContactTaxiVisible)
					.WithConversion("Visibility");

				set.Apply();
			});
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


			var animation = AnimationHelper.GetForYTranslation(this, 0);

			animation.AnimationStart += (sender, args) =>
			{
				if (((MarginLayoutParams) LayoutParameters).TopMargin != -Height)
				{
					((MarginLayoutParams) LayoutParameters).TopMargin = -Height;
				}
			};

			StartAnimation(animation);
		}

		public void HideIfNeeded()
		{
			if (!_isShown)
			{
				return;
			}

			_isShown = false;

			StartAnimation(AnimationHelper.GetForYTranslation(this, -Height));
		}
	}
}