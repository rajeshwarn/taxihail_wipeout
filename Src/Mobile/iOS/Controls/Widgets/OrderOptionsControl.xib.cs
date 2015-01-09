using System;
using Cirrious.MvvmCross.Binding.BindingContext;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;
using System.Linq;
using apcurium.MK.Booking.Mobile.PresentationHints;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.Data;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public partial class OrderOptionsControl : BaseBindableChildView<OrderOptionsViewModel>, IChangePresentation
    {
        private NSLayoutConstraint _heightConstraint;

        public OrderOptionsControl (IntPtr handle) : base(handle)
        {
        }

        private void Initialize()
        {
            _heightConstraint = NSLayoutConstraint.Create(this, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1.0f, 44.0f);
            AddConstraint(_heightConstraint);

            BackgroundColor = UIColor.Clear;
            viewPickup.BackgroundColor = UIColor.Clear;
            viewDestination.BackgroundColor = UIColor.Clear;

            viewPickup.IsDestination = false;
            viewDestination.IsDestination = true;
        }

        private void InitializeBinding()
        {
            viewPickup.AddressUpdated = streetNumber => {
                ViewModel.PickupAddress.ChangeStreetNumber(streetNumber);
                ViewModel.SetAddress.ExecuteIfPossible(ViewModel.PickupAddress);
            };

            viewDestination.AddressUpdated = streetNumber => {
                ViewModel.DestinationAddress.ChangeStreetNumber(streetNumber);
                ViewModel.SetAddress.ExecuteIfPossible(ViewModel.DestinationAddress);
            };

            viewVehicleType.VehicleSelected = vehicleType => ViewModel.SetVehicleType.ExecuteIfPossible(vehicleType);

            var set = this.CreateBindingSet<OrderOptionsControl, OrderOptionsViewModel>();

			set.Bind(viewPickup)
				.For(v => v.IsSelected)
				.To(vm => vm.AddressSelectionMode)
				.WithConversion("EnumToBool", AddressSelectionMode.PickupSelection);
            set.Bind(viewPickup)
                .For(v => v.IsLoadingAddress)
                .To(vm => vm.IsLoadingAddress);
            set.Bind(viewPickup.AddressTextView)
                .To(vm => vm.PickupAddress.DisplayAddress);

            set.Bind(viewDestination)
                .For(v => v.Hidden)
                .To(vm => vm.ShowDestination)
                .WithConversion("BoolInverter");
			set.Bind(viewDestination)
				.For(v => v.IsSelected)
				.To(vm => vm.AddressSelectionMode)
				.WithConversion("EnumToBool", AddressSelectionMode.DropoffSelection);
            set.Bind(viewDestination)
                .For(v => v.IsLoadingAddress)
                .To(vm => vm.IsLoadingAddress);
            set.Bind(viewDestination.AddressTextView)
                .To(vm => vm.DestinationAddress.DisplayAddress);

            set.Bind(viewVehicleType)
                .For(v => v.EstimatedFare)
                .To(vm => vm.EstimatedFare);
            set.Bind(viewVehicleType)
                .For(v => v.Hidden)
				.To(vm => vm.VehicleAndEstimateBoxIsVisible)
                .WithConversion("BoolInverter");
            set.Bind(viewVehicleType)
                .For(v => v.ShowEstimate)
				.To(vm => vm.ShowEstimate);
			set.Bind(viewVehicleType)
				.For(v => v.ShowEta)
                .To(vm => vm.ShowEta);
			set.Bind(viewVehicleType)
				.For(v => v.ShowVehicleSelection)
				.To(vm => vm.ShowVehicleSelection);
            set.Bind (viewVehicleType)
                .For (v => v.Vehicles)
                .To (vm => vm.VehicleTypes);
            set.Bind (viewVehicleType)
                .For (v => v.SelectedVehicle)
                .To (vm => vm.SelectedVehicleType);
			set.Bind (viewVehicleType)
				.For (v => v.Eta)
				.To (vm => vm.FormattedEta);

			set.Bind (viewPickup)
                .For ("AddressClicked")
				.To (vm => vm.ShowPickUpSearchAddress);

            set.Bind(viewDestination)
                .For("AddressClicked")
                .To(vm => vm.ShowDestinationSearchAddress);

            set.Apply();
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            var nib = UINib.FromName ("OrderOptionsControl", null);
            var view = (UIView)nib.Instantiate(this, null)[0];
            AddSubview(view);

            Initialize();

            this.DelayBind (() => {
                InitializeBinding();
            });
        }

        public void Resize()
        {
            _heightConstraint.Constant = Subviews[0].Subviews.Where(x => !x.Hidden).Sum(x => x.Frame.Height);
            SetNeedsDisplay();
        }

        private void ChangeState(HomeViewModelPresentationHint hint)
        {
            switch (hint.State)
            {
				case HomeViewModelState.Review:
                    viewPickup.IsSelected = false;
					viewPickup.UserInputDisabled = true;
                    viewDestination.IsSelected = false;
					viewDestination.UserInputDisabled = true;
                    viewVehicleType.IsReadOnly = true;
                    break;
                case HomeViewModelState.PickDate:
                    viewPickup.IsSelected = false;
					viewPickup.UserInputDisabled = true;
                    viewDestination.IsSelected = false;
					viewDestination.UserInputDisabled = true;
                    viewVehicleType.IsReadOnly = true;
                    break;
                case HomeViewModelState.Initial:
					viewPickup.IsSelected = ViewModel.AddressSelectionMode == AddressSelectionMode.PickupSelection;
                    viewPickup.UserInputDisabled = false;
                    viewDestination.IsSelected = ViewModel.AddressSelectionMode == AddressSelectionMode.DropoffSelection;
					viewDestination.UserInputDisabled = false;
                    viewVehicleType.IsReadOnly = false;
                    break;
            }
        }

        void IChangePresentation.ChangePresentation(ChangePresentationHint hint)
        {
            if (hint is HomeViewModelPresentationHint)
            {
                ChangeState((HomeViewModelPresentationHint)hint);
            }
        }
    }
}

