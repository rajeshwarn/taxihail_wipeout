using Android.Content;
using Android.Util;
using Android.Widget;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.Views;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public class OrderOptions : MvxFrameControl, IChangePresentation
    {
        private AddressTextBox ViewPickup;
        private AddressTextBox ViewDestination;
        private VehicleTypeAndEstimateControl ViewVehicleType;

        public Button BigInvisibleButton { get; set; }

        public OrderOptions(Context context, IAttributeSet attrs) : base (Resource.Layout.SubView_OrderOptions, context, attrs)
        {
            this.DelayBind(() => 
            {
                ViewPickup = Content.FindViewById<AddressTextBox>(Resource.Id.viewPickup);
                ViewDestination = Content.FindViewById<AddressTextBox>(Resource.Id.viewDestination);
                ViewVehicleType = Content.FindViewById<VehicleTypeAndEstimateControl>(Resource.Id.viewEstimate);

                ViewDestination.IsDestination = true;
                ViewPickup.IsDestination = false;

                ViewPickup.SetInvisibleButton(BigInvisibleButton);
                ViewDestination.SetInvisibleButton(BigInvisibleButton);

                InitializeBinding();
            });
        }

        private OrderOptionsViewModel ViewModel { get { return (OrderOptionsViewModel)DataContext; } }

        void InitializeBinding()
		{
			ViewPickup.AddressUpdated = (streetNumber, fullAddress) => {
				ViewModel.PickupAddress.StreetNumber = streetNumber;
				ViewModel.PickupAddress.FullAddress = fullAddress;

				ViewModel.SetAddress.Execute (ViewModel.PickupAddress);
			};

			ViewDestination.AddressUpdated = (streetNumber, fullAddress) => {
				ViewModel.DestinationAddress.StreetNumber = streetNumber;
				ViewModel.DestinationAddress.FullAddress = fullAddress;

				ViewModel.SetAddress.Execute (ViewModel.DestinationAddress);
			};

			ViewVehicleType.VehicleSelected = (vehicleType) => {
				ViewModel.SetVehicleType.Execute (vehicleType);
			};

			var set = this.CreateBindingSet<OrderOptions, OrderOptionsViewModel> ();

			set.Bind (ViewPickup)
                .For (v => v.IsReadOnly)
                .To (vm => vm.ShowDestination);
			set.Bind (ViewPickup)
                .For (v => v.IsLoadingAddress)
                .To (vm => vm.IsLoadingAddress);

			set.Bind (ViewPickup.AddressTextView)
                .To (vm => vm.PickupAddress.DisplayAddress);

			set.Bind (ViewDestination)
                .For (v => v.Visibility)
                .To (vm => vm.ShowDestination)
                .WithConversion ("Visibility");
			set.Bind (ViewDestination)
                .For (v => v.IsLoadingAddress)
                .To (vm => vm.IsLoadingAddress);

			set.Bind (ViewDestination.AddressTextView)
                .To (vm => vm.DestinationAddress.DisplayAddress);

			set.Bind (ViewVehicleType)
                .For (v => v.EstimatedFare)
                .To (vm => vm.EstimatedFare);
			set.Bind (ViewVehicleType)
                .For (v => v.Visibility)
				.To (vm => vm.VehicleAndEstimateBoxIsVisible)
                .WithConversion ("Visibility");

			set.Bind (ViewVehicleType)
				.For (v => v.ShowEstimate)
				.To (vm => vm.ShowEstimate);

			set.Bind (ViewVehicleType)
				.For (v => v.Vehicles)
				.To (vm => vm.VehicleTypes);

			set.Bind (ViewVehicleType)
				.For (v => v.SelectedVehicle)
				.To (vm => vm.SelectedVehicleType);

			set.Bind (ViewPickup)
                .For ("AddressClicked")
                .To (vm => vm.ShowSearchAddress);

			set.Bind (ViewDestination)
                .For ("AddressClicked")
                .To (vm => vm.ShowSearchAddress);

			set.Bind (ViewVehicleType)
				.For (v => v.ShowEta)
				.To (vm => vm.ShowEta);

			set.Bind (ViewVehicleType)
				.For (v => v.ShowVehicleSelection)
				.To (vm => vm.ShowVehicleSelection);

			set.Bind (ViewVehicleType)
				.For (v => v.Eta)
				.To (vm => vm.FormattedEta);

			set.Apply ();
		}

        private void ChangeState(HomeViewModelPresentationHint hint)
        {
            if (hint.State == HomeViewModelState.Review)
            {
                ViewPickup.IsReadOnly = true;
                ViewDestination.IsReadOnly = true;
				ViewVehicleType.IsReadOnly = true;
            }
            else if(hint.State == HomeViewModelState.PickDate)
            {
                ViewPickup.IsReadOnly = true;
                ViewDestination.IsReadOnly = true;
				ViewVehicleType.IsReadOnly = true;
            }
            else if(hint.State == HomeViewModelState.Initial)
            {
                ViewPickup.IsReadOnly = ViewModel.ShowDestination;
                ViewDestination.IsReadOnly = false;
				ViewVehicleType.IsReadOnly = false;
            }
        }

        public void ChangePresentation(ChangePresentationHint hint)
        {
            if (hint is HomeViewModelPresentationHint)
            {
                ChangeState((HomeViewModelPresentationHint)hint);
            }
        }
    }
}
      

