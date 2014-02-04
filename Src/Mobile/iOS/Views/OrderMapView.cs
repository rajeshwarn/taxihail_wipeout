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

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    [Register("OrderMapView")]
    public class OrderMapView: BindableMapView
    {
        private AddressAnnotation _pickupAnnotation;
        private AddressAnnotation _destinationAnnotation;
        private List<AddressAnnotation> _availableVehicleAnnotations = new List<AddressAnnotation> ();

        public OrderMapView(IntPtr handle)
            :base(handle)
        {
            Initialize();
        }

        private void Initialize()
        {
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
                string.Empty);
            _destinationAnnotation = new AddressAnnotation(new CLLocationCoordinate2D(),
                AddressAnnotationType.Destination,
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
                ShowAvailableVehicles (Clusterize(value.ToArray()));
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

        private void OnDestinationAddressChanged()
        {
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
            // remove currently displayed pushpins
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

                var vehicleAnnotation = new AddressAnnotation (new CLLocationCoordinate2D(v.Latitude, v.Longitude), annotationType, string.Empty, string.Empty);
                AddAnnotation (vehicleAnnotation);
                _availableVehicleAnnotations.Add (vehicleAnnotation);
            }
        }

        // TODO Refactor this to share the code between iOS/Android
        private AvailableVehicle[] Clusterize(AvailableVehicle[] vehicles)
        {
            // Divide the map in 25 cells (5*5)
            const int numberOfRows = 5;
            const int numberOfColumns = 5;
            // Maximum number of vehicles in a cell before we start displaying a cluster
            const int cellThreshold = 1;

            var result = new List<AvailableVehicle>();

            var bounds =  Bounds;
            var clusterWidth = bounds.Width / numberOfColumns;
            var clusterHeight = bounds.Height / numberOfRows;

            var list = new List<AvailableVehicle>(vehicles);

            for (var rowIndex = 0; rowIndex < numberOfRows; rowIndex++)
            {
                for (var colIndex = 0; colIndex < numberOfColumns; colIndex++)
                {
                    var rect = new RectangleF(Bounds.X + colIndex * clusterWidth, Bounds.Y + rowIndex * clusterHeight, clusterWidth, clusterHeight);

                    var vehiclesInRect = list.Where(v => rect.Contains(ConvertCoordinate(new CLLocationCoordinate2D(v.Latitude, v.Longitude), this))).ToArray();
                    if (vehiclesInRect.Length > cellThreshold)
                    {
                        var clusterBuilder = new VehicleClusterBuilder();
                        foreach (var v in vehiclesInRect)
                            clusterBuilder.Add(v);
                        result.Add(clusterBuilder.Build());
                    }
                    else
                    {
                        result.AddRange(vehiclesInRect);
                    }

                    foreach (var v in vehiclesInRect)
                    {
                        list.Remove(v);
                    }
                }
            }
            return result.ToArray();
        }
    }
}

