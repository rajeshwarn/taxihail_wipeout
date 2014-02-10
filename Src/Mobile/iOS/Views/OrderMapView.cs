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
using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Resources;
using System.Drawing;
using System.Linq;
using apcurium.MK.Booking.Mobile.Data;
using TinyIoC;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    [Register("OrderMapView")]
    public class OrderMapView: BindableMapView
    {
        private AddressAnnotation _pickupAnnotation;
        private AddressAnnotation _destinationAnnotation;
        private List<AddressAnnotation> _availableVehicleAnnotations = new List<AddressAnnotation> ();

        private bool UseThemeColorForPickupAndDestinationMapIcons;

        public OrderMapView(IntPtr handle)
            :base(handle)
        {
            Initialize();
        }

        private void Initialize()
        {
            UseThemeColorForPickupAndDestinationMapIcons = TinyIoCContainer.Current.Resolve<IAppSettings>().Data.UseThemeColorForMapIcons;

            Delegate = new AddressMapDelegate ();

            this.DelayBind(() => {

                var set = this.CreateBindingSet<OrderMapView, MapViewModel>();

                set.Bind()
                    .For(v => v.PickupAddress)
                    .To(vm => vm.PickupAddress);

                set.Bind()
                    .For(v => v.DestinationAddress)
                    .To(vm => vm.DestinationAddress);

                set.Bind()
                    .For(v => v.AddressSelectionMode)
                    .To(vm => vm.AddressSelectionMode);

                set.Bind()
                    .For(v => v.MapBounds)
                    .To(vm => vm.MapBounds);

                set.Bind()
                    .For("AvailableVehicles")
                    .To(vm => vm.AvailableVehicles);

                set.Apply();

            });

            _pickupAnnotation = new AddressAnnotation(new CLLocationCoordinate2D(),
                AddressAnnotationType.Pickup,
                string.Empty,
                string.Empty,
                UseThemeColorForPickupAndDestinationMapIcons);
            _destinationAnnotation = new AddressAnnotation(new CLLocationCoordinate2D(),
                AddressAnnotationType.Destination,
                string.Empty,
                string.Empty,
                UseThemeColorForPickupAndDestinationMapIcons);
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

        private AddressSelectionMode _addressSelectionMode; 
        public AddressSelectionMode AddressSelectionMode
        { 
            get
            {
                return _addressSelectionMode;
            }
            set
            {
                _addressSelectionMode = value;
                ShowMarkers();
            }
        }

        private MapBounds _mapBounds;
        public MapBounds MapBounds
        {
            get { return _mapBounds; }
            set
            {
                if (_mapBounds != value)
                {
                    _mapBounds = value;
                    OnMapBoundsChanged();
                }
            }
        }

        public IEnumerable<AvailableVehicle> AvailableVehicles
        {
            set
            {
                ShowAvailableVehicles (VehicleClusterHelper.Clusterize(value != null ? value.ToArray() : null, MapBounds));
            }
        }

        private void OnPickupAddressChanged()
        {
            ShowMarkers();

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

        private void OnDestinationAddressChanged()
        {
            ShowMarkers();

            if (DestinationAddress.HasValidCoordinate())
            {
                RemoveAnnotation(_destinationAnnotation);
                _destinationAnnotation.Coordinate = DestinationAddress.GetCoordinate();
                AddAnnotation(_destinationAnnotation);
            }
            else
            {
                RemoveAnnotation (_destinationAnnotation);
            }
        }

        void UpdateAnnotation(Address address, AddressAnnotation addressAnnotation)
        {

        }

        void ShowMarkers()
        {
            if (AddressSelectionMode == AddressSelectionMode.DropoffSelection)
            {
                Position position = new Position(){ Latitude = PickupAddress.Latitude, Longitude = PickupAddress.Longitude };

                if (!DestinationAddress.HasValidCoordinate())
                {
//                   _destinationPin.Visible = false;
                }

//                _pickupPin.Visible = true;
//                _pickupPin.Position = new LatLng(position.Latitude, position.Longitude);
//                _pickupOverlay.Visibility = ViewStates.Invisible;
//                _destinationOverlay.Visibility = ViewStates.Visible;
//
                if (PickupAddress.HasValidCoordinate())
                {
//                    _pickupPin.Visible = true;
//                    _pickupPin.Position = new LatLng(position.Latitude, position.Longitude);
                }
                else
                {
//                    _pickupPin.Visible = false;
                }
            }
            else
            {
//                _pickupPin.Visible = false;
//                _destinationOverlay.Visibility = ViewStates.Invisible;
//                _pickupOverlay.Visibility = ViewStates.Visible;
//
                if (DestinationAddress.HasValidCoordinate())
                {
                    Position position = new Position(){ Latitude = DestinationAddress.Latitude, Longitude = DestinationAddress.Longitude };
//                    _destinationPin.Visible = true;
//                    _destinationPin.Position = new LatLng(position.Latitude, position.Longitude);             
                }
                else
                {
//                    _destinationPin.Visible = false;
                }
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

        private void ShowAvailableVehicles(IEnumerable<AvailableVehicle> vehicles)
        {
            foreach (var vehicleAnnotation in _availableVehicleAnnotations)
            {
                RemoveAnnotation(vehicleAnnotation);
            }
            _availableVehicleAnnotations.Clear ();

            if (vehicles == null)
                return;

            foreach (var v in vehicles)
            {
                var annotationType = (v is AvailableVehicleCluster) 
                                     ? AddressAnnotationType.NearbyTaxiCluster 
                                     : AddressAnnotationType.NearbyTaxi;

                var vehicleAnnotation = new AddressAnnotation (new CLLocationCoordinate2D(v.Latitude, v.Longitude), annotationType, string.Empty, string.Empty, UseThemeColorForPickupAndDestinationMapIcons);
                AddAnnotation (vehicleAnnotation);
                _availableVehicleAnnotations.Add (vehicleAnnotation);
            }
        }
    }
}

