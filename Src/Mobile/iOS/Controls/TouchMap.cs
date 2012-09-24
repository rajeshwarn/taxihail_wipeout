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

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    [Register ("TouchMap")]
    public class TouchMap : MKMapView
    {
        private TouchGesture _gesture;

        public event EventHandler MapTouchUp;




        private IMvxCommand _mapMoved;

        private Address _pickup;
        private Address _dropoff;
        
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

        }

        private void InitializeGesture()
        {
            if (_gesture == null)
            {
                _gesture = new TouchGesture();
            
                AddGestureRecognizer(_gesture);
            
                RegionChanged += delegate
                {
                
                    try
                    {
                        if (_gesture.GetLastTouchDelay() < 1000)
                        {
                             if ( ( MapMoved != null ) && ( MapMoved.CanExecute() ) )
                            {
                                MapMoved.Execute(new Address{ Latitude = CenterCoordinate.Latitude , Longitude = CenterCoordinate.Longitude } );
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                
                };
            }
        }

        public IMvxCommand MapMoved 
        { 

            get{ return _mapMoved;} 
            set
            { 
                _mapMoved=value;
                if ( _mapMoved != null )
                {
                    InitializeGesture();
                }
            } 
        }


//      public override void TouchesMoved (NSSet touches, MonoTouch.UIKit.UIEvent evt)
//      {
//          base.TouchesMoved (touches, evt);
////            if (this.Overlays != null)
////            {
////                foreach (var i in this.Overlays.OfType<MKAnnotation>())
////                {
////                    i..RemoveBaloon();
////                }                    
////            }
//      }
//
//      //TouchUp
//      public override void TouchesEnded (NSSet touches, MonoTouch.UIKit.UIEvent evt)
//      {
//          base.TouchesEnded (touches, evt);
//          if (MapTouchUp != null)
//          {
//              MapTouchUp(this, EventArgs.Empty);
//          }
//          
//          if ( (MapMoved != null) && ( MapMoved.CanExecute()  ) )
//          {
//              MapMoved.Execute(new Address { Latitude = CenterCoordinate.Latitude, Longitude = CenterCoordinate.Longitude });
//          }
//
//      }



        private bool IsIntoCircle(double x, double y, double xCircle, double yCircle, double rCircle)
        {
            double dist = Math.Sqrt(Math.Pow(x - xCircle, 2) + Math.Pow(y - yCircle, 2));
            return dist <= rCircle;
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
            
            if (adressesToDisplay.Count() == 1)
            {
                double lat = adressesToDisplay.ElementAt(0).Coordinate.Latitude;
                double lon = adressesToDisplay.ElementAt(0).Coordinate.Longitude;
                region.Center = new CLLocationCoordinate2D(lat, lon);
                if (adressesToDisplay.ElementAt(0).Zoom != ViewModels.ZoomLevel.DontChange)
                {
                    SetRegion(region, true);
                    RegionThatFits(region);
                }
                return;
            }

            var minLat = adressesToDisplay.Min(a => a.Coordinate.Latitude);
            var maxLat = adressesToDisplay.Max(a => a.Coordinate.Latitude);
            var minLon = adressesToDisplay.Min(a => a.Coordinate.Longitude);
            var maxLon = adressesToDisplay.Max(a => a.Coordinate.Longitude);

            region.Center = new CLLocationCoordinate2D((maxLat + minLat) / 2, (maxLon + minLon) / 2);
            region.Span = new MKCoordinateSpan((Math.Abs(maxLat - minLat)), (Math.Abs(maxLon - minLon)));
            SetRegion(region, true);
            RegionThatFits(region);         
        }

    }
}

