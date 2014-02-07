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

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public partial class OrderOptionsControl : OverlayView
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

            // since we don't have the vehicle selection yet, we hardcode this value
            viewVehicleType.ShowEstimate = true;
            viewVehicleType.VehicleType = "Taxi";
        }

        private void InitializeBinding()
        {
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

    }
}

