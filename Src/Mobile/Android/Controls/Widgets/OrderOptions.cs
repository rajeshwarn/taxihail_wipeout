using Android.Content;
using Android.Util;
using Android.Widget;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Android.Views;
using apcurium.MK.Booking.Mobile.Data;
using Android.Runtime;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("apcurium.MK.Booking.Mobile.Client.Controls.Widgets.OrderOptions")]
    public class OrderOptions : MvxFrameControl, IChangePresentation
    {
        private AddressTextBox _viewPickup;
		private AddressTextBox _viewDestination;
        private VehicleTypeAndEstimateControl _viewVehicleType;
		private LinearLayout _etaContainer;
		private LinearLayout _etaBadge;
		private VehicleTypeControl _etaBadgeImage;
		private AutoResizeTextView _etaLabelInVehicleSelection;

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

			set.Bind (_viewPickup)
                .For (v => v.IsSelected)
				.To (vm => vm.AddressSelectionMode)
				.WithConversion("EnumToBool", AddressSelectionMode.PickupSelection);
			set.Bind (_viewPickup)
                .For (v => v.IsLoadingAddress)
                .To (vm => vm.IsLoadingAddress);

			set.Bind (_viewPickup.AddressTextView)
                .To (vm => vm.PickupAddress.DisplayAddress);

			set.Bind (_viewDestination)
				.For (v => v.IsSelected)
				.To (vm => vm.AddressSelectionMode)
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

			set.Bind(_etaContainer)
				.For(v => v.Visibility)
				.To(vm => vm.ShowEta)
				.WithConversion("Visibility");

			set.Bind(_etaLabelInVehicleSelection)
				.For(v => v.Text)
				.To(vm => vm.FormattedEta);

            set.Bind (_etaBadgeImage)
                .For (v => v.Vehicle)
                .To (vm => vm.SelectedVehicleType);

            set.Apply ();
		}

        private void ChangeState(HomeViewModelPresentationHint hint)
        {
            switch (hint.State)
            {
                case HomeViewModelState.Review:
                    _viewPickup.IsSelected = false;
                    _viewPickup.UserInputDisabled = true;
                    _viewDestination.IsSelected = false;
                    _viewDestination.UserInputDisabled = true;
                    _viewVehicleType.IsReadOnly = true;
                    break;
                case HomeViewModelState.PickDate:
                    _viewPickup.IsSelected = false;
                    _viewPickup.UserInputDisabled = true;
                    _viewDestination.IsSelected = false;
                    _viewDestination.UserInputDisabled = true;
                    _viewVehicleType.IsReadOnly = true;
                    break;
                case HomeViewModelState.Initial:
                    _viewPickup.IsSelected = ViewModel.AddressSelectionMode == AddressSelectionMode.PickupSelection;
                    _viewPickup.UserInputDisabled = false;
                    _viewDestination.IsSelected = ViewModel.AddressSelectionMode == AddressSelectionMode.DropoffSelection;
                    _viewDestination.UserInputDisabled = false;
                    _viewVehicleType.IsReadOnly = false;
                    break;
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
      

