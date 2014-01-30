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
using MonoTouch.MapKit;

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

                set.Bind()
                    .For(v => v.MapBounds)
                    .To(vm => vm.MapBounds);

                set.Apply();

            });

            _pickupAnnotation = new AddressAnnotation(new CLLocationCoordinate2D(),
                AddressAnnotationType.Pickup,
                string.Empty,
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

        private MapBounds _mapBounds;
        public MapBounds MapBounds
        {
            get
            {
                return _mapBounds;
            }

            set
            {
                if (_mapBounds != value)
                {
                    _mapBounds = value;
                    OnMapBoundsChanged();
                }
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

        private void OnMapBoundsChanged()
        {
            if (MapBounds != null)
            {
                var center = MapBounds.GetCenter();

                SetRegion(new MKCoordinateRegion(
                    new CLLocationCoordinate2D(center.Latitude, center.Longitude),
                    new MKCoordinateSpan(MapBounds.LatitudeDelta, MapBounds.LongitudeDelta)), true);
            }
        }
    }
}

