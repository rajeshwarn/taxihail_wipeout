using System;
using Cirrious.MvvmCross.Binding.BindingContext;
using UIKit;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;
using System.Linq;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.Client.Style;
using apcurium.MK.Booking.Mobile.Client.Localization;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public partial class OrderOptionsControl : BaseBindableChildView<OrderOptionsViewModel>
    {
        public NSLayoutConstraint HeightConstraint;

        public OrderOptionsControl (IntPtr handle) : base(handle)
        {
        }

        private void Initialize()
        {
        	TranslatesAutoresizingMaskIntoConstraints = false;

            HeightConstraint = NSLayoutConstraint.Create(this, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1.0f, 44.0f);
			AddConstraint(HeightConstraint);

            viewPickup.BackgroundColor = UIColor.Clear;
            viewDestination.BackgroundColor = UIColor.Clear;
            viewVehicleType.BackgroundColor = UIColor.Clear;
            viewEta.BackgroundColor = Theme.CompanyColor;

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

            viewVehicleType.VehicleSelected = vehicleSelectionModel => ViewModel.SetVehicleType.ExecuteIfPossible(vehicleSelectionModel);

            var set = this.CreateBindingSet<OrderOptionsControl, OrderOptionsViewModel>();

			set.Bind(viewPickup)
				.For(v => v.IsSelected)
				.To(vm => vm.IsPickupSelected);
            
            set.Bind(viewPickup)
                .For(v => v.IsLoadingAddress)
                .To(vm => vm.IsLoadingAddress);

            set.Bind(viewPickup.AddressTextView)
                .To(vm => vm.PickupAddress.DisplayAddress);

            set.Bind(viewPickup)
                .For(v => v.UserInputDisabled)
                .To(vm => vm.PickupInputDisabled);

            set.Bind(viewPickup.AddressButton)
                .For(ve => ve.AccessibilityLabel)
                .To(vm => vm.PickupAddress.DisplayAddress);
                
            viewPickup.AddressButton.AccessibilityHint = Localize.GetValue("PickupTextPlaceholder");

            set.Bind(viewDestination)
                .For(v => v.Hidden)
                .To(vm => vm.ShowDestination)
                .WithConversion("BoolInverter");
            
			set.Bind(viewDestination)
				.For(v => v.IsSelected)
				.To(vm => vm.IsDestinationSelected);
            
            set.Bind(viewDestination)
                .For(v => v.IsLoadingAddress)
                .To(vm => vm.IsLoadingAddress);
            
            set.Bind(viewDestination.AddressTextView)
                .To(vm => vm.DestinationAddress.DisplayAddress);

            set.Bind(viewDestination.AddressButton)
                .For(ve => ve.AccessibilityLabel)
                .To(vm => vm.DestinationAddress.DisplayAddress);

            viewDestination.AddressButton.AccessibilityHint = Localize.GetValue("DestinationTextPlaceholder");

            set.Bind(viewDestination)
                .For(v => v.UserInputDisabled)
                .To(vm => vm.DestinationInputDisabled);

            set.Bind(viewVehicleType)
                .For(v => v.IsReadOnly)
                .To(vm => vm.VehicleTypeInputDisabled);

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
				.For(v => v.ShowVehicleSelection)
				.To(vm => vm.ShowVehicleSelection);
            
            set.Bind (viewVehicleType)
                .For (v => v.Vehicles)
                .To (vm => vm.VehicleTypes);

            set.Bind (viewVehicleType)
                .For (v => v.VehicleRepresentations)
                .To (vm => vm.VehicleRepresentations);
			
            set.Bind (viewVehicleType)
                .For (v => v.SelectedVehicle)
                .To (vm => vm.SelectedVehicleType);
            
			set.Bind (viewVehicleType)
				.For (v => v.Eta)
				.To (vm => vm.FormattedEta);

            set.Bind(viewVehicleType)
                .For(v => v.ShowEta)
                .To(vm => vm.ShowEtaInEstimate);

            set.Bind(viewVehicleType)
                .For(v => v.GroupVehiclesByServiceType)
                .To(vm => vm.GroupVehiclesByServiceType);

            set.Bind(viewEta)
                .For(v => v.Hidden)
                .To(vm => vm.ShowEta)
                .WithConversion("BoolInverter");

			set.Bind(viewEta)
                .For(v => v.DisplayBaseRateInfo)
                .To(vm => vm.DisplayBaseRateInfo);

            set.Bind(viewEta)
                .For(v => v.UserInputDisabled)
                .To(vm => vm.CanShowRateBox)
                .WithConversion("BoolInverter");
            
            set.Bind (viewEta)
                .For (v => v.SelectedVehicle)
                .To (vm => vm.SelectedVehicleType);
            
            set.Bind (viewEta)
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

        public void Resize ()
		{
			HeightConstraint.Constant = (nfloat)Subviews [0].Subviews.Where (x => !x.Hidden).Sum (x => x.Frame.Height);
            SetNeedsDisplay();
        }
    }
}

