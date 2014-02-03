using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Widget;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Android.Views;
using Cirrious.MvvmCross.Binding.BindingContext;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class OrderMapView: BindableMapView
    {
        public GoogleMap Map { get; set;}
        private ImageView _pickupCenterPin;
        private Marker _pickupPin;
        private ImageView _dropoffCenterPin;
        private Marker _dropoffPin;

        public OrderMapView(SupportMapFragment _map)
        {
            Map = _map.Map;
        }

        public void ApplyBindings()
        {
            var binding = this.CreateBindingSet<OrderMapView, MapViewModel>();

            binding.Bind()
                .For(v => v.PickupAddress)
                    .To(vm => vm.PickupAddress);

            binding.Bind()
                .For(v => v.MapBounds)
                    .To(vm => vm.MapBounds);

            binding.Apply();
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
            ShowPickupPin(PickupAddress);           
        }

        private void OnMapBoundsChanged()
        {
            if (MapBounds != null)
            {
                var center = MapBounds.GetCenter();
                Map.AnimateCamera(CameraUpdateFactory.NewLatLng(new LatLng(center.Latitude, center.Longitude)));

                // SET ZOOM here

                //Map.AnimateCamera(CameraUpdateFactory.
                //SetRegion(new MKCoordinateRegion(
                  //  new CLLocationCoordinate2D(center.Latitude, center.Longitude),
//                    new MKCoordinateSpan(MapBounds.LatitudeDelta, MapBounds.LongitudeDelta)), true);


            }
        }

        public void SetMapCenterPins(ImageView pickup, ImageView dropoff)
        {
            _pickupCenterPin = pickup;
            _dropoffCenterPin = dropoff;

            //if (AddressSelectionMode == AddressSelectionMode.PickupSelection)
                _pickupCenterPin.Visibility = ViewStates.Visible;
            //if (AddressSelectionMode == AddressSelectionMode.DropoffSelection)
           //  _dropoffCenterPin.Visibility = ViewStates.Visible;
        }

        private void ShowPickupPin(Address address)
        {
            if (_pickupPin == null)
            {
                _pickupPin = Map.AddMarker(new MarkerOptions()
                                           .SetPosition(new LatLng(0, 0))
                                           .Anchor(.5f, 1f)
                                           .InvokeIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.pin_hail))
                                           .Visible(false));
            }

            if (address == null)
                return;

            if (address.Latitude != 0 && address.Longitude != 0)            
            {
                _pickupPin.Position = new LatLng(address.Latitude, address.Longitude);
                _pickupPin.Visible = true;
            }
            if (_pickupCenterPin != null) _pickupCenterPin.Visibility = ViewStates.Gone;
        }
    }
}