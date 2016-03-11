using System;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.Views;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("apcurium.MK.Booking.Mobile.Client.Controls.Widgets.OrderOptions")]
    public class OrderOptions : MvxFrameControl
    {
        private AddressTextBox _viewPickup;
        private AddressTextBox _viewDestination;
        private VehicleTypeAndEstimateControl _viewVehicleType;
		private LinearLayout _etaContainer;
		private LinearLayout _etaBadge;
		private ImageView _baseRateExpandImage;
		private BaseRateControl _baseRateControl;
		private VehicleTypeControl _etaBadgeImage;
		private AutoResizeTextView _etaLabelInVehicleSelection;

		public event EventHandler<EventArgs> SizeChanged;

	    private bool _isShown = true;
	    private ViewStates _animatedVisibility;

	    public Button BigInvisibleButton { get; set; }

		/// Added to prevent the ETA from becoming visible in during booking status in certain scenarios.
		private const int HiddenHeightOffset = -50;

	    private const int HiddenHeightNoAnim = -500;

        public OrderOptions(Context context, IAttributeSet attrs) : base (Resource.Layout.SubView_OrderOptions, context, attrs)
        {
            this.DelayBind(() => 
            {
                _viewPickup = Content.FindViewById<AddressTextBox>(Resource.Id.viewPickup);
                _viewDestination = Content.FindViewById<AddressTextBox>(Resource.Id.viewDestination);
                _viewVehicleType = Content.FindViewById<VehicleTypeAndEstimateControl>(Resource.Id.viewEstimate);

                _viewDestination.IsDestination = true;
                _viewPickup.IsDestination = false;

                _viewPickup.SetInvisibleButton(BigInvisibleButton);
                _viewDestination.SetInvisibleButton(BigInvisibleButton);

				_etaContainer = (LinearLayout)Content.FindViewById(Resource.Id.EtaContainer);
				_etaBadge = (LinearLayout)Content.FindViewById(Resource.Id.EtaBadge);
				_baseRateExpandImage = (ImageView)Content.FindViewById(Resource.Id.BaseRateExpandImage);
				_etaLabelInVehicleSelection = (AutoResizeTextView)Content.FindViewById(Resource.Id.EtaLabelInVehicleSelection);
				_baseRateControl = (BaseRateControl)Content.FindViewById(Resource.Id.BaseRate);

                _etaBadgeImage = new VehicleTypeControl (base.Context, (VehicleType)null);
                _etaBadge.AddView (_etaBadgeImage);

                _etaContainer.SetBackgroundColorWithRoundedCorners(0, 0, 3, 3, Resources.GetColor(Resource.Color.company_color));
				_etaContainer.Click += (sender, e) => ToggleBaseRate();

                _viewVehicleType.Visibility = ViewStates.Gone;
                InitializeBinding();
            });
        }

        private void ToggleBaseRate()
        {
            _baseRateControl.ToggleBaseRate();

            var toggleResource = _baseRateControl.BaseRateToggled 
                ? Resource.Drawable.up_arrow_light
                : Resource.Drawable.down_arrow_light;

            _baseRateExpandImage.SetImageDrawable(Resources.GetDrawable(toggleResource));
        }

		protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
		{
			base.OnSizeChanged(w, h, oldw, oldh);

			if (SizeChanged != null)
			{
				SizeChanged(this, EventArgs.Empty);
			}
		}

        private OrderOptionsViewModel ViewModel { get { return (OrderOptionsViewModel)DataContext; } }

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

	    public void HideWithoutAnimation()
	    {
		    _isShown = false;

			((MarginLayoutParams)LayoutParameters).TopMargin = HiddenHeightNoAnim;
	    }

		private void HideIfNeeded()
	    {
			if (!_isShown || Height == 0)
		    {
			    return;
		    }

		    _isShown = false;

		    var translationOffset = -Height + HiddenHeightOffset;

			StartAnimation(AnimationHelper.GetForYTranslation(this, translationOffset));
	    }

		private void ShowIfNeeded()
	    {
			if (_isShown || Height == 0)
			{
				return;
			}

			_isShown = true;

			var translationOffset = -Height + HiddenHeightOffset;

			((MarginLayoutParams)LayoutParameters).TopMargin = translationOffset;

			StartAnimation(AnimationHelper.GetForYTranslation(this, 0));
	    }

        public bool UserInputDisabled
        {
            get { return !_etaContainer.Clickable; }
            set
            {
                _etaContainer.Clickable = !value;
                if (!_etaContainer.Clickable && _baseRateControl.BaseRateToggled)
                {
                    // close the rate box
                    ToggleBaseRate();
                }
                _baseRateExpandImage.Visibility = UserInputDisabled ? ViewStates.Invisible : ViewStates.Visible;
            }
        }

        private void InitializeBinding()
		{
			_viewPickup.AddressUpdated = streetNumber => {
				ViewModel.PickupAddress.ChangeStreetNumber(streetNumber);
				ViewModel.SetAddress.Execute (ViewModel.PickupAddress);
			};

			_viewDestination.AddressUpdated = streetNumber => {
				ViewModel.DestinationAddress.ChangeStreetNumber(streetNumber);
				ViewModel.SetAddress.Execute (ViewModel.DestinationAddress);
			};

			_viewVehicleType.VehicleSelected = vehicleType => ViewModel.SetVehicleType.Execute (vehicleType);

			var set = this.CreateBindingSet<OrderOptions, OrderOptionsViewModel> ();

	        set.Bind(_viewPickup)
		        .For(v => v.IsSelected)
		        .To(vm => vm.IsPickupSelected);

			set.Bind (_viewPickup)
                .For (v => v.IsLoadingAddress)
                .To (vm => vm.IsLoadingAddress);

			set.Bind (_viewPickup)
                .For (v => v.CurrentAddress)
                .To (vm => vm.PickupAddress);

			set.Bind(_viewPickup)
				.For(v => v.UserInputDisabled)
				.To(vm => vm.PickupInputDisabled);

			set.Bind(_viewDestination)
				.For(v => v.UserInputDisabled)
				.To(vm => vm.DestinationInputDisabled);

	        set.Bind(_viewDestination)
		        .For(v => v.IsSelected)
		        .To(vm => vm.IsDestinationSelected);

			set.Bind (_viewDestination)
                .For (v => v.Visibility)
                .To (vm => vm.ShowDestination)
                .WithConversion ("Visibility");

			set.Bind (_viewDestination)
                .For (v => v.IsLoadingAddress)
                .To (vm => vm.IsLoadingAddress);

			set.Bind (_viewDestination)
                .For (v => v.CurrentAddress)
                .To (vm => vm.DestinationAddress);

			set.Bind (_viewVehicleType)
                .For (v => v.EstimatedFare)
                .To (vm => vm.EstimatedFare);

			set.Bind (_viewVehicleType)
                .For (v => v.Visibility)
				.To (vm => vm.VehicleAndEstimateBoxIsVisible)
                .WithConversion ("Visibility");

			set.Bind (_viewVehicleType)
				.For (v => v.ShowEstimate)
				.To (vm => vm.ShowEstimate);

			set.Bind (_viewVehicleType)
				.For (v => v.Vehicles)
				.To (vm => vm.VehicleTypes);

            set.Bind (_viewVehicleType)
                .For (v => v.VehicleRepresentations)
                .To (vm => vm.VehicleRepresentations);

			set.Bind (_viewVehicleType)
				.For (v => v.SelectedVehicle)
				.To (vm => vm.SelectedVehicleType);

			set.Bind(_viewVehicleType)
				.For(v => v.IsReadOnly)
				.To(vm => vm.VehicleTypeInputDisabled);

			set.Bind (_viewPickup)
                .For ("AddressClicked")
                .To (vm => vm.ShowPickUpSearchAddress);

			set.Bind (_viewDestination)
                .For ("AddressClicked")
                .To (vm => vm.ShowDestinationSearchAddress);

			set.Bind (_viewVehicleType)
				.For (v => v.ShowVehicleSelection)
				.To (vm => vm.ShowVehicleSelection);

			set.Bind(_viewVehicleType)
				.For (v => v.Eta)
				.To (vm => vm.FormattedEta);

	        set.Bind(_viewVehicleType)
		        .For(v => v.ShowEta)
				.To(vm => vm.ShowEtaInEstimate);

            set.Bind(_viewVehicleType)
                .For(v => v.GroupVehiclesByServiceType)
                .To(vm => vm.GroupVehiclesByServiceType);

			set.Bind(_etaContainer)
				.For(v => v.Visibility)
				.To(vm => vm.ShowEta)
				.WithConversion("Visibility");

			set.Bind(_baseRateControl)
				.For(v => v.Visibility)
                .To(vm => vm.DisplayBaseRateInfo)
				.WithConversion("Visibility");

			set.Bind(_baseRateControl)
                .For(v => v.VehicleType)
                .To(vm => vm.SelectedVehicleType)
				.OneWay();

			set.Bind(this)
                .For(v => v.UserInputDisabled)
                .To(vm => vm.CanShowRateBox)
                .WithConversion("BoolInverter")
				.OneWay();

			set.Bind(_etaBadge)
				.For(v => v.Visibility)
                .To(vm => vm.DisplayBaseRateInfo)
				.WithConversion("InvertedVisibility");

			set.Bind(_baseRateExpandImage)
				.For(v => v.Visibility)
                .To(vm => vm.DisplayBaseRateInfo)
				.WithConversion("Visibility");

			set.Bind(_etaLabelInVehicleSelection)
				.For(v => v.Text)
				.To(vm => vm.FormattedEta);

			set.Bind(_etaLabelInVehicleSelection)
				.For(v => v.Visibility)
				.To(vm => vm.ShowEta)
				.WithConversion("Visibility");

            set.Bind (_etaBadgeImage)
                .For (v => v.Vehicle)
                .To (vm => vm.SelectedVehicleType);

            set.Apply ();
		}
    }
}
      

