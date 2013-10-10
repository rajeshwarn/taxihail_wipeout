using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using MonoTouch.MapKit;
using apcurium.MK.Booking.Mobile.Client.MapUtilities;
using apcurium.MK.Booking.Api.Contract.Resources;
using Cirrious.MvvmCross.Interfaces.Commands;
using apcurium.MK.Booking.Mobile.Extensions;
using TinyIoC;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.ViewModels;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.CoreLocation;
using apcurium.MK.Common.Entity;
using System.Threading;
using apcurium.MK.Booking.Mobile.Infrastructure;
using MonoTouch.UIKit;
using System.Threading.Tasks;
using apcurium.MK.Common;
using MonoTouch.CoreGraphics;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    [Register ("TouchMap")]
    public class TouchMap : MKMapView
    {
        private UIImageView _pickupCenterPin;
        private UIImageView _dropoffCenterPin;

        private TouchGesture _gesture;
        private IMvxCommand _mapMoved;
        private Address _pickup;
        private Address _dropoff;
        private CancellationTokenSource _cancelToken;
        
        protected TouchMap(RectangleF rect)
            : base(rect)
        {
            Initialize();
        }

        public TouchMap(IntPtr handle)
            : base(handle)
        {
            Initialize();
        }
        
        public TouchMap(NSObjectFlag flag)
            : base(flag)
        {
            Initialize();
        }
        
        public TouchMap()
            : base()
        {
            Initialize();
        }

        public TouchMap(NSCoder coder)
            : base(coder)
        {
            Initialize();
        }



        private void Initialize()
        {   
           /// _cancelToken = new CancellationTokenSource();

            ShowsUserLocation = true;
            if (_pickupCenterPin == null) {
                _pickupCenterPin = new UIImageView(new UIImage(AddressAnnotation.GetImageFilename(AddressAnnotationType.Pickup)));
                _pickupCenterPin.BackgroundColor = UIColor.Clear;
                _pickupCenterPin.ContentMode = UIViewContentMode.Center;
                AddSubview(_pickupCenterPin);
                _pickupCenterPin.Hidden = true;                
            }
            if (_dropoffCenterPin == null) {
                _dropoffCenterPin = new UIImageView(new UIImage(AddressAnnotation.GetImageFilename(AddressAnnotationType.Destination)));
                _dropoffCenterPin.BackgroundColor = UIColor.Clear;
                _dropoffCenterPin.ContentMode = UIViewContentMode.Center;
                AddSubview(_dropoffCenterPin);
                
                _dropoffCenterPin.Hidden = true;
            }
        }

        public override void LayoutSubviews ()
        {
            base.LayoutSubviews ();



            var p = this.ConvertCoordinate(this.CenterCoordinate,this);
            _pickupCenterPin.Frame = new RectangleF(p.X - 21, p.Y - 57, 42, 57); //Image is 42 x 57           
            _dropoffCenterPin.Frame = new RectangleF(p.X - 21, p.Y - 57, 42, 57);
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
            catch
            {

            }
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

        private CancellationTokenSource _moveMapCommand;

        void CancelMoveMap()
        {
            if ((_moveMapCommand != null) && _moveMapCommand.Token.CanBeCanceled)
            {
                _moveMapCommand.Cancel();
                _moveMapCommand.Dispose();
                _moveMapCommand = null;
            }
        }

        void ExecuteMoveMapCommand()
        {
            CancelMoveMap();

            _moveMapCommand = new CancellationTokenSource();
                                
            var t = new Task(() =>
                             {
                Thread.Sleep(500);
            }, _moveMapCommand.Token );

            t.ContinueWith(r =>
            {
                if (r.IsCompleted && !r.IsCanceled && !r.IsFaulted)
                {
                    InvokeOnMainThread(() =>
                    {
                        if ((MapMoved != null) && (MapMoved.CanExecute()))
                        {
                            Console.WriteLine("MapMovedMapMovedMapMovedMapMovedMapMovedMapMovedMapMoved");
                            MapMoved.Execute(new Address {            Latitude = CenterCoordinate.Latitude,              Longitude = CenterCoordinate.Longitude });
                        }
                    });
                }        
            },_moveMapCommand.Token );
            t.Start();
        }

        void HandleTouchBegin (object sender, EventArgs e)
        {
            CancelMoveMap();
        }
               
        public IMvxCommand MapMoved
        {
            get{ return _mapMoved;} 
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
        public AddressSelectionMode AddressSelectionMode {
            get {
                return _addressSelectionMode;
            }
            set {
                _addressSelectionMode = value;
                if(_addressSelectionMode == Data.AddressSelectionMode.PickupSelection)
                {
                    _pickupCenterPin.Hidden = false;
                    if(_pickupPin != null) RemoveAnnotation(_pickupPin);
                    _pickupPin = null;

                    ShowDropOffPin(Dropoff);
                    SetNeedsDisplay();
                }
                else if(_addressSelectionMode == Data.AddressSelectionMode.DropoffSelection)
                {
                    _dropoffCenterPin.Hidden = false;
                    if(_dropoffPin != null) RemoveAnnotation(_dropoffPin);
                    _dropoffPin = null;

                    ShowPickupPin(Pickup);
                    SetNeedsDisplay();
                }
                else
                {
                    ShowDropOffPin(Dropoff);
                    ShowPickupPin(Pickup);
                    SetNeedsDisplay();
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
                if(this.AddressSelectionMode == Data.AddressSelectionMode.None)
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
                if(this.AddressSelectionMode == Data.AddressSelectionMode.None)
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

        private OrderStatusDetail _taxiLocation { get; set; }
        
        private MKAnnotation _taxiLocationPin;
        
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
                    CLLocationCoordinate2D coord = new CLLocationCoordinate2D(0,0);            
                    if (value.VehicleLatitude.HasValue
                        && value.VehicleLongitude.HasValue
                        && value.VehicleLongitude.Value !=0 
                        && value.VehicleLatitude.Value !=0
                        && !string.IsNullOrEmpty(value.VehicleNumber) 
                        && VehicleStatuses.ShowOnMapStatuses.Contains(value.IBSStatusId))
                    {
                        coord = new CLLocationCoordinate2D( value.VehicleLatitude.Value , value.VehicleLongitude.Value );
                    }   
                    _taxiLocationPin = new AddressAnnotation (coord, AddressAnnotationType.Taxi, Resources.TaxiMapTitle, value.VehicleNumber);
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
                //ShowAvailableVehicles (value.ToArray());
            }
        }

        
        private void SetZoom(IEnumerable<CoordinateViewModel> adressesToDisplay)
        {
            if(adressesToDisplay == null || !adressesToDisplay.Any())
            {
                return;
            }

            var region = new MKCoordinateRegion();
            double? deltaLat = null;
            double? deltaLng = null;
            CLLocationCoordinate2D center;

            if (adressesToDisplay.Count() == 1)
            {
                double lat = adressesToDisplay.ElementAt(0).Coordinate.Latitude;
                double lon = adressesToDisplay.ElementAt(0).Coordinate.Longitude;

                if (adressesToDisplay.ElementAt(0).Zoom == ViewModels.ZoomLevel.DontChange)
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
                var minLat = adressesToDisplay.Min(a => a.Coordinate.Latitude);
                var maxLat = adressesToDisplay.Max(a => a.Coordinate.Latitude);
                var minLon = adressesToDisplay.Min(a => a.Coordinate.Longitude);
                var maxLon = adressesToDisplay.Max(a => a.Coordinate.Longitude);

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

        public override void SubviewAdded(MonoTouch.UIKit.UIView uiview)
        {
            base.SubviewAdded(uiview);

            if (Subviews != null)
            {

                UIView legalView = null;

                foreach (var subview in Subviews)
                {
                    if (subview is UILabel)
                    { 
                        legalView = subview;
                    }
                    else if (subview is UIImageView)
                    {
                        // google image iOS 5 and lower
                        legalView = subview;
                    }
                }
                if (legalView != null)
                {
                    legalView.ToString();
                    legalView.Frame = new RectangleF(legalView.Frame.X, legalView.Frame.Y - 60, legalView.Frame.Width, legalView.Frame.Height);
                    //legalView.Frame = 
                }
            }

            
        }

        private void ShowDropOffPin (Address address)
        {
            if (_dropoffPin != null) {
                RemoveAnnotation (_dropoffPin);
                _dropoffPin = null;
            }

            if(address == null)
                return;
            var coords = address.GetCoordinate();
            if (coords.Latitude != 0 && coords.Longitude != 0) {
                _dropoffPin = new AddressAnnotation (coords, AddressAnnotationType.Destination, Resources.DestinationMapTitle, address.Display ());
                AddAnnotation (_dropoffPin);
            }
            if( _dropoffCenterPin!= null) _dropoffCenterPin.Hidden = true;

        }

        private void ShowPickupPin (Address address)
        {
            if (_pickupPin != null) {
                RemoveAnnotation (_pickupPin);
                _pickupPin = null;
            }

            if(address == null)
                return;
            var coords = address.GetCoordinate();
            if (coords.Latitude != 0 && coords.Longitude != 0) {
                _pickupPin = new AddressAnnotation (coords, AddressAnnotationType.Pickup, Resources.PickupMapTitle, address.Display ());
                AddAnnotation (_pickupPin);
            }
            if(_pickupCenterPin != null) _pickupCenterPin.Hidden = true;
        }

        private List<MKAnnotation> _availableVehiclePushPins = new List<MKAnnotation> ();
        private void ShowAvailableVehicles(IEnumerable<AvailableVehicle> vehicles)
        {
            // remove currently displayed pushpins
            foreach (var pp in _availableVehiclePushPins)
            {
                RemoveAnnotation(pp);
            }
            _availableVehiclePushPins.Clear ();

            if (vehicles == null)
                return;

            foreach (var v in vehicles)
            {
                var annotationType = (v is AvailableVehicleCluster) ? AddressAnnotationType.NearbyTaxiCluster : AddressAnnotationType.NearbyTaxi;
                var pushPin = new AddressAnnotation (new CLLocationCoordinate2D(v.Latitude, v.Longitude), annotationType, string.Empty, string.Empty);
                AddAnnotation (pushPin);
                _availableVehiclePushPins.Add (pushPin);
            }



        }

        private AvailableVehicle[] Clusterize(AvailableVehicle[] vehicles)
        {
            // Divide the map in 16 cells (4*4)
            const int numberOfRows = 4;
            const int numberOfColumns = 4;
            // Maximum number of vehicles in a cell before we start displaying a cluster
            const int cellThreshold = 4;

            var result = new List<AvailableVehicle>();

            var bounds =  this.Bounds;
            var clusterWidth = bounds.Width / numberOfColumns;
            var clusterHeight = bounds.Height / numberOfRows;

            var list = new List<AvailableVehicle>(vehicles);

            for (int rowIndex = 0; rowIndex < numberOfRows; rowIndex++)
                for (int colIndex = 0; colIndex < numberOfColumns; colIndex++)
                {
                    var rect = new RectangleF(this.Bounds.X + (colIndex + 1) * clusterWidth, this.Bounds.Y + (rowIndex + 1) * clusterHeight, clusterWidth, clusterHeight);

                    var vehiclesInRect = list.Where(v => rect.Contains(this.ConvertCoordinate(new CLLocationCoordinate2D(v.Latitude, v.Longitude), this))).ToArray();
                    if (vehiclesInRect.Length > cellThreshold)
                    {
                        var clusterBuilder = new VehicleClusterBuilder();
                        foreach(var v in vehiclesInRect) clusterBuilder.Add(v);
                        result.Add(clusterBuilder.Build());
                    }
                    else
                    {
                        result.AddRange(vehiclesInRect);
                    }
                    foreach(var v in vehiclesInRect) list.Remove(v);

                }
            return result.ToArray();
        }
    }

    public class AvailableVehicleCluster: AvailableVehicle
    {

    }

    public class VehicleClusterBuilder
    {
        private readonly List<AvailableVehicle> _vehicles = new List<AvailableVehicle>();
        public void Add(AvailableVehicle vehicle)
        {
            if (vehicle == null) throw new ArgumentNullException();

            _vehicles.Add(vehicle);
        }

        public bool IsEmpty { get { return _vehicles.Count == 0; } }

        public AvailableVehicle Build()
        {
            return new AvailableVehicleCluster
            {
                Latitude = IsEmpty
                            ? default(double)
                            : _vehicles.Sum(x => x.Latitude) / _vehicles.Count,
                Longitude = IsEmpty
                            ? default(double)
                            : _vehicles.Sum(x => x.Longitude) / _vehicles.Count,
            };
        }

    }
}

