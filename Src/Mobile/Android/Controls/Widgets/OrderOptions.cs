using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Data;
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
		private VehicleTypeControl _etaBadgeImage;
		private AutoResizeTextView _etaLabelInVehicleSelection;

	    private bool _isShown = true;
	    private ViewStates _animatedVisibility;

	    public Button BigInvisibleButton { get; set; }

		/// Added to prevent the ETA from becoming visible in during booking status in certain scenarios.
		private const int HIDDEN_HIGHT_OFFSET = -50;

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
				_etaLabelInVehicleSelection = (AutoResizeTextView)Content.FindViewById(Resource.Id.EtaLabelInVehicleSelection);
                _etaBadgeImage = new VehicleTypeControl (base.Context, (VehicleType)null);
                _etaBadge.AddView (_etaBadgeImage);

                _etaContainer.SetBackgroundColorWithRoundedCorners(0, 0, 3, 3, Resources.GetColor(Resource.Color.company_color));

                _viewVehicleType.Visibility = ViewStates.Gone;
                InitializeBinding();

            });
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

		private void HideIfNeeded()
	    {
		    if (!_isShown)
		    {
			    return;
		    }

		    _isShown = false;

		    var translationOffset = -Height + HIDDEN_HIGHT_OFFSET;

			StartAnimation(AnimationHelper.GetForYTranslation(this, translationOffset));
	    }
		private void ShowIfNeeded()
	    {
			if (_isShown)
			{
				return;
			}

			_isShown = true;

			StartAnimation(AnimationHelper.GetForYTranslation(this, 0));
	    }

        void InitializeBinding()
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

			set.Bind (_viewPickup.AddressTextView)
                .To (vm => vm.PickupAddress.DisplayAddress);

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

			set.Bind (_viewDestination.AddressTextView)
                .To (vm => vm.DestinationAddress.DisplayAddress);

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
      

