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

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("apcurium.MK.Booking.Mobile.Client.Controls.Widgets.OrderOptions")]
    public class OrderOptions : MvxFrameControl
    {
        private AddressTextBox _viewPickup;
        private AddressTextBox _viewDestination;
        private VehicleTypeAndEstimateControl _viewVehicleType;

	    private bool _isShown = true;

        public Button BigInvisibleButton { get; set; }

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

                _viewVehicleType.Visibility = ViewStates.Gone;

                InitializeBinding();

            });
        }

        private OrderOptionsViewModel ViewModel { get { return (OrderOptionsViewModel)DataContext; } }


	    public void HideIfNeeded()
	    {
		    if (!_isShown)
		    {
			    return;
		    }

		    _isShown = false;

			StartAnimation(AnimationHelper.GetForYTranslation(this, -Height));
	    }
	    public void ShowIfNeeded()
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
				.To(vm => vm.AddressSelectionMode)
				.WithConversion("EnumToBool", AddressSelectionMode.PickupSelection);

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
				.To(vm => vm.AddressSelectionMode)
				.WithConversion("EnumToBool", AddressSelectionMode.DropoffSelection);

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
				.For (v => v.ShowEta)
				.To (vm => vm.ShowEta);

			set.Bind (_viewVehicleType)
				.For (v => v.ShowVehicleSelection)
				.To (vm => vm.ShowVehicleSelection);

			set.Bind (_viewVehicleType)
				.For (v => v.Eta)
				.To (vm => vm.FormattedEta);

			set.Apply ();
		}
    }
}
      

