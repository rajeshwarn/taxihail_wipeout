using System;
using System.Drawing;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Touch.Views;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using System.Linq;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public partial class OrderOptionsControl : BaseBindableChildView<OrderOptionsViewModel>
    {
        public OrderOptionsControl (IntPtr handle) : base(handle)
        {
        }

        private void Initialize()
        {
            AutoresizingMask = UIViewAutoresizing.FlexibleHeight;

            BackgroundColor = UIColor.Clear;
            viewPickup.BackgroundColor = UIColor.Clear;
            viewDestination.BackgroundColor = UIColor.Clear;
            
            viewDestination.IsDestination = true;

            // since we don't have the vehicle selection yet, we hardcode this value
            viewVehicleType.ShowEstimate = true;
            viewVehicleType.VehicleType = "Taxi";
        }

        private void InitializeBinding()
        {
            // TODO to toggle the AddressSearch, taken from TaxiDiamond
//            viewPickup.AddressClicked += () => StartAddressSearch (Address.GetFirstPortionOfAddress(viewPickup.Address), address => {
//                ViewModel.PickupAddress = address.Address;
//            });
//
//            viewDestination.AddressClicked += () => StartAddressSearch (Address.GetFirstPortionOfAddress(viewDestination.Address), address => {
//                ViewModel.DestinationAddress = address.Address;
//            });

            viewPickup.AddressUpdated = (streetNumber, fullAddress) =>
            {
                var newAddress = ViewModel.PickupAddress.Copy();
                newAddress.StreetNumber = streetNumber;
                newAddress.FullAddress = fullAddress;

                ViewModel.PickupAddress = newAddress;
            };

            viewDestination.AddressUpdated = (streetNumber, fullAddress) =>
            {
                var newAddress = ViewModel.DestinationAddress.Copy();
                newAddress.StreetNumber = streetNumber;
                newAddress.FullAddress = fullAddress;

                ViewModel.DestinationAddress = newAddress;
            };

            var set = this.CreateBindingSet<OrderOptionsControl, OrderOptionsViewModel>();

            set.Bind(viewPickup)
                .For(v => v.IsReadOnly)
                .To(vm => vm.ShowDestination);
            set.Bind(viewPickup.AddressTextView)
                .To(vm => vm.PickupAddress.DisplayAddress);

            set.Bind(viewDestination)
                .For(v => v.Hidden)
                .To(vm => vm.ShowDestination)
                .WithConversion("BoolInverter");
            set.Bind(viewDestination)
                .For(v => v.IsReadOnly)
                .To(vm => vm.IsConfirmationScreen);
            set.Bind(viewDestination.AddressTextView)
                .To(vm => vm.DestinationAddress.DisplayAddress);

            set.Bind(viewVehicleType)
                .For(v => v.EstimatedFare)
                .To(vm => vm.EstimatedFare);
            set.Bind(viewVehicleType)
                .For(v => v.Hidden)
                .To(vm => vm.ShowDestination)
                .WithConversion("BoolInverter");

            set.Apply();
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
            var nib = UINib.FromName ("OrderOptionsControl", null);
            AddSubview((UIView)nib.Instantiate (this, null)[0]);

            Initialize();
            this.DelayBind (() => {
                InitializeBinding();
            });
        }
    }
}

