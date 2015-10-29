using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Android.Widget;
using Android.Views.Animations;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	[Register("apcurium.MK.Booking.Mobile.Client.Controls.Widgets.OrderStatusView")]
	public class OrderStatusView : MvxFrameControl
	{
		private OrderStatusContactTaxiOverlay _contactTaxiOverlay;

		private bool _isShown;
		private ViewStates _animatedVisibility;
        private LinearLayout _statusLayout;
        private ImageView _progressImage;
        private FrameLayout _progressLayout;

        public OrderStatusView(Context context, IAttributeSet attrs) : base(Resource.Layout.SubView_BookingStatus, context, attrs)
        {
            this.DelayBind(() =>
                {
                    _contactTaxiOverlay = FindViewById<OrderStatusContactTaxiOverlay>(Resource.Id.ContactTaxiOverlay);
                    _statusLayout = FindViewById<LinearLayout>(Resource.Id.statusLayout);
                    _progressImage = FindViewById<ImageView>(Resource.Id.progressImage);
                    _progressLayout = FindViewById<FrameLayout>(Resource.Id.progressLayout);

                    var set = this.CreateBindingSet<OrderStatusView, BookingStatusViewModel>();

                    set.Bind(_contactTaxiOverlay)
                        .For("DataContext")
                        .To(vm => vm);

                    set.Bind(_contactTaxiOverlay)
                        .For(v => v.AnimatedVisibility)
                        .To(vm => vm.IsContactTaxiVisible)
                        .WithConversion("Visibility");

                    set.Bind()
                        .For(v => v.ShowAnimation)
                        .To(vm => vm.IsProgressVisible);

                    set.Bind(_contactTaxiOverlay)
                        .For(v => v.Visibility)
                        .To(vm => ((HomeViewModel)vm.Parent).CurrentViewState)
                        .WithConversion("HomeViewStateToVisibility", new[] { HomeViewModelState.BookingStatus });

                    set.Apply();
                });
        }

        private bool _showAnnimation;
        public bool ShowAnimation
        {
            get
            {
                return _showAnnimation;
            }
            set
            {
                if (_showAnnimation != value)
                {
                    _showAnnimation = value;
                    if (ShowAnimation)
                    {
                        _progressLayout.Visibility = ViewStates.Visible;
                        _statusLayout.SetBackgroundColor(Resources.GetColor(Resource.Color.transparent));
                        AnimateProgress(_progressImage);
                    }
                    else
                    {
                        _progressLayout.Visibility = ViewStates.Gone;
                        _statusLayout.Background = Resources.GetDrawable(Resource.Drawable.drop_shadow_opaque);
                        _progressImage.ClearAnimation();
                    }
                }
            }
        }

        private void AnimateProgress(View view)
        {
            view.SetX(-_statusLayout.Width);
            var animation = new TranslateAnimation(0, _statusLayout.Width, 0, 0)
                {
                    Duration = 4000,
                    Interpolator = new LinearInterpolator()
                };

            animation.AnimationEnd += (sender, e) => 
                {
                    var animationEnd = new TranslateAnimation(_statusLayout.Width, _statusLayout.Width * 2, 0, 0)
                        {
                            Duration = 4000,
                            Interpolator = new LinearInterpolator()
                        };
                    animationEnd.AnimationEnd += (sender1, e1) => AnimateProgress(view);
                    view.StartAnimation(animationEnd);
                };
            view.StartAnimation(animation);
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

			if (Animation != null)
			{
				Animation.Cancel();
			}

			((MarginLayoutParams)LayoutParameters).TopMargin = 0;

		}

		private void ShowIfNeeded()
		{
			if (_isShown || Height == 0)
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

			animation.AnimationEnd += (sender, args) =>
			{
				if (((BookingStatusViewModel) DataContext).IsContactTaxiVisible)
				{
					return;
				}

				var desiredHeight = -_contactTaxiOverlay.Height;

				if (((MarginLayoutParams)_contactTaxiOverlay.LayoutParameters).TopMargin != desiredHeight)
				{
					((MarginLayoutParams)_contactTaxiOverlay.LayoutParameters).TopMargin = desiredHeight;
				}
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

			var animation = AnimationHelper.GetForYTranslation(this, -Height);

			animation.AnimationEnd += (sender, args) =>
			{
				var contactAnimation = _contactTaxiOverlay.Animation;

				if (contactAnimation != null && contactAnimation.HasStarted)
				{
					contactAnimation.Cancel();
				}

				((MarginLayoutParams)_contactTaxiOverlay.LayoutParameters).TopMargin = OrderStatusContactTaxiOverlay.CONTACT_TAXI_HIDDEN_Y_OFFSET;
				
				//Ensures that the status view is hidden correctly.
				if (((MarginLayoutParams) LayoutParameters).TopMargin != -Height)
				{
					((MarginLayoutParams) LayoutParameters).TopMargin = -Height;
				}
			};

			StartAnimation(animation);
		}
	}
}