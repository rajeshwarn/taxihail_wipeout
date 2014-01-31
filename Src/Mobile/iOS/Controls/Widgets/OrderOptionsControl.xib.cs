using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public partial class OrderOptionsControl : BaseBindableView<HomeViewModel>
    {
        public OrderOptionsControl (IntPtr handle) : base(handle)
        {

        }

        private void Initialize()
        {
            BackgroundColor = UIColor.Clear;
            viewPickup.BackgroundColor = UIColor.Clear;
            viewDestination.BackgroundColor = UIColor.Clear;
            viewVehicleType.BackgroundColor = UIColor.Clear;

            viewDestination.IsDestination = true;
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

