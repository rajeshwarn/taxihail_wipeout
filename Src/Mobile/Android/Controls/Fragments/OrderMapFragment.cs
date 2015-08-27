using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Input;
using Android.Content.Res;
using Google.Android.M4b.Maps;
using Google.Android.M4b.Maps.Model;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Common.Entity;
using Cirrious.MvvmCross.Binding.Attributes;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.Views;
using MK.Common.Configuration;
using apcurium.MK.Booking.Maps.Geo;
using apcurium.MK.Booking.Mobile.Client.Diagnostic;
using apcurium.MK.Common;
using apcurium.MK.Common.Extensions;
using Android.Graphics;
using Android.Text;
using Color = Android.Graphics.Color;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class OrderMapFragment: IMvxBindable, IDisposable, IChangePresentation
    {
        public GoogleMap Map { get; set;}
	    public TouchableMap TouchableMap { get; set;}
        private ImageView _pickupOverlay;
        private ImageView _destinationOverlay;
        private Marker _pickupPin;
        private Marker _destinationPin;
        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();
	    private bool _bypassCameraChangeEvent;

		private IEnumerable<CoordinateViewModel> _center;

		private OrderStatusDetail _taxiLocation;
		private Marker _taxiLocationPin;

        private readonly List<Marker> _availableVehicleMarkers = new List<Marker> ();

        private BitmapDescriptor _destinationIcon;
        private BitmapDescriptor _hailIcon;

        private readonly Resources _resources;
		private readonly TaxiHailSetting _settings;

        private IDictionary<string, BitmapDescriptor> _vehicleIcons; 

		private const int MAP_PADDING = 60;

		private readonly bool _showVehicleNumber;

	    private bool _isBookingMode;

		public OrderMapFragment(TouchableMap mapFragment, Resources resources, TaxiHailSetting settings)
        {
            _resources = resources;
			_settings = settings;
			_showVehicleNumber = settings.ShowAssignedVehicleNumberOnPin;

			InitDrawables();

            this.CreateBindingContext();  
                      
            Map = mapFragment.Map;

            // NOTE: wasn't working on some devices, reverted to standard padding and moved the buttons up in the layout
            // add padding to the map to move the Google logo around
            // the padding must be the same for left/right and top/bottom for the pins to be correctly aligned
			Map.SetPadding (6.ToPixels(), 6.ToPixels(), 6.ToPixels(), 6.ToPixels());

            TouchableMap = mapFragment;

            InitializeOverlayIcons();

            this.DelayBind(InitializeBinding);

            CreatePins();
        }

        private void InitializeOverlayIcons()
        {
            var useCompanyColor = _settings.UseThemeColorForMapIcons;
            var companyColor = _resources.GetColor (Resource.Color.company_color);

            var red = Color.Argb(255, 255, 0, 23);
            var green = Color.Argb(255, 30, 192, 34);

            _pickupOverlay = (ImageView)TouchableMap.Activity.FindViewById(Resource.Id.pickupOverlay);
            _pickupOverlay.Visibility = ViewStates.Visible;
            _pickupOverlay.SetPadding(0, 0, 0, _pickupOverlay.Drawable.IntrinsicHeight / 2);
            _pickupOverlay.SetImageBitmap(DrawHelper.ApplyColorToMapIcon(Resource.Drawable.hail_icon, useCompanyColor ? companyColor : green, true));

            _destinationOverlay = (ImageView)TouchableMap.Activity.FindViewById(Resource.Id.destinationOverlay);
            _destinationOverlay.Visibility = ViewStates.Visible;
            _destinationOverlay.SetPadding(0, 0, 0, _destinationOverlay.Drawable.IntrinsicHeight / 2);
            _destinationOverlay.SetImageBitmap(DrawHelper.ApplyColorToMapIcon(Resource.Drawable.destination_icon, useCompanyColor ? companyColor : red, true));
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

		public IEnumerable<CoordinateViewModel> Center
		{
			get { return _center; }
			set
			{
				_center = value;
				SetZoom(value); 
			}
		}

		public OrderStatusDetail TaxiLocation
		{
			get { return _taxiLocation; }
			set
			{
				_taxiLocation = value;

				UpdateTaxiLocation(value);
			}
		}


	    private void UpdateTaxiLocation(OrderStatusDetail value)
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
				try
				{
					_taxiLocationPin = Map.AddMarker(new MarkerOptions()
						.Anchor(.5f, 1f)
						.SetPosition(new LatLng(value.VehicleLatitude.Value, value.VehicleLongitude.Value))
						.InvokeIcon(BitmapDescriptorFactory.FromBitmap(CreateTaxiBitmap(value.VehicleNumber)))
						.Visible(true));
				}
				catch (Exception ex)
				{
					Logger.LogError(ex);
				}

				_isBookingMode = true;

				ShowAvailableVehicles(null);
			}
			else
			{
				_isBookingMode = false;
			}
	    }

		private Bitmap CreateTaxiBitmap(string vehicleNumber)
		{
			var taxiIcon = DrawHelper.ApplyColorToMapIcon(Resource.Drawable.taxi_icon, _resources.GetColor(Resource.Color.company_color), true);

			if (!_showVehicleNumber)
			{
				return taxiIcon;
			}

			var textSize = DrawHelper.GetPixels(11);
			var textVerticalOffset = DrawHelper.GetPixels(12);

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
			canvas.DrawText(vehicleNumber, canvas.Width / 2, rect.Height() + textVerticalOffset, paintText);
			return mutableBitmap;
		}

        private IList<AvailableVehicle> _availableVehicles = new List<AvailableVehicle>();
        public IList<AvailableVehicle> AvailableVehicles
        {
            get
            {
                return _availableVehicles;
            }
            set
            {
                if (_availableVehicles != value)
                {
                    _availableVehicles = value;
                    ShowAvailableVehicles (VehicleClusterHelper.Clusterize(value, GetMapBoundsFromProjection()));
                }
            }
        }

        public IMvxBindingContext BindingContext { get; set; }

        private bool _lockGeocoding;

	    [MvxSetToNullAfterBinding]
        public object DataContext
        {
            get { return BindingContext.DataContext; }
            set 
            { 
                BindingContext.DataContext = value; 
            }
        }

        public MapViewModel ViewModel
        {
            get
            {
                return (MapViewModel)DataContext;
            }
        }

        private void CancelAddressSearch()
        {
            _lockGeocoding = true;
            ((HomeViewModel)(ViewModel.Parent)).LocateMe.Cancel();
            ((HomeViewModel)(ViewModel.Parent)).AutomaticLocateMeAtPickup.Cancel();
            ViewModel.UserMovedMap.Cancel();
        }

        public void InitializeBinding()
        {
            var set = this.CreateBindingSet<OrderMapFragment, MapViewModel>();

            TouchableMap.Surface.Touched += (sender, e) =>
            {
                switch (e.Action)
                {
                    case MotionEventActions.Down:
                        CancelAddressSearch();
                        break;                    
                    case MotionEventActions.Move:                
                        CancelAddressSearch();
                        break;                       
                    default:
                        _lockGeocoding = false;
                        break;
                }               
            };

            TouchableMap.Surface.MoveBy = (deltaX, deltaY) =>
            {
                TouchableMap.Map.MoveCamera(CameraUpdateFactory.ScrollBy(deltaX, deltaY));
            };

            TouchableMap.Surface.ZoomBy = (animate, zoomByAmount) =>
            {
                if(animate)
                {
                    TouchableMap.Map.AnimateCamera (CameraUpdateFactory.ZoomBy(zoomByAmount));
                }
                else
                {
                    TouchableMap.Map.MoveCamera (CameraUpdateFactory.ZoomBy(zoomByAmount));
                }
            };

            Observable
                .FromEventPattern<GoogleMap.CameraChangeEventArgs>(Map, "CameraChange")
                .Throttle(TimeSpan.FromMilliseconds(500))
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(OnCameraChanged)
                .DisposeWith(_subscriptions);
			
            set.Bind()
                .For(v => v.AddressSelectionMode)
                .To(vm => vm.AddressSelectionMode);

            set.Bind()
                .For(v => v.PickupAddress)
                .To(vm => vm.PickupAddress);
            
            set.Bind()
                .For(v => v.DestinationAddress)
                .To(vm => vm.DestinationAddress);

            set.Bind()
                .For(v => v.AvailableVehicles)
                .To(vm => vm.AvailableVehicles);

            set.Apply();
        }

        private void InitDrawables()
        {     
            _vehicleIcons = new Dictionary<string, BitmapDescriptor>();

			var useCompanyColor = _settings.UseThemeColorForMapIcons;
            var companyColor = _resources.GetColor (Resource.Color.company_color);

            var red = Color.Argb(255, 255, 0, 23);
            var green = Color.Argb(255, 30, 192, 34);

            _destinationIcon = BitmapDescriptorFactory.FromBitmap(DrawHelper.ApplyColorToMapIcon(Resource.Drawable.@destination_icon, useCompanyColor ? companyColor : red, true));
            _hailIcon = BitmapDescriptorFactory.FromBitmap(DrawHelper.ApplyColorToMapIcon(Resource.Drawable.@hail_icon, useCompanyColor ? companyColor : green, true));
            
            _vehicleIcons.Add("nearby_taxi", BitmapDescriptorFactory.FromBitmap(DrawHelper.ApplyColorToMapIcon(Resource.Drawable.@nearby_taxi, companyColor, false)));
            _vehicleIcons.Add("nearby_suv", BitmapDescriptorFactory.FromBitmap(DrawHelper.ApplyColorToMapIcon(Resource.Drawable.@nearby_suv, companyColor, false)));
            _vehicleIcons.Add("nearby_blackcar", BitmapDescriptorFactory.FromBitmap(DrawHelper.ApplyColorToMapIcon(Resource.Drawable.@nearby_blackcar, companyColor, false)));
            _vehicleIcons.Add("cluster_taxi", BitmapDescriptorFactory.FromBitmap(DrawHelper.ApplyColorToMapIcon(Resource.Drawable.@cluster_taxi, companyColor, false)));
            _vehicleIcons.Add("cluster_suv", BitmapDescriptorFactory.FromBitmap(DrawHelper.ApplyColorToMapIcon(Resource.Drawable.@cluster_suv, companyColor, false)));
            _vehicleIcons.Add("cluster_blackcar", BitmapDescriptorFactory.FromBitmap(DrawHelper.ApplyColorToMapIcon(Resource.Drawable.@cluster_blackcar, companyColor, false)));
        }

        private MapBounds GetMapBoundsFromProjection()
        {
            var bounds = Map.Projection.VisibleRegion.LatLngBounds;

            var newMapBounds = new MapBounds()
            { 
                SouthBound = bounds.Southwest.Latitude, 
                WestBound = bounds.Southwest.Longitude, 
                NorthBound = bounds.Northeast.Latitude, 
                EastBound = bounds.Northeast.Longitude
            };
            return newMapBounds;
        }

		private LatLngBounds GetRegionFromMapBounds(MapBounds bounds)
		{
			var maxLat = bounds.NorthBound;
			var maxLon = bounds.EastBound;
			var minLat = bounds.SouthBound;
			var minLon = bounds.WestBound;

			return new LatLngBounds (new LatLng (minLat, minLon), new LatLng (maxLat, maxLon));
		}

        private void CreatePins()
        {
            if (_pickupPin == null)
            {
                _pickupPin = Map.AddMarker(new MarkerOptions()
                    .SetPosition(new LatLng(0, 0))
                    .Anchor(.5f, 1f)
                    .InvokeIcon(_hailIcon)
                    .Visible(false));
            }     

            if (_destinationPin == null)
            {
                _destinationPin = Map.AddMarker(new MarkerOptions()
                    .SetPosition(new LatLng(0, 0))
                    .Anchor(.5f, 1f)
                    .InvokeIcon(_destinationIcon)
                    .Visible(false));
            }     
        }

        private void OnPickupAddressChanged()
        {
            if (PickupAddress == null)
            {
                return;
            }
                
            ShowMarkers();
        }

        private void OnDestinationAddressChanged()
        {
            if (DestinationAddress == null)
            {
                return; 
            }
                
            ShowMarkers();
        }

        private void ShowMarkers()
        {
            if (AddressSelectionMode == AddressSelectionMode.DropoffSelection)
            {
                var position = new Position(){ Latitude = PickupAddress.Latitude, Longitude = PickupAddress.Longitude };

                _destinationPin.Visible = false;
                _pickupOverlay.Visibility = ViewStates.Invisible;
                _destinationOverlay.Visibility = ViewStates.Visible;

                if (PickupAddress.HasValidCoordinate())
                {
                    _pickupPin.Visible = true;
                    _pickupPin.Position = new LatLng(position.Latitude, position.Longitude);
                }
                else
                {
                    _pickupPin.Visible = false;
                }
            }
            else if(AddressSelectionMode == AddressSelectionMode.PickupSelection)
            {
                _pickupPin.Visible = false;
                _destinationOverlay.Visibility = ViewStates.Invisible;
                _pickupOverlay.Visibility = ViewStates.Visible;

                if (DestinationAddress.HasValidCoordinate())
                {
                    var position = new Position(){ Latitude = DestinationAddress.Latitude, Longitude = DestinationAddress.Longitude };
                    _destinationPin.Visible = true;
                    _destinationPin.Position = new LatLng(position.Latitude, position.Longitude);             
                }
                else
                {
                    _destinationPin.Visible = false;
                }
            }
            else
            {
				_destinationOverlay.Visibility = ViewStates.Gone;
				_pickupOverlay.Visibility = ViewStates.Gone;


	            if (PickupAddress.HasValidCoordinate())
	            {
					var position = new Position() { Latitude = PickupAddress.Latitude, Longitude = PickupAddress.Longitude };
					_pickupPin.Visible = true;
					_pickupPin.Position = new LatLng(position.Latitude, position.Longitude);  
	            }
	            else
	            {
					_pickupPin.Visible = false;
	            }

	            if (DestinationAddress.HasValidCoordinate())
	            {
					var position = new Position() { Latitude = DestinationAddress.Latitude, Longitude = DestinationAddress.Longitude };
					_destinationPin.Visible = true;
					_destinationPin.Position = new LatLng(position.Latitude, position.Longitude); 
	            }
	            else
	            {
					_destinationPin.Visible = false;
	            }


				
            }
        }

        private void OnCameraChanged(System.Reactive.EventPattern<GoogleMap.CameraChangeEventArgs> e)
        {
            if (_bypassCameraChangeEvent)
            {
                _bypassCameraChangeEvent = false;
                return;
            }

            var bounds = GetMapBoundsFromProjection();
            if (!_lockGeocoding)
            {
                ViewModel.UserMovedMap.ExecuteIfPossible(bounds);
            }

            ShowAvailableVehicles (VehicleClusterHelper.Clusterize(AvailableVehicles, bounds)); 
        }

        private void ClearAllMarkers()
        {
            foreach (var vehicleMarker in _availableVehicleMarkers)
            {
                vehicleMarker.Remove ();
            }

            _availableVehicleMarkers.Clear ();
        }

        private void DeleteMarker(Marker markerToRemove)
        {
            markerToRemove.Remove ();
            _availableVehicleMarkers.Remove (markerToRemove);
        }

        private void CreateMarker(AvailableVehicle vehicle)
        {
            var isCluster = vehicle is AvailableVehicleCluster;
            const string defaultLogoName = "taxi";
            var logoKey = isCluster
                                ? string.Format("cluster_{0}", vehicle.LogoName ?? defaultLogoName)
                                : string.Format("nearby_{0}", vehicle.LogoName ?? defaultLogoName);

            var vehicleMarker = Map.AddMarker(new MarkerOptions()
                .SetPosition(new LatLng(vehicle.Latitude, vehicle.Longitude))
                .Anchor(.5f, 1f)
                .InvokeIcon(_vehicleIcons[logoKey]));

            _availableVehicleMarkers.Add (vehicleMarker);
        }

        private void ShowAvailableVehicles(IEnumerable<AvailableVehicle> vehicles)
        {
            if (vehicles == null || _isBookingMode)
            {
                ClearAllMarkers ();
                return;
            }

            var vehicleNumbersToBeShown = vehicles.Select (x => x.VehicleNumber.ToString());

            // check for markers that needs to be removed
            var markersToRemove = _availableVehicleMarkers.Where(x => !vehicleNumbersToBeShown.Contains(x.Title)).ToList();
            foreach (var marker in markersToRemove)
            {
                DeleteMarker(marker);
            }

            // check for updated or new
            foreach (var vehicle in vehicles)
            {
                var existingMarkerForVehicle = _availableVehicleMarkers.FirstOrDefault (x => x.Title == vehicle.VehicleNumber.ToString());
                if (existingMarkerForVehicle != null)
                {
                    if (existingMarkerForVehicle.Position.Latitude == vehicle.Latitude && existingMarkerForVehicle.Position.Longitude == vehicle.Longitude)
                    {
                        // vehicle not updated, nothing to do
                        continue;
                    }

                    // coordinates were updated, remove and add later with new position
                    DeleteMarker (existingMarkerForVehicle);
                }

                CreateMarker (vehicle);
            }
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }

        public void ChangePresentation(ChangePresentationHint hint)
        {
			var streetLevelZoomHint = hint as ZoomToStreetLevelPresentationHint;
			if (streetLevelZoomHint != null)
            {
                // When doing this presentation change, we don't want to reverse geocode the position since we already know the address and it's already set
                // It occurs on Android only, because of a Camera Change event
                _bypassCameraChangeEvent = true;
                var zoomLevel = streetLevelZoomHint.InitialZoom 
                    ? _settings.InitialZoomLevel 
                    : MapViewModel.ZoomStreetLevel;

                if (_settings.DisableAutomaticZoomOnLocation && !streetLevelZoomHint.InitialZoom)
                {
                    Map.AnimateCamera(CameraUpdateFactory.NewLatLng(new LatLng(streetLevelZoomHint.Latitude, streetLevelZoomHint.Longitude)));
                }
                else
                {
                    Map.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(new LatLng(streetLevelZoomHint.Latitude, streetLevelZoomHint.Longitude), zoomLevel + 1));
                }
            }

			var zoomHint = hint as ChangeZoomPresentationHint;
			if (zoomHint != null) 
			{
				var newBounds = zoomHint.Bounds;
				var currentBounds = this.GetMapBoundsFromProjection();

				if (Math.Abs(currentBounds.LongitudeDelta) <= Math.Abs(newBounds.LongitudeDelta))
				{
					// add a negative padding to counterbalance the map padding done for the "Google" legal logo on the map
					Map.AnimateCamera(CameraUpdateFactory.NewLatLngBounds (GetRegionFromMapBounds(newBounds), -MAP_PADDING.ToPixels())); 
				}
			}

            var centerHint = hint as CenterMapPresentationHint;
            if(centerHint != null)
            {
                Map.AnimateCamera(CameraUpdateFactory.NewLatLng(new LatLng(centerHint.Latitude, centerHint.Longitude)));
            }
        }

		private void AnimateTo(double lat, double lng, float zoom)
		{
			Map.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(new LatLng(lat, lng), zoom));
		}

		private void AnimateTo(double lat, double lng)
		{
			Map.AnimateCamera(CameraUpdateFactory.NewLatLng(new LatLng(lat, lng)));
		}

		private void SetZoom(IEnumerable<CoordinateViewModel> addresseesToDisplay)
		{
			var coordinateViewModels = addresseesToDisplay as CoordinateViewModel[] ?? addresseesToDisplay.ToArray();
            if(addresseesToDisplay == null || !coordinateViewModels.Any())
            {
                return;
            }

			if (coordinateViewModels.Length == 1)
			{
				var lat = coordinateViewModels[0].Coordinate.Latitude;
				var lon = coordinateViewModels[0].Coordinate.Longitude;

				if (coordinateViewModels[0].Zoom != ZoomLevel.DontChange)
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

			foreach (var item in coordinateViewModels)
			{
				var lat = item.Coordinate.Latitude;
				var lon = item.Coordinate.Longitude;
				maxLat = Math.Max(lat, maxLat);
				minLat = Math.Min(lat, minLat);
				maxLon = Math.Max(lon, maxLon);
				minLon = Math.Min(lon, minLon);
			}

			if ((Math.Abs(maxLat - minLat) < 0.004) && (Math.Abs(maxLon - minLon) < 0.004))
			{
				AnimateTo((maxLat + minLat) / 2, (maxLon + minLon) / 2, 16);
				return;
			}
			
			// Changes the map zoom to prevent hiding the pin under the booking status.
			if (Math.Abs(maxLat - minLat) > .001)
			{
				maxLat += .0025;

				var bookingStatusViewModel = ((HomeViewModel) ViewModel.Parent).BookingStatus;

				if (_settings.ShowCallDriver)
				{
					maxLat += 0.0045;
				}

				if (_settings.ShowVehicleInformation)
				{	
					if (!bookingStatusViewModel.VehicleDriverHidden)
					{
						maxLat += 0.0007;
					}
					if (!bookingStatusViewModel.VehicleFullInfoHidden)
					{
						maxLat += 0.0007;
					}
					if (!bookingStatusViewModel.CompanyHidden)
					{
						maxLat += 0.0007;
					}
				}	
			}

			Map.AnimateCamera(CameraUpdateFactory.NewLatLngBounds(new LatLngBounds(new LatLng(minLat, minLon), new LatLng(maxLat, maxLon)), DrawHelper.GetPixels(100)));
		}
    }
}