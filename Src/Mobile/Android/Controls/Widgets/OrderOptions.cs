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
using apcurium.MK.Booking.Mobile.PresentationHints;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public class OrderOptions : MvxFrameControl
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

                // temporary until we can be notified by the service that we're searching for an address
                ViewPickup.IsLoadingAddress = false;
                ViewDestination.IsLoadingAddress = false;

                // since we don't have the vehicle selection yet, we hardcode this value
                ViewVehicleType.VehicleType = "Taxi";

                ViewPickup.SetInvisibleButton(BigInvisibleButton);
                ViewDestination.SetInvisibleButton(BigInvisibleButton);

                InitializeBinding();
            });
        }

        private OrderOptionsViewModel ViewModel { get { return (OrderOptionsViewModel)DataContext; } }

        void InitializeBinding()
        {
            ViewPickup.AddressUpdated = (streetNumber, fullAddress) =>
            {
                ViewModel.PickupAddress.StreetNumber = streetNumber;
                ViewModel.PickupAddress.FullAddress = fullAddress;

                ViewModel.SetAddress.Execute(ViewModel.PickupAddress);
            };

            ViewDestination.AddressUpdated = (streetNumber, fullAddress) =>
            {
                ViewModel.DestinationAddress.StreetNumber = streetNumber;
                ViewModel.DestinationAddress.FullAddress = fullAddress;

                ViewModel.SetAddress.Execute(ViewModel.DestinationAddress);
            };

            var set = this.CreateBindingSet<OrderOptions, OrderOptionsViewModel>();

            set.Bind(ViewPickup)
                .For(v => v.IsReadOnly)
                .To(vm => vm.ShowDestination);

            set.Bind(ViewPickup.AddressTextView)
                .To(vm => vm.PickupAddress.DisplayAddress);

            set.Bind(ViewDestination)
                .For(v => v.Visibility)
                .To(vm => vm.ShowDestination)
                .WithConversion("Visibility");
           
            set.Bind(ViewDestination.AddressTextView)
                .To(vm => vm.DestinationAddress.DisplayAddress);

            set.Bind(ViewVehicleType)
                .For(v => v.EstimatedFare)
                .To(vm => vm.EstimatedFare);
            set.Bind(ViewVehicleType)
                .For(v => v.Visibility)
                .To(vm => vm.ShowDestination)
                .WithConversion("Visibility");


            set.Bind(ViewPickup.AddressTextView)
                .For("Click")
                .To(vm => vm.ShowSearchAddress);

            set.Bind(ViewPickup.AddressTextView)
                .For("Click")
                .To(vm => vm.ShowSearchAddress);


            set.Apply();
        }

        public void ChangePresentation(HomeViewModelPresentationHint hint)
        {
            if (hint.State == HomeViewModelState.Review)
            {
                ViewPickup.IsReadOnly = true;
                ViewDestination.IsReadOnly = true;
            }
            else if(hint.State == HomeViewModelState.PickDate)
            {
                ViewPickup.IsReadOnly = true;
                ViewDestination.IsReadOnly = true;
            }
            else if(hint.State == HomeViewModelState.Initial)
            {
                ViewPickup.IsReadOnly = ViewModel.ShowDestination;
                ViewDestination.IsReadOnly = false;
            }
        }
    }
}

