using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using Cirrious.MvvmCross.Binding.Attributes;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public class OrderOptions : MvxFrameControl
    {
        private AddressTextBox viewPickup;
        private AddressTextBox viewDestination;
        private VehicleTypeAndEstimateControl viewVehicleType;

        public OrderOptions(Context context, IAttributeSet attrs) : base (Resource.Layout.SubView_OrderOptions, context, attrs)
        {
            this.DelayBind(() => 
            {
                viewPickup = Content.FindViewById<AddressTextBox>(Resource.Id.viewPickup);
                viewDestination = Content.FindViewById<AddressTextBox>(Resource.Id.viewDestination);
                viewVehicleType = Content.FindViewById<VehicleTypeAndEstimateControl>(Resource.Id.viewEstimate);

                viewDestination.IsDestination = true;

                // temporary until we can be notified by the service that we're searching for an address
                viewPickup.IsLoadingAddress = false;
                viewDestination.IsLoadingAddress = false;

                // since we don't have the vehicle selection yet, we hardcode this value
                viewVehicleType.VehicleType = "Taxi";

                InitializeBinding();
            });
        }

        private OrderOptionsViewModel ViewModel { get { return (OrderOptionsViewModel)DataContext; } }

        void InitializeBinding()
        {
            // TODO to toggle the AddressSearch, taken from TaxiDiamond
//            viewPickup.AddressClicked += () => StartAddressSearch (Address.GetFirstPortionOfAddress(viewPickup.Address), address => {
//                ViewModel.SetAddress.Execute(address.Address);
//            });
//
//            viewDestination.AddressClicked += () => StartAddressSearch (Address.GetFirstPortionOfAddress(viewDestination.Address), address => {
//                ViewModel.SetAddress.Execute(address.Address);
//            });

            viewPickup.AddressUpdated = (streetNumber, fullAddress) =>
            {
                var newAddress = ViewModel.PickupAddress.Copy();
                newAddress.StreetNumber = streetNumber;
                newAddress.FullAddress = fullAddress;

                ViewModel.SetAddress.Execute(newAddress);
            };

            viewDestination.AddressUpdated = (streetNumber, fullAddress) =>
            {
                var newAddress = ViewModel.DestinationAddress.Copy();
                newAddress.StreetNumber = streetNumber;
                newAddress.FullAddress = fullAddress;

                ViewModel.SetAddress.Execute(newAddress);
            };

            var set = this.CreateBindingSet<OrderOptions, OrderOptionsViewModel>();

            set.Bind(viewPickup)
                .For(v => v.IsReadOnly)
                .To(vm => vm.ShowDestination);
            set.Bind(viewPickup.AddressTextView)
                .To(vm => vm.PickupAddress.DisplayAddress);

            set.Bind(viewDestination)
                .For(v => v.Visibility)
                .To(vm => vm.ShowDestination)
                .WithConversion("Visibility");
            set.Bind(viewDestination)
                .For(v => v.IsReadOnly)
                .To(vm => vm.IsConfirmationScreen);
            set.Bind(viewDestination.AddressTextView)
                .To(vm => vm.DestinationAddress.DisplayAddress);

            set.Bind(viewVehicleType)
                .For(v => v.EstimatedFare)
                .To(vm => vm.EstimatedFare);
            set.Bind(viewVehicleType)
                .For(v => v.Visibility)
                .To(vm => vm.ShowDestination)
                .WithConversion("Visibility");

            set.Apply();
        }
    }
}

