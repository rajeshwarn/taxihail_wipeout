using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Cirrious.MvvmCross.Binding.BindingContext;
using MonoTouch.CoreLocation;
using MonoTouch.Foundation;
using MonoTouch.MapKit;
using MonoTouch.UIKit;
using TinyIoC;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Client.MapUtitilties;
using apcurium.MK.Booking.Mobile.Client.Controls;
using System.Windows.Input;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    [Register("OrderMapView")]
    public class OrderMapView: BindableMapView
    {
        private AddressAnnotation _pickupAnnotation;
        private AddressAnnotation _destinationAnnotation;
        private UIImageView _pickupCenterPin;
        private UIImageView _dropoffCenterPin;
        private List<AddressAnnotation> _availableVehicleAnnotations = new List<AddressAnnotation> ();
        private TouchGesture _gesture;
        private ICommand _userMovedMap;

        private bool UseThemeColorForPickupAndDestinationMapIcons;

        public OrderMapView(IntPtr handle)
            :base(handle)
        {
            Initialize();
        }

        private bool _useThemeColorForPickupAndDestinationMapIcons;

        private void Initialize()
        {
            _useThemeColorForPickupAndDestinationMapIcons = TinyIoCContainer.Current.Resolve<IAppSettings>().Data.UseThemeColorForMapIcons;

            //Delegate = new AddressMapDelegate ();

            this.DelayBind(() => {

                var set = this.CreateBindingSet<OrderMapView, MapViewModel>();

                set.Bind()
                    .For(v => v.PickupAddress)
                    .To(vm => vm.PickupAddress);

                set.Bind()
                    .For(v => v.DestinationAddress)
                    .To(vm => vm.DestinationAddress);

                set.Bind()
                    .For(v => v.UserMovedMap)
                    .To(vm => vm.UserMovedMap);

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

            _pickupAnnotation = GetAnnotation(new CLLocationCoordinate2D(), AddressAnnotationType.Pickup, _useThemeColorForPickupAndDestinationMapIcons);
            _destinationAnnotation = GetAnnotation(new CLLocationCoordinate2D(), AddressAnnotationType.Destination, _useThemeColorForPickupAndDestinationMapIcons);

            InitOverlays();
            InitializeGesture();
        }

        public AddressAnnotation GetAnnotation(CLLocationCoordinate2D coordinates, AddressAnnotationType addressType, bool useThemeColorForPickupAndDestinationMapIcons)
        {
            return new AddressAnnotation(coordinates,
                addressType,
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
        }

        private void OnDestinationAddressChanged()
        {
            ShowMarkers();
        }

        void SetAnnotation(Address address, AddressAnnotation addressAnnotation, bool visible)
        {
            if (address.HasValidCoordinate() && visible)
            {
                RemoveAnnotation(addressAnnotation);
                addressAnnotation.Coordinate = address.GetCoordinate(); //= // GetAnnotation(address.GetCoordinate(), AddressAnnotationType.Destination, _useThemeColorForPickupAndDestinationMapIcons);
                AddAnnotation(addressAnnotation);
            }
            else
            {
                RemoveAnnotation (addressAnnotation);
            }
        }

        void SetOverlay(UIImageView overlay, bool visible)
        {
            overlay.Hidden = !visible;
        }

        void InitOverlays()
        {
            _dropoffCenterPin = new UIImageView(AddressAnnotation.GetImage(AddressAnnotationType.Destination));
            _pickupCenterPin = new UIImageView(AddressAnnotation.GetImage(AddressAnnotationType.Pickup));

            var pinSize = _pickupCenterPin.IntrinsicContentSize;

            _pickupCenterPin.Frame = 
                _dropoffCenterPin.Frame = 
                    new RectangleF((this.Bounds.Width / 2) - (pinSize.Width / 2), ((this.Bounds.Height / 2) - pinSize.Height), pinSize.Width, pinSize.Height);

            _pickupCenterPin.BackgroundColor = UIColor.Clear;
            _pickupCenterPin.ContentMode = UIViewContentMode.Center;
            AddSubview(_pickupCenterPin);
            _pickupCenterPin.Hidden = true;                
            
            _dropoffCenterPin.BackgroundColor = UIColor.Clear;
            _dropoffCenterPin.ContentMode = UIViewContentMode.Center;
            AddSubview(_dropoffCenterPin);
            _dropoffCenterPin.Hidden = true;    
        }

        void ShowMarkers()
        {
            if (AddressSelectionMode == AddressSelectionMode.DropoffSelection)
            {
                if (!DestinationAddress.HasValidCoordinate())
                {
                    SetAnnotation(DestinationAddress, _destinationAnnotation, false);
                }

                SetOverlay(_pickupCenterPin, false);
                SetOverlay(_dropoffCenterPin, true);

                if (PickupAddress.HasValidCoordinate())
                {
                    SetAnnotation(PickupAddress, _pickupAnnotation, true);
                }
                else
                {
                    SetAnnotation(PickupAddress, _pickupAnnotation, false);
                }
            }
            else
            {
                SetAnnotation(PickupAddress, _pickupAnnotation, false);
                SetOverlay(_dropoffCenterPin, false);
                SetOverlay(_pickupCenterPin, true);

                if (DestinationAddress.HasValidCoordinate())
                {
                    SetAnnotation(DestinationAddress, _destinationAnnotation, true);
                }
                else
                {
                    SetAnnotation(DestinationAddress, _destinationAnnotation, false);
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

        public ICommand UserMovedMap { get; set; }

        private void InitializeGesture()
        {
            if (_gesture == null)
            {
                _gesture = new TouchGesture();              
                _gesture.TouchMove += HandleTouchMove;
                _gesture.TouchBegin += HandleTouchBegin;
                this.RegionChanged += OnRegionChanged;
                AddGestureRecognizer(_gesture);
            }
        }

        public void OnRegionChanged(object sender, MKMapViewChangeEventArgs e)
        {            
            try
            {
                if (_gesture.GetLastTouchDelay() < 1000)
                {
                    var bounds = GetMapBoundsFromProjection();
                    if (UserMovedMap != null && UserMovedMap.CanExecute(bounds))
                    {
                        if (bounds.LatitudeDelta < 0.3)
                        {
                            UserMovedMap.Execute(bounds);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private MapBounds GetMapBoundsFromProjection()
        {
            var bounds = new MapBounds()
            { 
                SouthBound = Region.Center.Latitude - (Region.Span.LatitudeDelta / 2), 
                WestBound = Region.Center.Longitude - (Region.Span.LongitudeDelta / 2), 
                NorthBound = Region.Center.Latitude + (Region.Span.LatitudeDelta / 2), 
                EastBound = Region.Center.Longitude + (Region.Span.LongitudeDelta / 2)
            };

            return bounds;
        }

        void HandleTouchMove (object sender, EventArgs e)
        {
            ((MapViewModel.CancellableCommand<MapBounds>)UserMovedMap).Cancel();
        }
                             
        void HandleTouchBegin (object sender, EventArgs e)
        {
            ((MapViewModel.CancellableCommand<MapBounds>)UserMovedMap).Cancel();
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

