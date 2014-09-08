using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Client.Diagnostic;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using ServiceStack.Common.Utils;
using TinyIoC;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class TouchMap : MapView
    {
        private readonly IList<Marker> _availableVehiclePushPins = new List<Marker>();
        private readonly Stack<Action> _deferedMapActions = new Stack<Action>();
        private AddressSelectionMode _addressSelectionMode;
        private IEnumerable<CoordinateViewModel> _center;
        private Address _dropoff;
        private ImageView _dropoffCenterPin;
        private Marker _dropoffPin;
        private bool _mapReady;
        private CancellationTokenSource _moveMapCommand;

        private Address _pickup;
        private ImageView _pickupCenterPin;
        private Marker _pickupPin;
        private OrderStatusDetail _taxiLocation;
        private Marker _taxiLocationPin;

        private BitmapDescriptor _destinationIcon;
        private BitmapDescriptor _hailIcon;

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

// ReSharper disable once UnusedAutoPropertyAccessor.Global
        public ICommand MapMoved { get; set; }

		public ICommand CancelCalculateFareEstimate { get; set; }

        private bool IsMapTouchDown { get; set; }

        public Address Pickup
        {
            get { return _pickup; }
            set
            {
                _pickup = value;
                DeferWhenMapReady(() =>
                {
                    if (AddressSelectionMode == AddressSelectionMode.None)
                    {
                        ShowPickupPin(value);
                    }
                });
            }
        }

        public Address DropOff
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
                            .Visible(true));
                    }
                    PostInvalidateDelayed(100);
                });
            }
        }

        public IEnumerable<CoordinateViewModel> Center
        {
            get { return _center; }
            set
            {
                _center = value;
                if (value != null)
                {
                    DeferWhenMapReady(() => { SetZoom(value.ToArray()); });
                }
            }
        }

        public IEnumerable<AvailableVehicle> AvailableVehicles
        {
            set
            {
                DeferWhenMapReady(
                    () =>
                    {
                        ShowAvailableVehicles(Clusterize((value ?? Enumerable.Empty<AvailableVehicle>()).ToArray()));
                    });
            }
        }

        public AddressSelectionMode AddressSelectionMode
        {
            get { return _addressSelectionMode; }
            set
            {
                _addressSelectionMode = value;
                DeferWhenMapReady(() =>
                {
                    if (_addressSelectionMode == AddressSelectionMode.PickupSelection)
                    {
                        if (_pickupCenterPin != null)
                        {
                            _pickupCenterPin.Visibility = ViewStates.Visible;
                        }
                        if (_pickupPin != null)
                        {
                            _pickupPin.Visible = false;
                        }

                        ShowDropoffPin(DropOff);
                        PostInvalidateDelayed(100);
                    }
                    else if (_addressSelectionMode == AddressSelectionMode.DropoffSelection)
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
                        ShowDropoffPin(DropOff);
                        ShowPickupPin(Pickup);
                        PostInvalidateDelayed(100);
                    }
                });
            }
        }

        public event EventHandler MapTouchUp;

        private void Initialize()
        {
			var useColor = this.Services().Settings.UseThemeColorForMapIcons;
            var colorBgTheme = useColor ? (Color?)Resources.GetColor(Resource.Color.company_color) : (Color?)null;

            var destinationIcon =  Resources.GetDrawable(Resource.Drawable.@destination_icon);
            _destinationIcon = DrawHelper.DrawableToBitmapDescriptor(destinationIcon, colorBgTheme);

            var hailIcon = Resources.GetDrawable(Resource.Drawable.@hail_icon);       
            _hailIcon = DrawHelper.DrawableToBitmapDescriptor(hailIcon, colorBgTheme);
        }

        public void SetMapReady()
        {
            _mapReady = true;
            while (_deferedMapActions.Count > 0)
            {
                _deferedMapActions.Pop().Invoke();
            }
        }

        public void SetMapCenterPins(ImageView pickup, ImageView dropoff)
        {
            _pickupCenterPin = pickup;
            _dropoffCenterPin = dropoff;
            // Since this method is called after the view is databound
            // check if we need to display the pins
            if (AddressSelectionMode == AddressSelectionMode.PickupSelection)
                _pickupCenterPin.Visibility = ViewStates.Visible;
            if (AddressSelectionMode == AddressSelectionMode.DropoffSelection)
                _dropoffCenterPin.Visibility = ViewStates.Visible;
        }

        private void CancelMoveMap()
        {
            if ((_moveMapCommand != null) && _moveMapCommand.Token.CanBeCanceled)
            {
                _moveMapCommand.Cancel();
                _moveMapCommand.Dispose();
                _moveMapCommand = null;
            }

			CancelCalculateFareEstimate.Execute (null);
        }

        public override bool DispatchTouchEvent(MotionEvent e)
        {
            if (Map != null)
            {
                switch (e.Action)
                {
                    case MotionEventActions.Down:
                        IsMapTouchDown = true;
                        CancelMoveMap();
                        break;
                    case MotionEventActions.Up:
                        IsMapTouchDown = false;
                        if (MapTouchUp != null)
                        {
                            MapTouchUp(this, EventArgs.Empty);
                        }
                        ExecuteCommand();
                        break;
                    case MotionEventActions.Move:
                        CancelMoveMap();
                        break;
                }
            }

            return base.DispatchTouchEvent(e);
        }

        private async void ExecuteCommand()
        {
            CancelMoveMap();

            _moveMapCommand = new CancellationTokenSource();

            try
            {
                await Task.Delay(500, _moveMapCommand.Token);
            }
            catch (TaskCanceledException e)
            {
                Logger.LogMessage(e.Message);
            }

            if (!IsMapTouchDown && (MapMoved != null) && (MapMoved.CanExecute()))
            {
                var center = Map.CameraPosition.Target;
                MapMoved.Execute(new Address
                {
                    Latitude = center.Latitude,
                    Longitude = center.Longitude
                });
            }
        }

        private void SetZoom(CoordinateViewModel[] adressesToDisplay)
        {
            if (adressesToDisplay.Length == 1)
            {
                double lat = adressesToDisplay[0].Coordinate.Latitude;
                double lon = adressesToDisplay[0].Coordinate.Longitude;

                if (adressesToDisplay[0].Zoom != ZoomLevel.DontChange)
                {
                    AnimateTo(lat, lon, 16);
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
                AnimateTo((maxLat + minLat)/2, (maxLon + minLon)/2, 16);
            }
            else
            {
                Map.AnimateCamera(
                    CameraUpdateFactory.NewLatLngBounds(
                        new LatLngBounds(new LatLng(minLat, minLon), new LatLng(maxLat, maxLon)),
                        DrawHelper.GetPixels(100)));
            }
            PostInvalidateDelayed(100);
        }

        private void ShowDropoffPin(Address address)
        {
            if (_dropoffPin == null)
            {
                _dropoffPin = Map.AddMarker(new MarkerOptions()
                    .SetPosition(new LatLng(0, 0))
                    .Anchor(.5f, 1f)
                    .InvokeIcon(_destinationIcon)
                    .Visible(false));
            }

            if (address == null)
                return;

// ReSharper disable CompareOfFloatsByEqualityOperator
            if (address.Latitude != 0 && address.Longitude != 0)
// ReSharper restore CompareOfFloatsByEqualityOperator
            {
                _dropoffPin.Position = new LatLng(address.Latitude, address.Longitude);
                _dropoffPin.Visible = true;
            }
            if (_dropoffCenterPin != null) _dropoffCenterPin.Visibility = ViewStates.Gone;
        }

        private void ShowPickupPin(Address address)
        {
            if (_pickupPin == null)
            {
                _pickupPin = Map.AddMarker(new MarkerOptions()
                    .SetPosition(new LatLng(0, 0))
                    .Anchor(.5f, 1f)
                    .InvokeIcon(_hailIcon)
                    .Visible(false));
            }

            if (address == null)
                return;

// ReSharper disable CompareOfFloatsByEqualityOperator
            if (address.Latitude != 0 && address.Longitude != 0)
// ReSharper restore CompareOfFloatsByEqualityOperator
            {
                _pickupPin.Position = new LatLng(address.Latitude, address.Longitude);
                _pickupPin.Visible = true;
            }
            if (_pickupCenterPin != null) _pickupCenterPin.Visibility = ViewStates.Gone;
        }

        private void ShowAvailableVehicles(AvailableVehicle[] vehicles)
        {
            // remove currently displayed pushpins
            foreach (var marker in _availableVehiclePushPins)
            {
                marker.Remove();
                marker.Dispose();
            }
            _availableVehiclePushPins.Clear();

            if (vehicles == null)
                return;

            const string defaultLogoName = "taxi";

            foreach (var v in vehicles)
            {
                bool isCluster = v is AvailableVehicleCluster;
                string logoKey = isCluster
                                 ? string.Format("cluster_{0}", v.LogoName ?? defaultLogoName)
                                 : string.Format("nearby_{0}", v.LogoName ?? defaultLogoName);

                var resId = DrawableHelper.GetIdFromString(Resources, logoKey);
                if (!resId.HasValue)
                {
                    // Resource not found, skip to next
                    continue;
                }

                var pushPin = Map.AddMarker(new MarkerOptions()
                    .Anchor(.5f, 1f)
                    .SetPosition(new LatLng(v.Latitude, v.Longitude))
                    .InvokeIcon(BitmapDescriptorFactory.FromResource(resId.Value))
                    .Visible(true));

                _availableVehiclePushPins.Add(pushPin);
            }
            PostInvalidate();
        }

        private AvailableVehicle[] Clusterize(AvailableVehicle[] vehicles)
        {
            // Divide the map in 25 cells (5*5)
            const int numberOfRows = 5;
            const int numberOfColumns = 5;
            // Maximum number of vehicles in a cell before we start displaying a cluster
            const int cellThreshold = 1;

            var result = new List<AvailableVehicle>();

            var bounds = new Rect();
            GetDrawingRect(bounds);

            var clusterWidth = (bounds.Right - bounds.Left)/numberOfColumns;
            var clusterHeight = (bounds.Bottom - bounds.Top)/numberOfRows;

            var list = new List<AvailableVehicle>(vehicles);

            for (int rowIndex = 0; rowIndex < numberOfRows; rowIndex++)
                for (int colIndex = 0; colIndex < numberOfColumns; colIndex++)
                {
                    var top = bounds.Top + rowIndex*clusterHeight;
                    var left = bounds.Left + colIndex*clusterWidth;
                    var bottom = bounds.Top + (rowIndex + 1)*clusterHeight;
                    var right = bounds.Left + (colIndex + 1)*clusterWidth;
                    var rect = new Rect(left, top, right, bottom);

                    var vehiclesInRect = list.Where(v => IsVehicleInRect(v, rect)).ToArray();
                    if (vehiclesInRect.Length > cellThreshold)
                    {
                        var clusterBuilder = new VehicleClusterBuilder();
                        foreach (var v in vehiclesInRect) clusterBuilder.Add(v);
                        result.Add(clusterBuilder.Build());
                    }
                    else
                    {
                        result.AddRange(vehiclesInRect);
                    }
                    foreach (var v in vehiclesInRect) list.Remove(v);
                }
            return result.ToArray();
        }

        private bool IsVehicleInRect(AvailableVehicle vehicle, Rect rect)
        {
            var vehicleLocationInScreen =
                Map.Projection.ToScreenLocation(new LatLng(vehicle.Latitude, vehicle.Longitude));

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
            var textSize = DrawHelper.GetPixels(11);
            var textVerticalOffset = DrawHelper.GetPixels(12);
            var taxiIcon = BitmapFactory.DecodeResource(Resources, Resource.Drawable.taxi_icon);

            /* Find the width and height of the title*/
            var paintText = new TextPaint(PaintFlags.AntiAlias | PaintFlags.LinearText);
            paintText.SetARGB(255, 0, 0, 0);
            paintText.SetTypeface(Typeface.DefaultBold);
            paintText.TextSize = textSize;
            paintText.TextAlign = Paint.Align.Center;

            var rect = new Rect();
            paintText.GetTextBounds(vehicleNumber, 0, vehicleNumber.Length, rect);

            var mutableBitmap = taxiIcon.Copy(taxiIcon.GetConfig(), true);
            var canvas = new Canvas(mutableBitmap);
// ReSharper disable once PossibleLossOfFraction
            canvas.DrawText(vehicleNumber, canvas.Width/2, rect.Height() + textVerticalOffset, paintText);
            return mutableBitmap;
        }
		public void Pause()
		{
			base.OnPause();
			_mapReady = false;
		}


    }

	public class LayoutObserverForMap : Java.Lang.Object, ViewTreeObserver.IOnGlobalLayoutListener
	{
        private readonly TouchMap _touchMap;

        public LayoutObserverForMap(TouchMap touchMap)
		{
			_touchMap = touchMap;
		}

		public void OnGlobalLayout()
		{
			_touchMap.SetMapReady();
		}
	}
}