using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Windows.Input;
using Android.Animation;
using Android.App;
using Android.Content.Res;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.MapDataProvider.Resources;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using apcurium.MK.Booking.Mobile.Client.Diagnostic;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.ViewModels.Map;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using Cirrious.MvvmCross.Binding.Attributes;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Com.Mapbox.Mapboxsdk.Annotations;
using Com.Mapbox.Mapboxsdk.Camera;
using Com.Mapbox.Mapboxsdk.Geometry;
using Com.Mapbox.Mapboxsdk.Maps;
using MK.Common.Configuration;
using TinyIoC;
using Android.Content;
using System.Reactive.Linq;
using apcurium.MK.Booking.Mobile.Extensions;
using System.Threading;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class OrderMapFragment: IMvxBindable, IDisposable, IChangePresentation
    {
		public MapboxMap Mapbox { get; set; } 
		public MapView MapView { get; set; } 

        private MarkerOptions _pickupPin;
        private MarkerOptions _destinationPin;

        private MarkerOptions _taxiLocationPin;

        private readonly List<MarkerOptions> _availableVehicleMarkers = new List<MarkerOptions> ();

        private Icon _destinationIcon;
        private Icon _hailIcon;

        private IDictionary<string, Icon> _vehicleIcons; 

        public OrderMapFragment(TouchableMap mapFragment, Resources resources, TaxiHailSetting settings)
        {
            _resources = resources;
            _settings = settings;
            _showVehicleNumber = settings.ShowAssignedVehicleNumberOnPin;

            this.CreateBindingContext();  

			MapView = mapFragment.Map;

			var mapBoxInit = new MapAsync();

			mapBoxInit.MapReady += (s,e) =>
			{
				var locationService = TinyIoCContainer.Current.Resolve<ILocationService>();
            	var initialPosition = locationService.GetInitialPosition();

				Mapbox = mapBoxInit.Map;
				Mapbox.UiSettings.LogoEnabled = false; // TODO: is it ok? Legacy: Map.SetLogoVisibility((int)ViewStates.Gone);
				Mapbox.UiSettings.AttributionEnabled = false; // SetAttributionVisibility((int)ViewStates.Gone);
				Mapbox.UiSettings.MaxZoom = 20;
				Mapbox.UiSettings.MinZoom = 4;

				MoveCameraTo(initialPosition.Latitude, initialPosition.Longitude, 12f);
				Mapbox.UiSettings.CompassEnabled = false;
				Mapbox.UiSettings.RotateGesturesEnabled = false; // Legacy: RotateEnable.

	            InitializeOverlayIcons();
	            CreatePins();
				InitDrawables();
			}; 

			TouchableMap = mapFragment;
			MapView.GetMapAsync(mapBoxInit);
			this.DelayBind(InitializeBinding);
        }

		private class MapAsync : Java.Lang.Object, IOnMapReadyCallback
		{
			public MapboxMap Map { get; private set; }

  			public event EventHandler MapReady;

			void Com.Mapbox.Mapboxsdk.Maps.IOnMapReadyCallback.OnMapReady (Com.Mapbox.Mapboxsdk.Maps.MapboxMap map)
			{
				Map = map;
				var handler = MapReady;
				if (handler != null)
				{
					handler(this, EventArgs.Empty);
				}
			}
		}

        public OrderStatusDetail OrderStatusDetail
        {
            get { return _orderStatusDetail; }
            set
            {
                _orderStatusDetail = value;

                if (value != null && _orderStatusDetail.IBSStatusId.SoftEqual(VehicleStatuses.Common.Loaded))
                {
                	_pickupPin.Marker.Remove();
                }
            }
        }

        private void MoveMarker(MarkerOptions pin, Icon icon, double lat, double lng)
        {
            pin.SetPosition(new LatLng(lat, lng));
        }

        private void AnimateMarkerOnMap (MarkerToAnimate markerToAnimate)
		{
			AnimateMarkersOnMap(new List<MarkerToAnimate> { markerToAnimate });
		}

		private void AnimateMarkersOnMap (List<MarkerToAnimate> markersToAnimate)
		{
			var markersToUpdate = markersToAnimate.Select (x => x.MarkerOptions).ToArray ();
			var animation = ValueAnimator.OfFloat (0, 100);
			var fromLatLngList = markersToAnimate.Select(x => x.FromPosition).ToArray();
			var toLatLngList = markersToAnimate.Select(x => x.ToPosition).ToArray();

            animation.SetDuration (2000);
            animation.SetInterpolator(new Android.Views.Animations.AccelerateDecelerateInterpolator());

            animation.Update += (s, e) =>
            {
            	var fraction = e.Animation.AnimatedFraction;
				
				for (int i = 0; i < fromLatLngList.Length; i++)
				{
					var fromPosition = fromLatLngList[i];
					var toPosition = toLatLngList[i];
					var animatedPosition = new LatLng (fromPosition.Latitude + fraction * (toLatLngList[i].Latitude - fromPosition.Latitude),
                		fromPosition.Longitude + fraction * (toPosition.Longitude - fromPosition.Longitude));

					markersToUpdate[i].Marker.Position = animatedPosition;
					markersToUpdate[i].SetPosition(animatedPosition).SetIcon(markersToAnimate[i].Icon);
				}
            };
            animation.AnimationEnd += (s, e) =>
            {
            	
            };
            animation.Start();
        }

        private void UpdateTaxiLocation(TaxiLocation value)
        {
            if (value != null && value.Latitude.HasValue && value.Longitude.HasValue && value.VehicleNumber.HasValue())
            {
                ShowAvailableVehicles(null);

                // Update Marker and Animate it to see it move on the map
                if (_taxiLocationPin != null)
                {
                    var icon = ViewModel.Settings.ShowOrientedPins  && value.CompassCourse.HasValue
						? IconFactory.FromBitmap(DrawHelper.RotateImageByDegreesWithСenterCrop(Resource.Drawable.nearby_oriented_passenger, value.CompassCourse.Value))
                        : IconFactory.FromBitmap(CreateTaxiBitmap());

					var pinToAnimate = new MarkerToAnimate(_taxiLocationPin, new LatLng(value.Latitude.Value, value.Longitude.Value), new LatLng(value.Latitude.Value, value.Longitude.Value), icon, value.CompassCourse);
					AnimateMarkerOnMap(pinToAnimate);
                	
                    if (_showVehicleNumber)
                    {
                        Mapbox.SelectMarker(_taxiLocationPin.Marker);
                    }
                }

                // Create Marker the first time
                else
                {
                    try
                    {
                        var icon = ViewModel.Settings.ShowOrientedPins && value.CompassCourse.HasValue
                            ? IconFactory.FromBitmap(DrawHelper.RotateImageByDegreesWithСenterCrop(Resource.Drawable.nearby_oriented_passenger, value.CompassCourse.Value))
                            : IconFactory.FromBitmap(CreateTaxiBitmap());

                        var mapOptions = new MarkerOptions()
							.SetPosition(new LatLng(value.Latitude.Value, value.Longitude.Value))
							.SetIcon(icon);

                        if (_showVehicleNumber)
                        {
                            var inflater = Application.Context.GetSystemService(Context.LayoutInflaterService) as LayoutInflater;
                            var addBottomMargin = !(ViewModel.Settings.ShowOrientedPins && value.CompassCourse.HasValue);
                            var markerAdapter = new CustomMarkerPopupAdapter(inflater, addBottomMargin, _resources, value.Market);
                            Mapbox.InfoWindowAdapter = markerAdapter;
                            mapOptions.SetTitle(value.VehicleNumber);
                        }
                        _taxiLocationPin = mapOptions;

                        Mapbox.AddMarker(mapOptions);

                        if (_showVehicleNumber)
                        {
                            Mapbox.SelectMarker(_taxiLocationPin.Marker);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex);
                    }

                    _isBookingMode = true;

                    return;
                }
            }

            // Booking is now over, so we need to clean up.
            if (value == null && _taxiLocationPin != null)
            {
                _isBookingMode = false;
                Mapbox.RemoveMarker(_taxiLocationPin.Marker);
                //_taxiLocationPin.Marker.Remove();
                _taxiLocationPin = null;
            }
        }

        private IconFactory IconFactory
        {
        	get
        	{
				return IconFactory.GetInstance(MapView.Context);
        	}
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

            TouchableMap.Surface.ScrollBy = (deltaX, deltaY) =>
                {
                    ScrollBy(deltaX, deltaY);
                };



            TouchableMap.Surface.ZoomBy = (animate, zoomByAmount) =>
                {
					var zoomLevel = ZoomLevel + zoomByAmount;

					if(ZoomLevel <= Mapbox.UiSettings.MaxZoom && zoomLevel >= Mapbox.UiSettings.MinZoom)
                    {
                        try
                        {
                            ZoomTo(zoomLevel, animate); 
                        }
                        catch(Exception ex)
                        {
                            Logger.LogError(ex);
                        }
                    }
                };

            Observable
				.FromEventPattern<MapView.MapChangedEventArgs>(MapView, "MapChanged")
                .Where(arg =>
                    {
						return (arg.EventArgs.P0 == MapView.RegionDidChange || arg.EventArgs.P0 == MapView.RegionDidChangeAnimated) 
							&& Mapbox != null;
                    })
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Do(_ =>
                    {
                        if (!_bypassCameraChangeEvent)
                        {
                            ViewModel.DisableBooking();
                        }
                    })
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(OnMapChanged, Logger.LogError)
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

        private void ScrollBy (float x, float y)
		{
			Mapbox.MoveCamera(CameraUpdateFactory.ScrollBy(x, y));
		}

		// https://github.com/mapbox/mapbox-gl-native/blob/d64652f9bef9f1c252d197d3ceed638778540d6f/platform/android/MapboxGLAndroidSDK/src/main/java/com/mapbox/mapboxsdk/camera/CameraUpdateFactory.java
		private float ZoomLevel
		{
			get
			{ 
				var uiSettings = Mapbox.UiSettings;

				// Calculate the bounds of the possibly rotated shape with respect to the viewport
				var nePixel = new PointF(-10000, -10000);
            	var swPixel = new PointF(10000, 10000);

            	var viewportHeight = uiSettings.Height;

            	foreach (var latLng in Mapbox.Projection.VisibleRegion.LatLngBounds.ToLatLngs()) 
            	{
					var pixel = Mapbox.Projection.ToScreenLocation(latLng);
                	swPixel.X = Math.Min(swPixel.X, pixel.X);
                	nePixel.X = Math.Max(nePixel.X, pixel.X);
                	swPixel.Y = Math.Min(swPixel.Y, viewportHeight - pixel.Y);
                	nePixel.Y = Math.Max(nePixel.Y, viewportHeight - pixel.Y);
            	}

				// Calculate wid=th/height
            	float width = nePixel.X - swPixel.X;
            	float height = nePixel.Y - swPixel.Y;

				var paddingValues = Mapbox.GetPadding();
				var padding = new RectF(paddingValues[0], paddingValues[1], paddingValues[2], paddingValues[3]);

				var scaleX = (uiSettings.Width - padding.Left - padding.Right) / width;
            	var scaleY = (uiSettings.Height - padding.Top - padding.Bottom) / height;
            	var minScale = scaleX < scaleY ? scaleX : scaleY;
            	var zoom = Mapbox.Projection.CalculateZoom(minScale);
				zoom = Com.Mapbox.Mapboxsdk.Utils.MathUtils.Clamp(zoom, uiSettings.MinZoom, uiSettings.MaxZoom);
	            return (float) Com.Mapbox.Mapboxsdk.Utils.MathUtils.Clamp(zoom, Mapbox.UiSettings.MinZoom, Mapbox.UiSettings.MaxZoom);
			}
		}

        private void InitDrawables()
        {     
            _vehicleIcons = new Dictionary<string, Icon>();

            var useCompanyColor = _settings.UseThemeColorForMapIcons;
            var companyColor = _resources.GetColor (Resource.Color.company_color);

            var red = Color.Argb(255, 255, 0, 23);
            var green = Color.Argb(255, 30, 192, 34);

            _destinationIcon = IconFactory.FromBitmap(DrawHelper.ApplyColorToMapIcon(Resource.Drawable.@destination_icon, red, true));
            _hailIcon = IconFactory.FromBitmap(DrawHelper.ApplyColorToMapIcon(Resource.Drawable.@hail_icon, green, true));

            _vehicleIcons.Add("nearby_taxi", IconFactory.FromBitmap(DrawHelper.ApplyColorToMapIcon(Resource.Drawable.@nearby_taxi, companyColor, false)));
            _vehicleIcons.Add("nearby_suv", IconFactory.FromBitmap(DrawHelper.ApplyColorToMapIcon(Resource.Drawable.@nearby_suv, companyColor, false)));
            _vehicleIcons.Add("nearby_blackcar", IconFactory.FromBitmap(DrawHelper.ApplyColorToMapIcon(Resource.Drawable.@nearby_blackcar, companyColor, false)));
            _vehicleIcons.Add("nearby_wheelchair", IconFactory.FromBitmap(DrawHelper.ApplyColorToMapIcon(Resource.Drawable.@nearby_wheelchair, companyColor, false)));
            _vehicleIcons.Add("cluster_taxi", IconFactory.FromBitmap(DrawHelper.ApplyColorToMapIcon(Resource.Drawable.@cluster_taxi, companyColor, false)));
            _vehicleIcons.Add("cluster_suv", IconFactory.FromBitmap(DrawHelper.ApplyColorToMapIcon(Resource.Drawable.@cluster_suv, companyColor, false)));
            _vehicleIcons.Add("cluster_blackcar", IconFactory.FromBitmap(DrawHelper.ApplyColorToMapIcon(Resource.Drawable.@cluster_blackcar, companyColor, false)));
            _vehicleIcons.Add("cluster_wheelchair", IconFactory.FromBitmap(DrawHelper.ApplyColorToMapIcon(Resource.Drawable.@cluster_wheelchair, companyColor, false)));
        }

        private MapBounds GetMapBoundsFromProjection()
        {
			var latLngBounds = Mapbox.Projection.VisibleRegion.LatLngBounds;

            var newMapBounds = new MapBounds()
                { 
					SouthBound = latLngBounds.LatSouth,
                    WestBound = latLngBounds.LonWest, 
                    NorthBound = latLngBounds.LatNorth, 
                    EastBound = latLngBounds.LonEast
                };

            return newMapBounds;
        }

        private LatLngBounds GetRegionFromMapBounds(MapBounds bounds)
        {
			return new LatLngBounds.Builder()
				.Include(new LatLng(bounds.SouthBound, bounds.WestBound))
				.Include(new LatLng(bounds.NorthBound, bounds.EastBound))
				.Build();
        }

        private void CreatePins()
        {
            if (_pickupPin == null)
            {
                _pickupPin = new MarkerOptions()
                    .SetPosition(new LatLng(0, 0))
                    .SetIcon(_hailIcon);

                Mapbox.AddMarker(_pickupPin);
            }     

            if (_destinationPin == null)
            {
                _destinationPin = new MarkerOptions()
                    .SetPosition(new LatLng(0, 0))
                    .SetIcon(_destinationIcon);

                Mapbox.AddMarker(_destinationPin);
            }     
        }

        private void ShowMarkers()
        {
            if (AddressSelectionMode == AddressSelectionMode.DropoffSelection)
            {
                var position = new LatLng(PickupAddress.Latitude, PickupAddress.Longitude);

                _destinationPin.Marker.Remove();
                _pickupOverlay.Visibility = ViewStates.Invisible;
                _destinationOverlay.Visibility = ViewStates.Visible;

                if (PickupAddress.HasValidCoordinate())
                {
                    _pickupPin.Marker.Remove();
					_pickupPin.SetIcon(_hailIcon);
                    _pickupPin.SetPosition(GetMarkerPositionWithAnchor(position, _pickupPin.Icon));
                    Mapbox.AddMarker(_pickupPin);
                }
                else
                {
                    _pickupPin.Marker.Remove();
                }
            }
            else if(AddressSelectionMode == AddressSelectionMode.PickupSelection)
            {
                _pickupPin.Marker.Remove();
                _destinationOverlay.Visibility = ViewStates.Invisible;
                _pickupOverlay.Visibility = ViewStates.Visible;

                if (DestinationAddress.HasValidCoordinate())
                {
                    var position = new LatLng(DestinationAddress.Latitude, DestinationAddress.Longitude);
                    _destinationPin.Marker.Remove();
					_destinationPin.SetIcon(_destinationIcon);
                    _destinationPin.SetPosition(GetMarkerPositionWithAnchor(position, _destinationPin.Icon));             
                    Mapbox.AddMarker(_destinationPin);
                }
                else
                {
                    _destinationPin.Marker.Remove();
                }
            }
            else
            {
                _destinationOverlay.Visibility = ViewStates.Gone;
                _pickupOverlay.Visibility = ViewStates.Gone;


                if (PickupAddress.HasValidCoordinate())
                {
                    var position = new LatLng(PickupAddress.Latitude, PickupAddress.Longitude);
                    _pickupPin.Marker.Remove();
					_pickupPin.SetIcon(_hailIcon);
                    _pickupPin.SetPosition(GetMarkerPositionWithAnchor(position, _pickupPin.Icon));  
                    Mapbox.AddMarker(_pickupPin);
                }
                else
                {
                    _pickupPin.Marker.Remove();
                }

                if (DestinationAddress.HasValidCoordinate())
                {
                    var position = new LatLng(DestinationAddress.Latitude, DestinationAddress.Longitude);
                    _destinationPin.Marker.Remove();
					_destinationPin.SetIcon(_destinationIcon);
                    _destinationPin.SetPosition(GetMarkerPositionWithAnchor(position, _destinationPin.Icon)); 
                    Mapbox.AddMarker(_destinationPin);
                }
                else
                {
                    _destinationPin.Marker.Remove();
                }
            }
        }

        private LatLng GetMarkerPositionWithAnchor(LatLng position, Icon image)
        {
            var point = Mapbox.Projection.ToScreenLocation(position);
            point.Y = point.Y - image.Bitmap.Height / 2;

            return Mapbox.Projection.FromScreenLocation(point);
        }

        private void OnMapChanged(EventPattern<MapView.MapChangedEventArgs> e)
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

            if (!_settings.ShowIndividualTaxiMarkerOnly)
            {
                ShowAvailableVehicles(VehicleClusterHelper.Clusterize(AvailableVehicles, bounds)); 
            }

            if (TaxiLocation != null)
            {
                CancelAutoFollow.ExecuteIfPossible();
            } 
        }

        private void ClearAllMarkers()
        {
            foreach (var markerOptions in _availableVehicleMarkers)
            {
                markerOptions.Marker.Remove();
            }
            _availableVehicleMarkers.Clear ();
        }

        private void DeleteMarker(MarkerOptions markerToRemove)
        {
            markerToRemove.Marker.Remove ();
            _availableVehicleMarkers.Remove (markerToRemove);
        }

        private void CreateMarker(AvailableVehicle vehicle)
        {
            var isCluster = vehicle is AvailableVehicleCluster;
            const string defaultLogoName = "taxi";
            var logoKey = isCluster
                ? string.Format ("cluster_{0}", vehicle.LogoName ?? defaultLogoName)
                : string.Format ("nearby_{0}", vehicle.LogoName ?? defaultLogoName);

            // if it uses pin message in future - it has to use RotateImageByDegreesWithСenterCrop instead of RotateImageByDegrees
            var icon = ViewModel.Settings.ShowOrientedPins
                ? IconFactory.FromBitmap(DrawHelper.RotateImageByDegreesWithСenterCrop(Resource.Drawable.nearby_oriented_available, vehicle.CompassCourse))
                : _vehicleIcons[logoKey];

            var vehicleMarker = new MarkerOptions()
                .SetPosition(new LatLng(vehicle.Latitude, vehicle.Longitude))
                .SetTitle(vehicle.VehicleName)
                .SetSnippet(vehicle.Market)
				.SetIcon(icon);

            Mapbox.AddMarker(vehicleMarker);

            _availableVehicleMarkers.Add(vehicleMarker);
        }

        private void ShowAvailableVehicles (IEnumerable<AvailableVehicle> vehicles)
		{
			if (vehicles == null || _isBookingMode)
			{
				ClearAllMarkers ();
				return;
			}

			var vehicleArray = vehicles.ToArray ();

			var vehicleNumbersToBeShown = vehicleArray.Select (x => x.VehicleName);

			// check for markers that needs to be removed
			var markersToRemove = _availableVehicleMarkers.Where (x => !vehicleNumbersToBeShown.Contains (x.Title)).ToList ();
			foreach (var marker in markersToRemove)
			{
				DeleteMarker (marker);
			}
			// check for updated or new
			var markersToAnimate = new List<MarkerToAnimate> ();

			foreach (var vehicle in vehicleArray)
			{
				var existingMarkerForVehicle = _availableVehicleMarkers.FirstOrDefault (x => x.Title == vehicle.VehicleName);

				if (existingMarkerForVehicle != null)
				{
					if (Math.Abs (existingMarkerForVehicle.Position.Latitude - vehicle.Latitude) < double.Epsilon
					                   && Math.Abs (existingMarkerForVehicle.Position.Longitude - vehicle.Longitude) < double.Epsilon)
					{
						// vehicle not updated, nothing to do
						continue;
					}

					var oldPosition = new LatLng () {
						Latitude = existingMarkerForVehicle.Position.Latitude,
						Longitude = existingMarkerForVehicle.Position.Longitude,
					};

					var isCluster = vehicle is AvailableVehicleCluster;

					const string defaultLogoName = "taxi";

					var logoKey = isCluster
		                ? string.Format ("cluster_{0}", vehicle.LogoName ?? defaultLogoName)
		                : string.Format ("nearby_{0}", vehicle.LogoName ?? defaultLogoName);

					var icon = ViewModel.Settings.ShowOrientedPins
		                ? IconFactory.FromBitmap (DrawHelper.RotateImageByDegreesWithСenterCrop (Resource.Drawable.nearby_oriented_available, vehicle.CompassCourse))
		                : _vehicleIcons [logoKey];

					var vehicleCoordinates = new LatLng (vehicle.Latitude, vehicle.Longitude);

					var markerToAnimate = new MarkerToAnimate (existingMarkerForVehicle, oldPosition, vehicleCoordinates, icon, vehicle.CompassCourse);

					markersToAnimate.Add (markerToAnimate); 
				} else
				{
					CreateMarker (vehicle);
				}
			} 

			if (markersToAnimate.Any ())
			{
				AnimateMarkersOnMap(markersToAnimate);
			}
        }

        public class MarkerToAnimate
		{
			public MarkerToAnimate(MarkerOptions markerOptions, LatLng oldPosition, LatLng newPosition, Icon icon, double? compassCourse)
			{
				MarkerOptions = markerOptions;
				FromPosition = oldPosition;
				ToPosition = newPosition;
				Icon = icon;
				CompassCourse = compassCourse;
			}

			public MarkerOptions MarkerOptions { get; set; }
			public LatLng FromPosition  { get; set; }
			public LatLng ToPosition  { get; set; }
			public Icon Icon { get; set; }
			public double? CompassCourse { get; set; }
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
                    MoveCameraTo(streetLevelZoomHint.Latitude, streetLevelZoomHint.Longitude);
                }
                else
                {
                    MoveCameraTo(streetLevelZoomHint.Latitude, streetLevelZoomHint.Longitude, zoomLevel + 1);
                }
            }

            var zoomHint = hint as ChangeZoomPresentationHint;
            if (zoomHint != null) 
            {
                var newBounds = zoomHint.Bounds;
                var currentBounds = GetMapBoundsFromProjection();

                if (Math.Abs(currentBounds.LongitudeDelta) <= Math.Abs(newBounds.LongitudeDelta))
                {
					MoveCameraTo(GetRegionFromMapBounds(newBounds), true); 
                }
            }

            var centerHint = hint as CenterMapPresentationHint;
            if(centerHint != null)
            {
                MoveCameraTo(centerHint.Latitude, centerHint.Longitude);
            }
        }

		private void ZoomTo (float zoom, bool animate)
		{
			if (animate)
			{
				Mapbox.AnimateCamera (CameraUpdateFactory.ZoomTo (zoom));
			} else
			{
				Mapbox.MoveCamera(CameraUpdateFactory.ZoomTo(zoom));
			}
        }


		private void MoveCameraTo (LatLngBounds bounds, bool animate = false)
		{
			if (animate)
			{
				Mapbox.AnimateCamera (CameraUpdateFactory.NewLatLngBounds (bounds, MapPadding.ToPixels ()));
			} else
			{
				Mapbox.MoveCamera (CameraUpdateFactory.NewLatLngBounds (bounds, MapPadding.ToPixels ()));
			}
        }

		private void MoveCameraTo (LatLngBounds bounds, int[] paddings, bool animate = false)
		{
			if (animate)
			{
				Mapbox.AnimateCamera (CameraUpdateFactory.NewLatLngBounds (bounds, paddings[0], paddings[1], paddings[2], paddings[3]));
			} else
			{
				Mapbox.MoveCamera (CameraUpdateFactory.NewLatLngBounds (bounds, paddings[0], paddings[1], paddings[2], paddings[3]));
			}
        }

        private void MoveCameraTo(double lat, double lng, float zoom)
        {
            Mapbox.MoveCamera(CameraUpdateFactory.NewLatLngZoom(new LatLng(lat, lng), zoom));
        }

        private void MoveCameraTo(double lat, double lng)
        {
			Mapbox.MoveCamera(CameraUpdateFactory.NewLatLng(new LatLng(lat, lng)));
        }

        private void SetZoom (IEnumerable<CoordinateViewModel> addressesToDisplay)
		{
			var coordinateViewModels = addressesToDisplay as CoordinateViewModel[] ?? addressesToDisplay.SelectOrDefault (addresses => addresses.ToArray (), new CoordinateViewModel[0]);
			if (!coordinateViewModels.Any ())
			{
				return;
			}

			// We should not trigger the camera change event since this is an automated camera change.
			_bypassCameraChangeEvent = true;

			if (coordinateViewModels.Length == 1)
			{
				var lat = coordinateViewModels [0].Coordinate.Latitude;
				var lon = coordinateViewModels [0].Coordinate.Longitude;

				if (coordinateViewModels [0].Zoom != ViewModels.ZoomLevel.DontChange)
				{
					MoveCameraTo (lat, lon, 16);
				} else
				{
					MoveCameraTo (lat, lon);
				}
				return;
			}

			double northLat = 90;
			double southLat = -90;
			double eastLon = 180;
			double westLon = -180;

			foreach (var item in coordinateViewModels)
			{
				var lat = item.Coordinate.Latitude;
				var lon = item.Coordinate.Longitude;
				southLat = Math.Max (lat, southLat);
				northLat = Math.Min (lat, northLat);
				westLon = Math.Max (lon, westLon);
				eastLon = Math.Min (lon, eastLon);
			}

			var overlayOffset = OverlayOffsetProvider != null
                ? OverlayOffsetProvider () + _pickupOverlay.Height
                : 0;

			var latLngBounds = new LatLngBounds.Builder ()
				.Include (new LatLng (southLat, westLon))
				.Include (new LatLng (northLat, eastLon))
				.Build ();

			MoveCameraTo (latLngBounds, new int[] { 40, overlayOffset, 40, 40 });

			overlayOffset = (TouchableMap.View.Height / 2) - (((TouchableMap.View.Height - overlayOffset) / 2) + overlayOffset);

			AnimateLatLngZoom (Mapbox.CameraPosition.Target, 0, overlayOffset);
		}

		private void AnimateLatLngZoom(LatLng latlng, int offsetX, int offsetY) 
        {
            PointF pointInScreen = Mapbox.Projection.ToScreenLocation(latlng);

            PointF newPoint = new PointF();
            newPoint.X = pointInScreen.X + offsetX;
            newPoint.Y = pointInScreen.Y + offsetY;

			LatLng newCenterLatLng = Mapbox.Projection.FromScreenLocation(newPoint);

            // Animate a camera with new latlng center and required zoom.
			Mapbox.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(newCenterLatLng, (float) Mapbox.CameraPosition.Zoom));
        }

        public TouchableMap TouchableMap { get; set;}
        private ImageView _pickupOverlay;
        private ImageView _destinationOverlay;

        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();
        private bool _bypassCameraChangeEvent;

        private IEnumerable<CoordinateViewModel> _center;

        private readonly Resources _resources;
        private readonly TaxiHailSetting _settings;

        private const int MapPadding = 60;

        private readonly bool _showVehicleNumber;

        private bool _isBookingMode;

        private bool _lockGeocoding;
        private TaxiLocation _taxiLocation;
        private OrderStatusDetail _orderStatusDetail;

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

        public TaxiLocation TaxiLocation
        {
            get { return _taxiLocation; }
            set
            {
                _taxiLocation = value;
                UpdateTaxiLocation(value);
            }
        }

        private Bitmap CreateTaxiBitmap()
        {
            return DrawHelper.ApplyColorToMapIcon(Resource.Drawable.taxi_icon, _resources.GetColor(Resource.Color.company_color), true);
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
                if (_availableVehicles == null || _availableVehicles.SequenceEqual(value))
                {
                    return;
                }

                _availableVehicles = _settings.ShowIndividualTaxiMarkerOnly
                    ? value
                    : VehicleClusterHelper.Clusterize(value, GetMapBoundsFromProjection());

                ShowAvailableVehicles(_availableVehicles);
            }
        }

        public IMvxBindingContext BindingContext { get; set; }

        public ICommand CancelAutoFollow { get; set; }

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

        public void Dispose()
        {
            _subscriptions.Dispose();
        }

        public Func<int> OverlayOffsetProvider { get; set; }
    }

}