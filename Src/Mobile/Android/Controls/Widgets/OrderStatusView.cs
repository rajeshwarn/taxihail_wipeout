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
using System;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	[Register("apcurium.MK.Booking.Mobile.Client.Controls.Widgets.OrderStatusView")]
	public class OrderStatusView : MvxFrameControl
	{
        private OrderStatusContactTaxiOverlay _contactTaxiOverlay;
        private OrderStatusChangeDropOffOverlay _changeDropOffOverlay;

		private bool _isShown;
        private ViewStates _animatedVisibility;
        private ViewStates _contactTaxiAnimatedVisibility;
        private ViewStates _changeDropOffAnimatedVisibility;
        private LinearLayout _statusLayout;
        private RelativeLayout _topViewLayout;
        private ImageView _progressImage;
        private FrameLayout _progressLayout;

        public OrderStatusView(Context context, IAttributeSet attrs) : base(Resource.Layout.SubView_BookingStatus, context, attrs)
        {
            this.DelayBind(() =>
                {
                    _contactTaxiOverlay = FindViewById<OrderStatusContactTaxiOverlay>(Resource.Id.ContactTaxiOverlay);
                    _changeDropOffOverlay = FindViewById<OrderStatusChangeDropOffOverlay>(Resource.Id.ChangeDropOffOverlay);
                    _statusLayout = FindViewById<LinearLayout>(Resource.Id.statusLayout);
                    _topViewLayout = FindViewById<RelativeLayout>(Resource.Id.topViewLayout);
                    _progressImage = FindViewById<ImageView>(Resource.Id.progressImage);
                    _progressLayout = FindViewById<FrameLayout>(Resource.Id.progressLayout);

                    var set = this.CreateBindingSet<OrderStatusView, BookingStatusViewModel>();

                    set.Bind(_contactTaxiOverlay)
                        .For("DataContext")
                        .To(vm => vm);

                    set.Bind()
                        .For(v => v.ContactTaxiAnimatedVisibility)
                        .To(vm => vm.IsContactTaxiVisible)
                        .WithConversion("Visibility");

                    set.Bind(_changeDropOffOverlay)
                        .For("DataContext")
                        .To(vm => vm);

                    set.Bind()
                        .For(v => v.ChangeDropOffAnimatedVisibility)
                        .To(vm => vm.IsChangeDropOffVisible)
                        .WithConversion("Visibility");

                    set.Bind(_changeDropOffOverlay)
                        .For("Click")
                        .To(vm => vm.AddOrRemoveDropOffCommand);

                    set.Bind()
                        .For(v => v.ShowAnimation)
                        .To(vm => vm.IsProgressVisible);

                    set.Bind(_contactTaxiOverlay)
                        .For(v => v.Visibility)
                        .To(vm => ((HomeViewModel)vm.Parent).CurrentViewState)
                        .WithConversion("HomeViewStateToVisibility", new[] { HomeViewModelState.BookingStatus });

                    set.Bind(_changeDropOffOverlay)
                        .For(v => v.Visibility)
                        .To(vm => ((HomeViewModel)vm.Parent).CurrentViewState)
                        .WithConversion("HomeViewStateToVisibility", new[] { HomeViewModelState.BookingStatus });

                    set.Apply();
                });
        }

        public BookingStatusViewModel BookingStatusViewModel
        {
            get
            {
                return (BookingStatusViewModel)DataContext;
            }
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);

            if (_statusLayout.Height != 0 && BookingStatusViewModel.IsProgressVisible)
            {
                var layoutParams = (RelativeLayout.LayoutParams)_progressLayout.LayoutParameters;

                if (layoutParams.Height != _statusLayout.Height)
                {
                    layoutParams.Height = _statusLayout.Height;
                    _progressLayout.LayoutParameters = layoutParams;
                }
            }
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

        public ViewStates ContactTaxiAnimatedVisibility
        {
            get { return _contactTaxiAnimatedVisibility; }
            set
            {
                _contactTaxiAnimatedVisibility = value;

                var layoutParamsChangeDropOff = (RelativeLayout.LayoutParams)_changeDropOffOverlay.LayoutParameters; 
                var layoutParamsTopView = (FrameLayout.LayoutParams)_topViewLayout.LayoutParameters;
                
                if (value == ViewStates.Visible)
                {
                    var desiredHeight = _statusLayout.Height + _contactTaxiOverlay.Height + (BookingStatusViewModel.IsChangeDropOffVisible ? _changeDropOffOverlay.Height : 0);

                    if (layoutParamsTopView.Height != desiredHeight && _contactTaxiOverlay.LayoutParameters.Height != 0 && _statusLayout.Height != 0)
                    {
                        layoutParamsTopView.Height = desiredHeight;
                        _topViewLayout.LayoutParameters = layoutParamsTopView;

                        _contactTaxiOverlay.ActionOnAnimationEnd = new Action(() => 
                            {
                                layoutParamsChangeDropOff.RemoveRule(LayoutRules.Below);
                                layoutParamsChangeDropOff.AddRule(LayoutRules.Below, Resource.Id.ContactTaxiOverlay);

                                _changeDropOffOverlay.LayoutParameters = layoutParamsChangeDropOff;

                                ChangeDropOffAnimatedVisibility = BookingStatusViewModel.IsChangeDropOffVisible ? ViewStates.Visible : ViewStates.Gone;

                                if (_changeDropOffOverlay.LayoutParameters.Height != 0 && _statusLayout.Height != 0)
                                {
                                    layoutParamsTopView.Height = desiredHeight;
                                    _topViewLayout.LayoutParameters = layoutParamsTopView;
                                    _changeDropOffOverlay.AnimatedVisibility = ChangeDropOffAnimatedVisibility;
                                }
                            });
                    }
                }
                else
                {
                    layoutParamsChangeDropOff.RemoveRule(LayoutRules.Below);
                    layoutParamsChangeDropOff.AddRule(LayoutRules.Below, Resource.Id.statusLayout);
                    _changeDropOffOverlay.LayoutParameters = layoutParamsChangeDropOff;
                }
                _contactTaxiOverlay.AnimatedVisibility = _contactTaxiAnimatedVisibility;
            }
        }

        public ViewStates ChangeDropOffAnimatedVisibility
        {
            get { return _changeDropOffAnimatedVisibility; }
            set
            {
                _changeDropOffAnimatedVisibility = value;

                var layoutParamsTopView = (FrameLayout.LayoutParams)_topViewLayout.LayoutParameters;

                if (value == ViewStates.Visible && ContactTaxiAnimatedVisibility != ViewStates.Visible)
                {
                    var desiredHeight = _statusLayout.Height + _changeDropOffOverlay.LayoutParameters.Height;

                    if (_statusLayout.Height != desiredHeight && _changeDropOffOverlay.LayoutParameters.Height != 0 && _statusLayout.Height != 0)
                    {
                        layoutParamsTopView.Height = desiredHeight;
                        _topViewLayout.LayoutParameters = layoutParamsTopView;
                        _changeDropOffOverlay.AnimatedVisibility = ChangeDropOffAnimatedVisibility;
                    }
                }
                else if (value != ViewStates.Visible)
                {
                    _changeDropOffOverlay.AnimatedVisibility = _changeDropOffAnimatedVisibility;
                }
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

            if (((MarginLayoutParams) LayoutParameters).TopMargin != -Height)
            {
                ((MarginLayoutParams) LayoutParameters).TopMargin = -Height;
            }

            var animation = AnimationHelper.GetForYTranslation(this, 0);

            animation.AnimationEnd += (sender, args) =>
                {
                    if (BookingStatusViewModel.IsContactTaxiVisible)
                    {
                        var desiredHeight = -_contactTaxiOverlay.Height;

                        if (((MarginLayoutParams)_contactTaxiOverlay.LayoutParameters).TopMargin != desiredHeight)
                        {
                            ((MarginLayoutParams)_contactTaxiOverlay.LayoutParameters).TopMargin = desiredHeight;
                        }

                        var animContactTaxi = AnimationHelper.GetForYTranslation(_contactTaxiOverlay, 0);

                        animContactTaxi.AnimationEnd += (s, a) =>
                        {
                                if (BookingStatusViewModel.IsChangeDropOffVisible)
                                {
                                    desiredHeight = 0;
        
                                    if (((MarginLayoutParams)_changeDropOffOverlay.LayoutParameters).TopMargin != desiredHeight)
                                    {
                                        ((MarginLayoutParams)_changeDropOffOverlay.LayoutParameters).TopMargin = desiredHeight;
                                    }
        
                                    var animDropOffSelection = AnimationHelper.GetForYTranslation(_changeDropOffOverlay, 0);
        
                                    StartAnimation(animDropOffSelection);
                                }
                        };

                        StartAnimation(animContactTaxi);
                        return;
                    }

                    if (BookingStatusViewModel.IsChangeDropOffVisible)
                    {
                        var desiredHeight = -_changeDropOffOverlay.Height;

                        if (((MarginLayoutParams)_changeDropOffOverlay.LayoutParameters).TopMargin != desiredHeight)
                        {
                            ((MarginLayoutParams)_changeDropOffOverlay.LayoutParameters).TopMargin = desiredHeight;
                        }

                        var animDropOffSelection = AnimationHelper.GetForYTranslation(_changeDropOffOverlay, 0);

                        StartAnimation(animDropOffSelection);
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

                var changeDropOffAnimation = _changeDropOffOverlay.Animation;

                if (changeDropOffAnimation != null && changeDropOffAnimation.HasStarted)
                {
                    changeDropOffAnimation.Cancel();
                }

                ((MarginLayoutParams)_contactTaxiOverlay.LayoutParameters).TopMargin = OrderStatusContactTaxiOverlay.CONTACT_TAXI_HIDDEN_Y_OFFSET;
                ((MarginLayoutParams)_changeDropOffOverlay.LayoutParameters).TopMargin = OrderStatusChangeDropOffOverlay.CHANGE_DROPOFF_HIDDEN_Y_OFFSET;

                var layoutParamsTopView = (FrameLayout.LayoutParams)_topViewLayout.LayoutParameters;
                layoutParamsTopView.Height = LayoutParams.WrapContent;
                _topViewLayout.LayoutParameters = layoutParamsTopView;
				
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