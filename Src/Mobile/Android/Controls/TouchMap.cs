using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Android.Content;
using Android.GoogleMaps;
using Android.Runtime;
using Android.Util;
using Android.Views;
using apcurium.MK.Booking.Mobile.Client.MapUtitilties;
using apcurium.MK.Booking.Api.Contract.Resources;
using Cirrious.MvvmCross.Interfaces.Commands;
using apcurium.MK.Booking.Mobile.Client.Converters;
using apcurium.MK.Booking.Mobile.Extensions;
using TinyIoC;
using apcurium.MK.Booking.Mobile.AppServices;
using Xamarin.Geolocation;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class TouchMap : MapView
    {

        public event EventHandler MapTouchUp;

        public IMvxCommand MapMoved { get; set; }

        private Address _pickup;
        private Address _dropoff;

        protected TouchMap(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
            Initialize();
        }

        public TouchMap(Context context, string apiKey)
            : base(context, apiKey)
        {
            Initialize();
        }

        public TouchMap(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            Initialize();
        }

        public TouchMap(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {
            Initialize();
        }

        private void Initialize()
        {            
        }


        public override bool DispatchTouchEvent(Android.Views.MotionEvent e)
        {
            if (e.Action == MotionEventActions.Up)
            {
                
                if (MapTouchUp != null)
                {
                    MapTouchUp(this, EventArgs.Empty);
                }

                if ( (MapMoved != null) && ( MapMoved.CanExecute()  ) )
                {
                    MapMoved.Execute(new Address { Latitude = CoordinatesHelper.ConvertFromE6(MapCenter.LatitudeE6), Longitude = CoordinatesHelper.ConvertFromE6(MapCenter.LongitudeE6) });
                }
            }

            if (e.Action == MotionEventActions.Move)
            {
                if (this.Overlays != null)
                {
                    foreach (var i in this.Overlays.OfType<PushPinOverlay>())
                    {
                        i.RemoveBaloon();
                    }
                    this.Overlays.Clear();
                }
            }

            Console.WriteLine(e.Action.ToString());
            return base.DispatchTouchEvent(e);
        }

        private bool IsIntoCircle(double x, double y, double xCircle, double yCircle, double rCircle)
        {
            double dist = Math.Sqrt(Math.Pow(x - xCircle, 2) + Math.Pow(y - yCircle, 2));
            return dist <= rCircle;
        }

        private PushPinOverlay _pickupPin;
        private PushPinOverlay _dropoffPin;

        public Address Pickup
        {
            get { return _pickup; }
            set 
            { 
                _pickup = value;
               /* if (_pickupPin != null)
                {
                    this.Overlays.Remove(_pickupPin);
                    _pickupPin = null;
                }*/


                if ((value != null) && (value.Latitude != 0) && (value.Longitude != 0))
                {
                    _pickupPin = MapUtitilties.MapService.AddPushPin(this, Resources.GetDrawable(Resource.Drawable.pin_green), value,  _pickup.FullAddress);
                }

                Invalidate();
            }
        }

        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();
            Console.WriteLine(this.ChildCount.ToString());
        }

        public Address Dropoff
        {
            get { return _dropoff; }
            set 
            { 
                _dropoff = value;
                /*if (_dropoffPin != null)
                {
                    this.Overlays.Remove(_dropoffPin);
                    _dropoffPin = null;
                }*/
                if ((value != null) && (value.Latitude != 0) && (value.Longitude != 0))
                {
                    _dropoffPin = MapUtitilties.MapService.AddPushPin(this, Resources.GetDrawable(Resource.Drawable.pin_red), value, _dropoff.FullAddress);
                }
                Invalidate();
            }
        }

        private bool _isDropoffActive;
        private IEnumerable<CoordinateViewModel> _center;
        public IEnumerable<CoordinateViewModel> Center
        {
            get { return _center; }
            set
            {
                _center = value;                
                
                SetZoom(Center);                   
                
            }
        }

        

        //public bool IsDropoffActive
        //{
        //    get { return _isDropoffActive; }
        //    set 
        //    { 
        //        _isDropoffActive = value;                
        //    }
        //}

        //private bool _isPickupActive;

        //public bool IsPickupActive
        //{
        //    get { return _isPickupActive; }
        //    set 
        //    { 
        //        _isPickupActive = value;
        //    }
        //}



        //private void RecenterMap()
        //{
        //    if (IsPickupActive && Pickup.HasValidCoordinate() )
        //    {
        //        SetZoom( Pickup );
        //    }
        //    else if ( IsDropoffActive && Dropoff.HasValidCoordinate())
        //    {
        //        SetZoom( Dropoff );
        //    }
        //    else if (!IsDropoffActive && !IsDropoffActive && Pickup.HasValidCoordinate() && Dropoff.HasValidCoordinate())
        //    {
        //        SetZoom(Pickup, Dropoff);
        //    }
        //}

        private void SetZoom(IEnumerable<CoordinateViewModel> adressesToDisplay)
        {
            var map = this;
            var mapController = this.Controller;

            if (adressesToDisplay.Count() == 1)
            {
                int lat = CoordinatesHelper.ConvertToE6(adressesToDisplay.ElementAt(0).Coordinate.Latitude);
                int lon = CoordinatesHelper.ConvertToE6(adressesToDisplay.ElementAt(0).Coordinate.Longitude);
                mapController.AnimateTo(new GeoPoint(lat, lon));
                if (adressesToDisplay.ElementAt(0).Zoom != ViewModels.ZoomLevel.DontChange)
                {
                    mapController.SetZoom(18);
                }
                return;
            }


            int minLat = int.MaxValue;
            int maxLat = int.MinValue;
            int minLon = int.MaxValue;
            int maxLon = int.MinValue;

            foreach (var item in adressesToDisplay)
            {
                int lat = CoordinatesHelper.ConvertToE6(item.Coordinate.Latitude);
                int lon = CoordinatesHelper.ConvertToE6(item.Coordinate.Longitude);
                maxLat = Math.Max(lat, maxLat);
                minLat = Math.Min(lat, minLat);
                maxLon = Math.Max(lon, maxLon);
                minLon = Math.Min(lon, minLon);
            }

            if ((Math.Abs(maxLat - minLat) < 0.004) && (Math.Abs(maxLon - minLon) < 0.004))
            {
                mapController.AnimateTo(new GeoPoint((maxLat + minLat) / 2, (maxLon + minLon) / 2));
                mapController.SetZoom(18);
            }
            else
            {
                double fitFactor = 1.5;

                mapController.ZoomToSpan((int)(Math.Abs(maxLat - minLat) * fitFactor), (int)(Math.Abs(maxLon - minLon) * fitFactor));
                mapController.AnimateTo(new GeoPoint((maxLat + minLat) / 2, (maxLon + minLon) / 2));
            }

        }

        public override bool OnTouchEvent(Android.Views.MotionEvent e)
        {
            return base.OnTouchEvent(e);
        }


        
        
    }
}
