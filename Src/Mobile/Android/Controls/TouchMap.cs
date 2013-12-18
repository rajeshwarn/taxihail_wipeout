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
using Cirrious.MvvmCross.Interfaces.Commands;
using apcurium.MK.Booking.Mobile.Client.Converters;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.Client.Diagnostic;
using System.Reactive.Linq;
using Android.Widget;
using Android.Graphics;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Text;
using apcurium.MK.Booking.Mobile.Client.Helpers;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
	public class TouchMap : Android.Gms.Maps.MapView
    {
		private readonly Stack<Action> _deferedMapActions = new Stack<Action>();
		private bool _mapReady = false;

        public event EventHandler MapTouchUp;

        public IMvxCommand MapMoved { get; set; }

        private Address _pickup;
        private Address _dropoff;
		private ImageView _pickupCenterPin;
		private ImageView _dropoffCenterPin;

       public TouchMap(Context context)
            : base(context)
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

		public void SetMapReady()
		{
			_mapReady = true;
			while (_deferedMapActions.Count > 0)
			{
				_deferedMapActions.Pop().Invoke();
			}
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
            }

            return base.DispatchTouchEvent(e);
        }

        private bool IsMapTouchDown { get; set; }

		private async void ExecuteCommand()
        {
            CancelMoveMap();

            _moveMapCommand = new CancellationTokenSource();

			try
			{
				await Task.Delay(500, _moveMapCommand.Token);
			}
			catch(TaskCanceledException e)
			{
				Logger.LogMessage(e.Message);
			}

			if (!IsMapTouchDown && (MapMoved != null) && (MapMoved.CanExecute()))
			{
				var center = Map.CameraPosition.Target;
				MapMoved.Execute(new Address
					{
						Latitude =  center.Latitude,
						Longitude = center.Longitude
					});
			}
        }

		private Marker _pickupPin;
		private Marker _dropoffPin;

		public Address Pickup
		{
			get { return _pickup; }
			set
			{ 
				_pickup = value;
				DeferWhenMapReady(() =>
				{
						if(AddressSelectionMode == AddressSelectionMode.None)
					{
						ShowPickupPin(value);
					}
				});
			}
		}
		
		public Address Dropoff
		{
			get { return _dropoff; }
			set
			{ 
				_dropoff = value;
				DeferWhenMapReady(() =>
				{
					if (AddressSelectionMode == AddressSelectionMode.None)
					{
						ShowDropoffPin(value);
					}
				});
			}
		}

		private Marker _taxiLocationPin;

        private OrderStatusDetail _taxiLocation;
        public OrderStatusDetail TaxiLocation
        {
            get { return _taxiLocation; }
            set
            {
				_taxiLocation = value;
				DeferWhenMapReady(() =>
				{
					if (_taxiLocationPin != null)
					{
						_taxiLocationPin.Remove();
					}

					if (value != null
							&& value.VehicleLatitude.HasValue 
							&& value.VehicleLongitude.HasValue
							&& !string.IsNullOrEmpty(value.VehicleNumber)
							&& VehicleStatuses.ShowOnMapStatuses.Contains(value.IBSStatusId))
					{
						_taxiLocationPin = Map.AddMarker(new MarkerOptions()
							.Anchor(.5f, 1f)
							.SetPosition(new LatLng(value.VehicleLatitude.Value, value.VehicleLongitude.Value))
							.InvokeIcon(BitmapDescriptorFactory.FromBitmap(CreateTaxiBitmap(value.VehicleNumber)))
							.Visible(false));
					}
					PostInvalidateDelayed(100);
				});

            }
        }

        private IEnumerable<CoordinateViewModel> _center;
        public IEnumerable<CoordinateViewModel> Center
        {
            get { return _center; }
            set
            {
                _center = value;                
				if(value!= null)
                {
					SetZoom(value.ToArray());                   
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
				DeferWhenMapReady(() =>
				{
					if (_addressSelectionMode == Data.AddressSelectionMode.PickupSelection)
					{
						if (_pickupCenterPin != null)
						{
							_pickupCenterPin.Visibility = ViewStates.Visible;
						}
						if (_pickupPin != null)
						{
							_pickupPin.Visible = false;
						}

						ShowDropoffPin(Dropoff);
						PostInvalidateDelayed(100);
					}
					else if (_addressSelectionMode == Data.AddressSelectionMode.DropoffSelection)
					{
						if (_dropoffCenterPin != null)
						{
							_dropoffCenterPin.Visibility = ViewStates.Visible;
						}
						if (_dropoffPin != null)
						{
							_dropoffPin.Visible = false;
						}

						ShowPickupPin(Pickup);
						PostInvalidateDelayed(100);
					}
					else
					{
						ShowDropoffPin(Dropoff);
						ShowPickupPin(Pickup);
						PostInvalidateDelayed(100);
					}
				});
			}
		}

		private void SetZoom(CoordinateViewModel[] adressesToDisplay)
        {
			if ( adressesToDisplay.Length == 1)
            {
				double lat = adressesToDisplay[0].Coordinate.Latitude;
				double lon = adressesToDisplay[0].Coordinate.Longitude;

				if (adressesToDisplay[0].Zoom != ViewModels.ZoomLevel.DontChange)
				{
					AnimateTo(lat, lon, 18);
				}
				else
				{
					AnimateTo(lat, lon);
				}
                return;
            }

			double minLat = 90;
			double maxLat = -90;
			double minLon = 180;
			double maxLon = -180;

            foreach (var item in adressesToDisplay)
            {
				double lat = item.Coordinate.Latitude;
				double lon = item.Coordinate.Longitude;
                maxLat = Math.Max(lat, maxLat);
                minLat = Math.Min(lat, minLat);
                maxLon = Math.Max(lon, maxLon);
                minLon = Math.Min(lon, minLon);
            }

            if ((Math.Abs(maxLat - minLat) < 0.004) && (Math.Abs(maxLon - minLon) < 0.004))
            {
				AnimateTo((maxLat + minLat) / 2, (maxLon + minLon) / 2, 18);
            }
            else
            {
				Map.AnimateCamera(CameraUpdateFactory.NewLatLngBounds(new LatLngBounds(new LatLng(minLat, minLon), new LatLng(maxLat, maxLon)), DrawHelper.GetPixels(100)));
            }
            PostInvalidateDelayed(100);
        }

		private void ShowDropoffPin (Address address)
		{
			if (_dropoffPin == null)
			{
				_dropoffPin = Map.AddMarker(new MarkerOptions()
					.SetPosition(new LatLng(0,0))
					.Anchor(.5f, 1f)
					.InvokeIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.pin_destination))
					.Visible(false));
			}

			if(address == null)
				return;

			if (address.Latitude != 0 && address.Longitude != 0)
			{
				_dropoffPin.Position = new LatLng(address.Latitude, address.Longitude);
				_dropoffPin.Visible = true;
			}
			if(_dropoffCenterPin!= null) _dropoffCenterPin.Visibility = ViewStates.Gone;
			
		}
		
		private void ShowPickupPin (Address address)
		{
			if (_pickupPin == null)
			{
				_pickupPin = Map.AddMarker(new MarkerOptions()
					.SetPosition(new LatLng(0,0))
					.Anchor(.5f, 1f)
					.InvokeIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.pin_hail))
					//.InvokeIcon(BitmapDescriptorFactory.FromBitmap(CreateTaxiBitmap("1234")))
					.Visible(false));
			}

			if(address == null)
				return;

			if (address.Latitude != 0 && address.Longitude != 0)
			{
				_pickupPin.Position = new LatLng(address.Latitude, address.Longitude);
				_pickupPin.Visible = true;
			}
			if(_pickupCenterPin!= null) _pickupCenterPin.Visibility = ViewStates.Gone;
		}

		readonly IList<Marker> _availableVehiclePushPins = new List<Marker> ();
		private void ShowAvailableVehicles(AvailableVehicle[] vehicles)
		{
			// remove currently displayed pushpins
			foreach (var marker in _availableVehiclePushPins)
			{
				marker.Remove();
				marker.Dispose();
			}
			_availableVehiclePushPins.Clear ();

			if (vehicles == null)
				return;

			foreach (var v in vehicles)
			{
                var resId = (v is AvailableVehicleCluster) ? Resource.Drawable.pin_cluster : Resource.Drawable.nearby_cab;
				var pushPin = Map.AddMarker(new MarkerOptions()
					.Anchor(.5f, 1f)
					.SetPosition(new LatLng(v.Latitude, v.Longitude))
					.InvokeIcon(BitmapDescriptorFactory.FromResource(resId))
					.Visible(true));

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
			var vehicleLocationInScreen = Map.Projection.ToScreenLocation(new LatLng(vehicle.Latitude, vehicle.Longitude));

			return rect.Contains(vehicleLocationInScreen.X, vehicleLocationInScreen.Y);
        }

		private void AnimateTo(double lat, double lng, float zoom)
		{
			Map.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(new LatLng(lat, lng), zoom));
		}

		private void AnimateTo(double lat, double lng)
		{
			Map.AnimateCamera(CameraUpdateFactory.NewLatLng(new LatLng(lat, lng)));
		}



		private void DeferWhenMapReady(Action action)
		{
			if (_mapReady)
			{
				action.Invoke();
			}
			else
			{
				_deferedMapActions.Push(action);
			}
		}

		private Bitmap CreateTaxiBitmap(string vehicleNumber)
		{
			var textSize = DrawHelper.GetPixels(12);
			var textVerticalOffset = DrawHelper.GetPixels(3);
			var taxiIcon = BitmapFactory.DecodeResource(this.Resources, Resource.Drawable.pin_cab);

			/* Find the width and height of the title*/
			TextPaint paintText = new TextPaint(PaintFlags.AntiAlias | PaintFlags.LinearText);
			Paint paintRect = new Paint();
			paintText.SetARGB(255, 0, 0, 0);
			paintText.SetTypeface( AppFonts.Bold );
			paintText.TextSize = textSize;
			paintText.TextAlign = Paint.Align.Center;

			Rect rect = new Rect();
			paintText.GetTextBounds(vehicleNumber, 0, vehicleNumber.Length, rect);

			var mutableBitmap = taxiIcon.Copy(taxiIcon.GetConfig(), true);
			var canvas = new Canvas(mutableBitmap);
			canvas.DrawText(vehicleNumber, canvas.Width / 2, rect.Height() + textVerticalOffset, paintText);
			return mutableBitmap;
		}

    }
}
