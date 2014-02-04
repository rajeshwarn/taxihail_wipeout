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
using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Data;
using System.Linq;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class OrderMapFragment: IMvxBindable
    {
        public GoogleMap Map { get; set;}
        private Marker _pickupPin;
        private Marker _destinationPin;

        private List<Marker> _availableVehicleMarkers = new List<Marker> ();

        public OrderMapFragment(SupportMapFragment _map)
        {
            this.CreateBindingContext();
            Map = _map.Map;
            this.DelayBind(() => InitializeBinding());
        }

        public IMvxBindingContext BindingContext { get; set; }

        [MvxSetToNullAfterBinding]
        public object DataContext
        {
            get { return BindingContext.DataContext; }
            set 
            { 
                BindingContext.DataContext = value; 
            }
        }

        public void InitializeBinding()
        {
            var binding = this.CreateBindingSet<OrderMapFragment, MapViewModel>();

            binding.Bind()
                .For(v => v.PickupAddress)
                    .To(vm => vm.PickupAddress);
            
            binding.Bind()
                .For(v => v.DestinationAddress)
                    .To(vm => vm.DestinationAddress);

            binding.Bind()
                .For(v => v.MapBounds)
                    .To(vm => vm.MapBounds);
            
            binding.Bind()
                .For(v => v.AvailableVehicles)
                    .To(vm => vm.AvailableVehicles);

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

        private Address _destinationAddress;
        public Address DestinationAddress
        {
            get { return _destinationAddress; }
            set
            { 
                _destinationAddress = value;
                OnDestinationAddressChanged();
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

        private IEnumerable<AvailableVehicle> _availableVehicles = new List<AvailableVehicle>();
        public IEnumerable<AvailableVehicle> AvailableVehicles
        {
            get
            {
                return _availableVehicles;
            }
            set
            {
                if (_availableVehicles != value)
                {
                    ShowAvailableVehicles (VehicleClusterHelper.Clusterize(value.ToArray(), MapBounds));
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

        private void OnDestinationAddressChanged()
        {
            if (DestinationAddress == null)
                return; 

            if (_destinationPin == null)
            {
                _destinationPin = Map.AddMarker(new MarkerOptions()
                                           .SetPosition(new LatLng(0, 0))
                                           .Anchor(.5f, 1f)
                                           .InvokeIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.destination_icon))
                                           .Visible(false));
            }

            if (DestinationAddress.HasValidCoordinate())
            {
                _destinationPin.Visible = true;
                _destinationPin.Position = new LatLng(DestinationAddress.Latitude, DestinationAddress.Longitude);
            }
            else
            {
                _destinationPin.Visible = false;
            }
        }

        private void OnMapBoundsChanged()
        {
            if (MapBounds != null)
            {
                Map.AnimateCamera(
                    CameraUpdateFactory.NewLatLngBounds(
                        new LatLngBounds(new LatLng(MapBounds.SouthBound, MapBounds.WestBound), 
                                     new LatLng(MapBounds.NorthBound, MapBounds.EastBound)),
                        DrawHelper.GetPixels(100)));
            }
        }

        private void ShowAvailableVehicles(AvailableVehicle[] vehicles)
        {
            foreach (var vehicleMarker in _availableVehicleMarkers)
            {                
                vehicleMarker.Remove();
            }

            _availableVehicleMarkers.Clear();

            if (vehicles == null)
                return;

            foreach (var v in vehicles)
            {
                bool isCluster = (v is AvailableVehicleCluster) 
                    ? true 
                        : false;

                var vehicleMarker = 
                    Map.AddMarker(new MarkerOptions()
                                  .SetPosition(new LatLng(0, 0))
                                  .Anchor(.5f, 1f)
                                  .InvokeIcon(BitmapDescriptorFactory.FromResource(isCluster ? Resource.Drawable.cluster : Resource.Drawable.nearby_taxi))
                                  .Visible(false));

                _availableVehicleMarkers.Add (vehicleMarker);
            }
        }
    }
}