using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Widget;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Android.Views;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Extensions;
using Cirrious.MvvmCross.Binding.Attributes;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class OrderMapFragment: IMvxBindable
    {
        public GoogleMap Map { get; set;}
        private Marker _pickupPin;
        private Marker _dropoffPin;

        public OrderMapFragment(SupportMapFragment _map)
        {
            this.CreateBindingContext();
            Map = _map.Map;
        }

        public IMvxBindingContext BindingContext { get; set; }

        [MvxSetToNullAfterBinding]
        public object DataContext
        {
            get { return BindingContext.DataContext; }
            set { BindingContext.DataContext = value; }
        }

        public void ApplyBindings()
        {
            var binding = this.CreateBindingSet<OrderMapFragment, MapViewModel>();

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
            if (PickupAddress == null)
                return; 

            if (_pickupPin == null)
            {
                _pickupPin = Map.AddMarker(new MarkerOptions()
                                           .SetPosition(new LatLng(0, 0))
                                           .Anchor(.5f, 1f)
                                           .InvokeIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.hail_icon))
                                           .Visible(false));
            }

            if (PickupAddress.HasValidCoordinate())
            {
                _pickupPin.Visible = true;
                _pickupPin.Position = new LatLng(PickupAddress.Latitude, PickupAddress.Longitude);
            }
            else
            {
                _pickupPin.Visible = false;
            }
        }

        private void OnMapBoundsChanged()
        {
            if (MapBounds != null)
            {
                var center = MapBounds.GetCenter();
                Map.AnimateCamera(CameraUpdateFactory.NewLatLng(new LatLng(center.Latitude, center.Longitude)));


                Map.AnimateCamera(
                    CameraUpdateFactory.NewLatLngBounds(
                        new LatLngBounds(new LatLng(center.Latitude - MapBounds.LatitudeDelta, center.Longitude - MapBounds.LongitudeDelta), 
                                     new LatLng(center.Latitude + MapBounds.LatitudeDelta, center.Longitude + MapBounds.LongitudeDelta)),
                        DrawHelper.GetPixels(100)));
            }
        }
    }
}