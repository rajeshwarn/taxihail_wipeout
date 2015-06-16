using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Client.MapUtitilties;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using CoreLocation;
using Foundation;
using MapKit;
using System;
using System.Collections.Generic;
using CoreGraphics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using TinyIoC;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    [Register ("TouchMap")]
    public class TouchMap : MKMapView
    {
        private TouchGesture _gesture;
        private ICommand _mapMoved;
        private Address _pickup;
        private Address _dropoff;
        private CancellationTokenSource _cancelToken;
        private CancellationTokenSource _moveMapCommand;

		private bool _useThemeColorForPickupAndDestinationMapIcons;
		private bool _showAssignedVehicleNumberOnPin;

        protected TouchMap(CGRect rect) : base(rect)
        {
            Initialize();
        }

        public TouchMap(IntPtr handle) : base(handle)
        {
            Initialize();
        }
        
        public TouchMap(NSObjectFlag flag) : base(flag)
        {
            Initialize();
        }
        
        public TouchMap()
        {
            Initialize();
        }

        public TouchMap(NSCoder coder) : base(coder)
        {
            Initialize();
        }

        private void Initialize()
        {   
			var settings = TinyIoCContainer.Current.Resolve<IAppSettings> ().Data;
			_useThemeColorForPickupAndDestinationMapIcons = settings.UseThemeColorForMapIcons;
			_showAssignedVehicleNumberOnPin = settings.ShowAssignedVehicleNumberOnPin;
        }

        public void OnRegionChanged()
        {
            try
            {
                if (_gesture.GetLastTouchDelay() < 1000)
                {
                    ExecuteMoveMapCommand();
                }
            }
            catch (Exception)
            {}
        }

        private void InitializeGesture()
        {
            if (_gesture == null)
            {
                _gesture = new TouchGesture();
                _gesture.TouchBegin += HandleTouchBegin;
                AddGestureRecognizer(_gesture);
            }
        }

        private void CancelMoveMap()
        {
            if ((_moveMapCommand != null) && _moveMapCommand.Token.CanBeCanceled)
            {
                _moveMapCommand.Cancel();
                _moveMapCommand.Dispose();
                _moveMapCommand = null;
            }
        }

        private void ExecuteMoveMapCommand()
        {
            CancelMoveMap();

            _moveMapCommand = new CancellationTokenSource();
                                
            var t = new Task(() => Thread.Sleep(500), _moveMapCommand.Token);

            t.ContinueWith(r =>
            {
                if (r.IsCompleted && !r.IsCanceled && !r.IsFaulted)
                {
                    InvokeOnMainThread(() =>
                    {
						MapMoved.ExecuteIfPossible(new Address { Latitude = CenterCoordinate.Latitude, Longitude = CenterCoordinate.Longitude });                        
                    });
                }        
            },_moveMapCommand.Token);
            t.Start();
        }

        private void HandleTouchBegin (object sender, EventArgs e)
        {
            CancelMoveMap();
        }
               
        public ICommand MapMoved
        {
            get { return _mapMoved; } 
            set
            { 
                _mapMoved = value;
                if (_mapMoved != null)
                {
                    InitializeGesture();
                }
            } 
        }

        private AddressSelectionMode _addressSelectionMode;
        public AddressSelectionMode AddressSelectionMode 
        {
            get { return _addressSelectionMode; }
            set
            {
                _addressSelectionMode = value;
                switch (_addressSelectionMode)
                {
                    case AddressSelectionMode.PickupSelection:
                        if(_pickupPin != null) RemoveAnnotation(_pickupPin);
                        _pickupPin = null;
                        ShowDropOffPin(Dropoff);
                        SetNeedsDisplay();
                        break;
                    case AddressSelectionMode.DropoffSelection:
                        if(_dropoffPin != null) RemoveAnnotation(_dropoffPin);
                        _dropoffPin = null;
                        ShowPickupPin(Pickup);
                        SetNeedsDisplay();
                        break;
                    default:
                        ShowDropOffPin(Dropoff);
                        ShowPickupPin(Pickup);
                        SetNeedsDisplay();
                        break;
                }
            }
        }

        private AddressAnnotation _pickupPin;
        private AddressAnnotation _dropoffPin;
        
        public Address Pickup
        {
            get { return _pickup; }
            set
            { 
                _pickup = value;
                if(AddressSelectionMode == AddressSelectionMode.None)
                {
                    ShowPickupPin(value);
                }
            }
        }
        
        public Address Dropoff
        {
            get { return _dropoff; }
            set
            { 
                _dropoff = value;
                if(AddressSelectionMode == AddressSelectionMode.None)
                {
                    ShowDropOffPin(value);
                }
            }
        }
        
        private IEnumerable<CoordinateViewModel> _center;
        public IEnumerable<CoordinateViewModel> MapCenter
        {
            get { return _center; }
            set
            {
                _center = value;                
                SetZoom(MapCenter);                   
            }
        }

        private MKAnnotation _taxiLocationPin;

		private OrderStatusDetail _displayDeviceLocation;
		public OrderStatusDetail DisplayDeviceLocation
		{
			get { return _displayDeviceLocation; }
			set
			{
				_displayDeviceLocation = value;
				ShowsUserLocation = !_displayDeviceLocation.IBSStatusId.HasValue() || VehicleStatuses.CanCancelOrderStatus.Contains(_displayDeviceLocation.IBSStatusId);
			}
		}

        private OrderStatusDetail _taxiLocation;
        public OrderStatusDetail TaxiLocation
        {
            get { return _taxiLocation; }
            set
            {
                _taxiLocation = value;
                if (_taxiLocationPin != null)
                {
                    RemoveAnnotation(_taxiLocationPin);
                    _taxiLocationPin = null;
                }
                
                if ((value != null))
                {
                    var coord = new CLLocationCoordinate2D(0,0);            
                    if (value.VehicleLatitude.HasValue
                        && value.VehicleLongitude.HasValue
                        && value.VehicleLongitude.Value != 0
                        && value.VehicleLatitude.Value != 0
                        && !string.IsNullOrEmpty(value.VehicleNumber) 
						&& VehicleStatuses.ShowOnMapStatuses.Contains(value.IBSStatusId))
                    {
                        coord = new CLLocationCoordinate2D(value.VehicleLatitude.Value, value.VehicleLongitude.Value);
                    }
					_taxiLocationPin = new AddressAnnotation(coord, AddressAnnotationType.Taxi, Localize.GetValue("TaxiMapTitle"), value.VehicleNumber, _useThemeColorForPickupAndDestinationMapIcons, _showAssignedVehicleNumberOnPin);
                    AddAnnotation(_taxiLocationPin);
                }
                SetNeedsDisplay();
            }
        }

        public IEnumerable<AvailableVehicle> AvailableVehicles
        {
            set
            {
                ShowAvailableVehicles (Clusterize(value.ToArray()));
            }
        }

        private void SetZoom(IEnumerable<CoordinateViewModel> adressesToDisplay)
        {
            var coordinateViewModels = adressesToDisplay as CoordinateViewModel[] ?? adressesToDisplay.ToArray();
            if(adressesToDisplay == null || !coordinateViewModels.Any())
            {
                return;
            }

            var region = new MKCoordinateRegion();
            double? deltaLat = null;
            double? deltaLng = null;
            CLLocationCoordinate2D center;

            if (coordinateViewModels.Count() == 1)
            {
                var lat = coordinateViewModels.ElementAt(0).Coordinate.Latitude;
                var lon = coordinateViewModels.ElementAt(0).Coordinate.Longitude;

                if (coordinateViewModels.ElementAt(0).Zoom == ZoomLevel.DontChange)
                {
                    region = Region;
                }
                else
                {
                    deltaLat = 0.004;
                    deltaLng = 0.004;
                }

                center = new CLLocationCoordinate2D(lat, lon);
            }
            else
            {
                var minLat = coordinateViewModels.Min(a => a.Coordinate.Latitude);
                var maxLat = coordinateViewModels.Max(a => a.Coordinate.Latitude);
                var minLon = coordinateViewModels.Min(a => a.Coordinate.Longitude);
                var maxLon = coordinateViewModels.Max(a => a.Coordinate.Longitude);

                deltaLat = (Math.Abs(maxLat - minLat)) * 1.5;
                deltaLng = (Math.Abs(maxLon - minLon)) * 1.5;
                center = new CLLocationCoordinate2D((maxLat + minLat) / 2, (maxLon + minLon) / 2);
            }

            SetRegionAndZoom(region, center, deltaLat, deltaLng);
        }

        private void SetRegionAndZoom(MKCoordinateRegion region, CLLocationCoordinate2D center, double? deltaLat, double? deltaLng)
        {
            region.Center = center;
            if (deltaLat.HasValue && deltaLng.HasValue)
            {
                region.Span = new MKCoordinateSpan(deltaLat.Value, deltaLng.Value);
            }
            SetRegion(region, true);
            RegionThatFits(region);
        }

        private void CancelInitialZooming()
        {
            if (_cancelToken != null && _cancelToken.Token.CanBeCanceled)
            {
                _cancelToken.Cancel();
                _cancelToken.Dispose();
                _cancelToken = null;
            }
        }

        public override void SetRegion(MKCoordinateRegion region, bool animated)
        {
            CancelInitialZooming();
            base.SetRegion(region, animated);
        }

        private void ShowDropOffPin (Address address)
        {
            if (_dropoffPin != null) 
            {
                RemoveAnnotation (_dropoffPin);
                _dropoffPin = null;
            }

            if (address == null)
            {
                return;
            }
                
            var coords = address.GetCoordinate();
            if (coords.Latitude != 0 && coords.Longitude != 0) 
            {
                _dropoffPin = new AddressAnnotation(coords, AddressAnnotationType.Destination, Localize.GetValue("DestinationMapTitle"), address.Display(), _useThemeColorForPickupAndDestinationMapIcons, false);
                AddAnnotation (_dropoffPin);
            }
        }

        private void ShowPickupPin (Address address)
        {
            if (_pickupPin != null) 
            {
                RemoveAnnotation (_pickupPin);
                _pickupPin = null;
            }

            if (address == null)
            {
                return;
            }
                
            var coords = address.GetCoordinate();
            if (coords.Latitude != 0 && coords.Longitude != 0)
            {
                _pickupPin = new AddressAnnotation(coords, AddressAnnotationType.Pickup, Localize.GetValue("PickupMapTitle"), address.Display(), _useThemeColorForPickupAndDestinationMapIcons, false);
                AddAnnotation (_pickupPin);
            }
        }

        private readonly List<MKAnnotation> _availableVehiclePushPins = new List<MKAnnotation> ();
        private void ShowAvailableVehicles(IEnumerable<AvailableVehicle> vehicles)
        {
            // remove currently displayed pushpins
            foreach (var pp in _availableVehiclePushPins)
            {
                RemoveAnnotation(pp);
            }

            _availableVehiclePushPins.Clear ();

            if (vehicles == null)
            {
                return;
            }

            foreach (var v in vehicles)
            {
                var annotationType = (v is AvailableVehicleCluster)
                                     ? AddressAnnotationType.NearbyTaxiCluster
                                     : AddressAnnotationType.NearbyTaxi;

                var pushPin = new AddressAnnotation (
                                new CLLocationCoordinate2D(v.Latitude, v.Longitude),
                                annotationType,
                                string.Empty,
                                string.Empty, 
                                _useThemeColorForPickupAndDestinationMapIcons,
								false,
                                v.LogoName);

                AddAnnotation (pushPin);
                _availableVehiclePushPins.Add (pushPin);
            }
        }

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
                    var rect = new CGRect (Bounds.X + colIndex * clusterWidth, Bounds.Y + rowIndex * clusterHeight, clusterWidth, clusterHeight);

                    var vehiclesInRect = list.Where (v => rect.Contains (ConvertCoordinate (new CLLocationCoordinate2D (v.Latitude, v.Longitude), this))).ToArray ();
                    if (vehiclesInRect.Length > cellThreshold)
                    {
                        var clusterBuilder = new VehicleClusterBuilder ();
                        foreach (var v in vehiclesInRect)
                        {
                            clusterBuilder.Add (v);
                        }
                        result.Add (clusterBuilder.Build ());
                    }
                    else
                    {
                        result.AddRange (vehiclesInRect);
                    }
                    foreach (var v in vehiclesInRect)
                    {
                        list.Remove (v);
                    }
                }
            }
            return result.ToArray();
        }
    }
}

