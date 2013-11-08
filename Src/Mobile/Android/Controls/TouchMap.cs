using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.GoogleMaps;
using Android.Runtime;
using Android.Util;
using Android.Views;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Client.MapUtitilties;
using Cirrious.MvvmCross.Interfaces.Commands;
using apcurium.MK.Booking.Mobile.Client.Converters;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using System.Reactive.Linq;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Data;
using Android.Graphics;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class TouchMap : MapView
    {
        public event EventHandler MapTouchUp;

        public IMvxCommand MapMoved { get; set; }

        private Address _pickup;
        private Address _dropoff;
		private ImageView _pickupCenterPin;
		private ImageView _dropoffCenterPin;

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

		public void SetMapCenterPins (ImageView pickup, ImageView dropoff)
		{
			_pickupCenterPin = pickup;
			_dropoffCenterPin = dropoff;
			// Since this method is called after the view is databound
			// check if we need to display the pins
			if(AddressSelectionMode == AddressSelectionMode.PickupSelection) _pickupCenterPin.Visibility = ViewStates.Visible;
			if(AddressSelectionMode == AddressSelectionMode.DropoffSelection) _dropoffCenterPin.Visibility = ViewStates.Visible;
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

        public override bool DispatchTouchEvent(MotionEvent e)
        {
            if (e.Action == MotionEventActions.Down)
            {
               IsMapTouchDown = true;
               CancelMoveMap();
            }
            
            if (e.Action == MotionEventActions.Up)
            {
                IsMapTouchDown = false;
                if (MapTouchUp != null)
                {
                    MapTouchUp(this, EventArgs.Empty);
                }
                ExecuteCommand();
            }

            if (e.Action == MotionEventActions.Move)
            {
                CancelMoveMap();
                if (this.Overlays != null)
                {
                    foreach (var i in this.Overlays.OfType<PushPinOverlay>())
                    {
                        i.RemoveBaloon();
                    }                    
                }
            }

            return base.DispatchTouchEvent(e);
        }

        private bool IsMapTouchDown { get; set; }

        private void ExecuteCommand()
        {
            CancelMoveMap();

            _moveMapCommand = new CancellationTokenSource();

            var t = Task.Delay(500, _moveMapCommand.Token);

            t.ContinueWith(r =>
                {
                    if (!r.IsCompleted || r.IsCanceled || r.IsFaulted) return;

                    if (!IsMapTouchDown && (MapMoved != null) && (MapMoved.CanExecute()))
                    {
                        MapMoved.Execute(new Address
                            {
                                Latitude = MapCenter.LatitudeE6.ConvertFromE6(),
                                Longitude = MapCenter.LongitudeE6.ConvertFromE6()
                            });
                    }
                }, _moveMapCommand.Token);
//            t.Start();

          
        }

        private PushPinOverlay _pickupPin;
        private PushPinOverlay _dropoffPin;

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
					ShowDropoffPin(value);
				}
			}
		}

        private TaxiOverlay _taxiLocationPin;

        private OrderStatusDetail _taxiLocation;
        public OrderStatusDetail TaxiLocation
        {
            get { return _taxiLocation; }
            set
            {
                _taxiLocation = value;
                if (_taxiLocationPin != null)
                {
                    this.Overlays.Remove(_taxiLocationPin);
                    _taxiLocationPin = null;
                }

                if ((value != null) && (value.VehicleLatitude.HasValue) && (value.VehicleLongitude.HasValue) && (!string.IsNullOrEmpty(value.VehicleNumber)) && VehicleStatuses.ShowOnMapStatuses.Contains(value.IBSStatusId))
                {
                    var point = new GeoPoint(CoordinatesHelper.ConvertToE6(value.VehicleLatitude.Value), CoordinatesHelper.ConvertToE6(value.VehicleLongitude.Value));
                    _taxiLocationPin = new TaxiOverlay(this, Resources.GetDrawable(Resource.Drawable.pin_cab), value.VehicleNumber, point);
                   this.Overlays.Add(_taxiLocationPin);
                }
                PostInvalidateDelayed(100);
            }
        }

        private IEnumerable<CoordinateViewModel> _center;
        public IEnumerable<CoordinateViewModel> Center
        {
            get { return _center; }
            set
            {
                _center = value;                
                if(Center!= null)
                {
                    SetZoom(Center);                   
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

		private AddressSelectionMode _addressSelectionMode;
		public AddressSelectionMode AddressSelectionMode {
			get {
				return _addressSelectionMode;
			}
			set {
				_addressSelectionMode = value;
				if(_addressSelectionMode == Data.AddressSelectionMode.PickupSelection)
				{
					if(_pickupCenterPin != null) _pickupCenterPin.Visibility = ViewStates.Visible;
					if(_pickupPin != null) this.Overlays.Remove(_pickupPin);
					_pickupPin = null;

					ShowDropoffPin(Dropoff);
                    PostInvalidateDelayed(100);
				}
				else if(_addressSelectionMode == Data.AddressSelectionMode.DropoffSelection)
				{
					if(_dropoffCenterPin != null) _dropoffCenterPin.Visibility = ViewStates.Visible;
					if(_dropoffPin != null) this.Overlays.Remove(_dropoffPin);
					_dropoffPin = null;

					ShowPickupPin(Pickup);
                    PostInvalidateDelayed(100);
				}
				else 
				{
					ShowDropoffPin(Dropoff);
					ShowPickupPin(Pickup);
                    PostInvalidateDelayed(100);
				}
			}
		}

        private void SetZoom(IEnumerable<CoordinateViewModel> adressesToDisplay)
        {
            var mapController = this.Controller;

            if ( adressesToDisplay.Count() == 1)
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
            PostInvalidateDelayed(100);
        }

		private void ShowDropoffPin (Address address)
		{
			if (_dropoffPin != null)
			{
				this.Overlays.Remove(_dropoffPin);
				_dropoffPin = null;
			}

			if(address == null)
				return;

			if (address.Latitude != 0 && address.Longitude != 0)
			{
				_dropoffPin = MapUtitilties.MapService.AddPushPin(this, Resources.GetDrawable(Resource.Drawable.pin_destination), address, address.FullAddress);
			}
			if(_dropoffCenterPin!= null) _dropoffCenterPin.Visibility = ViewStates.Gone;
			
		}
		
		private void ShowPickupPin (Address address)
		{
			if (_pickupPin != null)
			{
				this.Overlays.Remove(_pickupPin);
				_pickupPin = null;
			}
			
			if(address == null)
				return;

			if (address.Latitude != 0 && address.Longitude != 0)
			{
				_pickupPin = MapService.AddPushPin(this, Resources.GetDrawable(Resource.Drawable.pin_hail), address,  address.FullAddress);
			}
			if(_pickupCenterPin!= null) _pickupCenterPin.Visibility = ViewStates.Gone;
		}

		private List<PushPinOverlay> _availableVehiclePushPins = new List<PushPinOverlay> ();
		private void ShowAvailableVehicles(AvailableVehicle[] vehicles)
		{
			// remove currently displayed pushpins
			foreach (var pp in _availableVehiclePushPins)
			{
				this.Overlays.Remove(pp);
			}
			_availableVehiclePushPins.Clear ();

			if (vehicles == null)
				return;

			foreach (var v in vehicles)
			{
                var resId = (v is AvailableVehicleCluster) ? Resource.Drawable.pin_cluster : Resource.Drawable.nearby_cab;
				var pushPin = MapService.AddPushPin (this,
                                       Resources.GetDrawable (resId),
				                       MapService.GetGeoPoint (v.Latitude, v.Longitude),
				                       string.Empty);
				_availableVehiclePushPins.Add (pushPin);
			}
            this.PostInvalidate();
		}

        private AvailableVehicle[] Clusterize(AvailableVehicle[] vehicles)
        {
            // Divide the map in 25 cells (5*5)
            const int numberOfRows = 5;
            const int numberOfColumns = 5;
            // Maximum number of vehicles in a cell before we start displaying a cluster
            const int cellThreshold = 1;

            var result = new List<AvailableVehicle>();

            Rect bounds = new Rect();
            this.GetDrawingRect(bounds);

            var clusterWidth = (bounds.Right - bounds.Left) / numberOfColumns;
            var clusterHeight = (bounds.Bottom - bounds.Top) / numberOfRows;

            var list = new List<AvailableVehicle>(vehicles);

            for (int rowIndex = 0; rowIndex < numberOfRows; rowIndex++)
                for (int colIndex = 0; colIndex < numberOfColumns; colIndex++)
            {
                var top = bounds.Top + rowIndex * clusterHeight;
                var left = bounds.Left + colIndex * clusterWidth;
                var bottom = bounds.Top + (rowIndex + 1) * clusterHeight;
                var right = bounds.Left + (colIndex + 1) * clusterWidth;
                var rect = new Rect(left, top, right, bottom);

                var vehiclesInRect = list.Where(v => IsVehicleInRect(v, rect)).ToArray();
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

        private bool IsVehicleInRect(AvailableVehicle vehicle, Rect rect)
        {
            Point currentDevicePosition = new Point();
            GeoPoint vehicleLocation = new GeoPoint((int) (vehicle.Latitude.ConvertToE6()), (int) (vehicle.Longitude.ConvertToE6()));

            this.Projection.ToPixels(vehicleLocation, currentDevicePosition);

            return rect.Contains(currentDevicePosition.X, currentDevicePosition.Y);

        }
    }
}
