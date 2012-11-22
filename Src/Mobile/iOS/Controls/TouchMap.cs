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

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    [Register ("TouchMap")]
    public class TouchMap : MKMapView
    {
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
            _cancelToken = new CancellationTokenSource();


            TinyIoCContainer.Current.Resolve<ILocationService>().GetPositionAsync(5000, 4000, 5000, 8000, _cancelToken.Token).ContinueWith(t => {
                if (t.IsCompleted && !t.IsCanceled)
                {
                    InvokeOnMainThread(() =>
                    {
                        if (t.Result.Latitude != 0 && t.Result.Longitude != 0)
                        {
                            SetRegionAndZoom(new MKCoordinateRegion(), new CLLocationCoordinate2D(t.Result.Latitude, t.Result.Longitude), 0.2, 0.2);
                        }
                    });
                }
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
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

        void HandleTouchBegin(object sender, EventArgs e)
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
        
        private MKAnnotation _pickupPin;
        private MKAnnotation _dropoffPin;
        
        public Address Pickup
        {
            get { return _pickup; }
            set
            { 
                _pickup = value;
                if (_pickupPin != null)
                {
                    RemoveAnnotation(_pickupPin);
                    _pickupPin = null;
                }
                
                
                if ((value != null) && (value.Latitude != 0) && (value.Longitude != 0))
                {
                    var coord = _pickup.GetCoordinate();
                    _pickupPin = new AddressAnnotation(coord, AddressAnnotationType.Pickup, Resources.PickupMapTitle, _pickup.Display());
                    AddAnnotation(_pickupPin);
                }
                
                SetNeedsDisplay();
            }
        }
        
        public Address Dropoff
        {
            get { return _dropoff; }
            set
            { 
                _dropoff = value;
                if (_dropoffPin != null)
                {
                    RemoveAnnotation(_dropoffPin);
                    _dropoffPin = null;
                }
                
                if ((value != null) && (value.Latitude != 0) && (value.Longitude != 0))
                {
                    var coord = _dropoff.GetCoordinate();
                    _dropoffPin = new AddressAnnotation(coord, AddressAnnotationType.Destination, Resources.DestinationMapTitle, _dropoff.Display());
                    AddAnnotation(_dropoffPin);
                }
                                                   
                SetNeedsDisplay();
            }
        }
        
        private bool _isDropoffActive;
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
        
        private void SetZoom(IEnumerable<CoordinateViewModel> adressesToDisplay)
        {

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
    }
}

