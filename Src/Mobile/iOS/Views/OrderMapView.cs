using System;
using MonoTouch.Foundation;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.Client.MapUtitilties;
using MonoTouch.CoreLocation;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.BindingContext;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    [Register("OrderMapView")]
    public class OrderMapView: BindableMapView
    {
        private AddressAnnotation _pickupAnnotation;

        public OrderMapView(IntPtr handle)
            :base(handle)
        {
            Initialize();
        }

        private void Initialize()
        {
            this.DelayBind(() => {

                var set = this.CreateBindingSet<OrderMapView, MapViewModel>();

                set.Bind()
                    .For(v => v.PickupAddress)
                    .To(vm => vm.PickupAddress);

                set.Apply();

            });

            _pickupAnnotation = new AddressAnnotation(new CLLocationCoordinate2D(),
                AddressAnnotationType.Pickup,
                Localize.GetValue("PickupMapTitle"),
                string.Empty);
        }

        private Address _pickupAddress;
        public Address PickupAddress
        {
            get { return _pickupAddress; }
            set
            { 
                _pickupAddress = value;
                OnPickupAddressChanged();
            }
        }

        private void OnPickupAddressChanged()
        {
            if (PickupAddress.HasValidCoordinate())
            {
                RemoveAnnotation(_pickupAnnotation);

                _pickupAnnotation.Coordinate = PickupAddress.GetCoordinate();

                AddAnnotation(_pickupAnnotation);
            }
            else
            {
                RemoveAnnotation (_pickupAnnotation);
            }
        }
    }
}

