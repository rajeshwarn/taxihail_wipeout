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
    public class OrderOptions : LinearLayout, IMvxBindable
    {
        private EditText _viewPickupAddressNumber;
        private EditText _viewPickupAddressText;
        private EditText _viewDestinationAddressNumber;
        private EditText _viewDestinationAddressText;
        private LinearLayout _viewPickup;
        private LinearLayout _viewDestination;

        public OrderOptions(Context context) :
            base (context)
        {
            this.CreateBindingContext();
        }

        public OrderOptions(Context context, IAttributeSet attrs) :
            base (context, attrs)
        {
            this.CreateBindingContext();
        }

        public IMvxBindingContext BindingContext { get; set; }

        [MvxSetToNullAfterBinding]
        public object DataContext
        {
            get { return BindingContext.DataContext; }
            set { BindingContext.DataContext = value; }
        }

        protected override void OnFinishInflate()
        {
            base.OnFinishInflate();
            var inflater = (LayoutInflater) Context.GetSystemService(Context.LayoutInflaterService);
            var layout = inflater.Inflate(Resource.Layout.Control_OrderOptions, this, true);
            _viewPickup = (LinearLayout) layout.FindViewById(Resource.Id.viewPickup);
            _viewDestination = (LinearLayout) layout.FindViewById(Resource.Id.viewDestination);
            _viewPickupAddressNumber = (EditText) layout.FindViewById(Resource.Id.viewPickupAddressNumber);
            _viewPickupAddressText = (EditText) layout.FindViewById(Resource.Id.viewPickupAddressText);
            _viewDestinationAddressNumber = (EditText) layout.FindViewById(Resource.Id.viewDestinationAddressNumber);
            _viewDestinationAddressText = (EditText) layout.FindViewById(Resource.Id.viewDestinationAddressText);

            Initialize();
        }

        void Initialize()
        {
            var set = this.CreateBindingSet<OrderOptions, OrderOptionsViewModel>();

//            set.Bind(_viewPickupAddressText)
//                .For(v => v.Enabled)
//                    .To(vm => vm.ShowDestination)
//                    .WithConversion("BoolInverter");

            set.Bind(_viewPickupAddressText)
                .To(vm => vm.PickupAddress.DisplayAddress);

            set.Bind(_viewDestination)
                .For(v => v.Visibility)
                    .To(vm => vm.ShowDestination)
                    .WithConversion("Visibility");

            set.Bind(_viewDestinationAddressText)
                .For(v => v.Enabled)
                    .To(vm => vm.IsConfirmationScreen)
                    .WithConversion("BoolInverted");

            set.Bind(_viewDestinationAddressText)
                .To(vm => vm.DestinationAddress.DisplayAddress);

            set.Apply();
        }
    }
}

