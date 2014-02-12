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
using apcurium.MK.Booking.Mobile.PresentationHints;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public partial class OrderOptionsControl : BaseBindableChildView<OrderOptionsViewModel>
    {
        private NSLayoutConstraint _heightConstraint;

        public OrderOptionsControl (IntPtr handle) : base(handle)
        {
        }

        private void Initialize()
        {
            _heightConstraint = NSLayoutConstraint.Create(this, NSLayoutAttribute.Height, 
                NSLayoutRelation.Equal, 
                null, 
                NSLayoutAttribute.NoAttribute, 
                1.0f, 44.0f);

            this.AddConstraint(_heightConstraint);

            BackgroundColor = UIColor.Clear;
            viewPickup.BackgroundColor = UIColor.Clear;
            viewDestination.BackgroundColor = UIColor.Clear;
            
            viewDestination.IsDestination = true;

            // temporary until we can be notified by the service that we're searching for an address
            viewPickup.IsLoadingAddress = false;
            viewDestination.IsLoadingAddress = false;

            // since we don't have the vehicle selection yet, we hardcode this value
            viewVehicleType.VehicleType = "Taxi";
        }

        private void InitializeBinding()
        {
            viewPickup.AddressUpdated = (streetNumber, fullAddress) =>
            {
                ViewModel.PickupAddress.StreetNumber = streetNumber;
                ViewModel.PickupAddress.FullAddress = fullAddress;

                ViewModel.SetAddress.Execute(ViewModel.PickupAddress);
            };

            viewDestination.AddressUpdated = (streetNumber, fullAddress) =>
            {
                ViewModel.DestinationAddress.StreetNumber = streetNumber;
                ViewModel.DestinationAddress.FullAddress = fullAddress;

                ViewModel.SetAddress.Execute(ViewModel.DestinationAddress);
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
            set.Bind(viewDestination.AddressTextView)
                .To(vm => vm.DestinationAddress.DisplayAddress);

            set.Bind(viewVehicleType)
                .For(v => v.EstimatedFare)
                .To(vm => vm.EstimatedFare);
            set.Bind(viewVehicleType)
                .For(v => v.Hidden)
                .To(vm => vm.ShowDestination)
                .WithConversion("BoolInverter");
            set.Bind(viewVehicleType)
                .For(v => v.ShowEstimate)
                .To(vm => vm.ShowDestination);

            set.Bind(viewPickup)
                .For("AddressClicked")
                .To(vm => vm.ShowSearchAddress);

            set.Bind(viewDestination)
                .For("AddressClicked")
                .To(vm => vm.ShowSearchAddress);

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

        public void ChangeState(HomeViewModelPresentationHint hint)
        {
            if (hint.State == HomeViewModelState.Review)
            {
                viewPickup.IsReadOnly = true;
                viewDestination.IsReadOnly = true;
            }
            else if(hint.State == HomeViewModelState.Initial)
            {
                viewPickup.IsReadOnly = ViewModel.ShowDestination;
                viewDestination.IsReadOnly = false;
            }
        }
    }
}

