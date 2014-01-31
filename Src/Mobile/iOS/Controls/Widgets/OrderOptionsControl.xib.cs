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
            var set = this.CreateBindingSet<OrderOptionsControl, HomeViewModel>();

            set.Bind(this)
                .For(v => v.AddressSelectionMode)
                .To(vm => vm.AddressSelectionMode);

            set.Bind(viewPickup.AddressTextView)
                .To(vm => vm.PickupAddress.DisplayAddress);

            set.Apply();
        }

        AddressSelectionMode _addressSelectionMode;
        public AddressSelectionMode AddressSelectionMode
        {
            get { return _addressSelectionMode; }
            set
            {
                _addressSelectionMode = value;
                if (value == AddressSelectionMode.PickupSelection)
                {
                    viewPickup.IsReadOnly = false;
                    viewDestination.Hidden = true;
//                    viewVehicleType.ShowEstimate = false;
                }
                else
                {
                    viewPickup.IsReadOnly = true;
                    viewDestination.Hidden = false;
//                    viewVehicleType.ShowEstimate = true;
                }
            }
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

