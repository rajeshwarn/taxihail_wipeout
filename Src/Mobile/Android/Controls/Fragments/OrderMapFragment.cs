using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Input;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Maps.Geo;
using apcurium.MK.Booking.Mobile.Client.Diagnostic;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using Android.Content.Res;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Attributes;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Google.Android.M4b.Maps;
using Google.Android.M4b.Maps.Model;
using MK.Common.Configuration;
using apcurium.MK.Booking.Mobile.ViewModels.Map;
using apcurium.MK.Common;
using Android.Animation;
using Android.App;
using Android.Content;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    /*
     * PARTIAL CLASS : the other part of the rest is situated in the TaxiHail.Shared Project 
    */
    public partial class OrderMapFragment
    {
        public GoogleMap Map { get; set;}
        private Marker _pickupPin;
        private Marker _destinationPin;

	    private Marker _taxiLocationPin;

        private readonly List<Marker> _availableVehicleMarkers = new List<Marker> ();

        private BitmapDescriptor _destinationIcon;
        private BitmapDescriptor _hailIcon;


        private IDictionary<string, BitmapDescriptor> _vehicleIcons; 

		public OrderMapFragment(TouchableMap mapFragment, Resources resources, TaxiHailSetting settings)
        {
            _resources = resources;
			_settings = settings;
			_showVehicleNumber = settings.ShowAssignedVehicleNumberOnPin;

			InitDrawables();

            this.CreateBindingContext();  
                      
            Map = mapFragment.Map;
		    Map.MyLocationEnabled = true;
		    Map.UiSettings.MyLocationButtonEnabled = false;

            // NOTE: wasn't working on some devices, reverted to standard padding and moved the buttons up in the layout
            // add padding to the map to move the Google logo around
            // the padding must be the same for left/right and top/bottom for the pins to be correctly aligned
			Map.SetPadding (6.ToPixels(), 6.ToPixels(), 6.ToPixels(), 6.ToPixels());

            TouchableMap = mapFragment;

            InitializeOverlayIcons();

            this.DelayBind(InitializeBinding);

            CreatePins();
        }

	    public OrderStatusDetail OrderStatusDetail
	    {
		    get { return _orderStatusDetail; }
		    set
		    {
			    _orderStatusDetail = value;

                if (value != null && (_orderStatusDetail.IBSStatusId.SoftEqual(VehicleStatuses.Common.Loaded) || _orderStatusDetail.IBSStatusId.SoftEqual(VehicleStatuses.Common.Unloaded)))
			    {
				    _pickupPin.Visible = false;
			    }
		    }
	    }

        // Animate Marker on the map between retrieving positions
        private void AnimateMarkerOnMap(BitmapDescriptor icon, Marker markerToUpdate, LatLng newPosition, double? compassCourse, Position oldPosition)
        {
            markerToUpdate.SetIcon(icon);
            markerToUpdate.SetAnchor(.5f, ViewModel.Settings.ShowOrientedPins && compassCourse.HasValue ? .5f : 1f);

            var evaluator = new LatLngEvaluator ();
            var objectAnimator = ObjectAnimator.OfObject (markerToUpdate, "position", evaluator, new LatLng(oldPosition.Latitude, oldPosition.Longitude), newPosition);
            objectAnimator.SetAutoCancel(true);
            objectAnimator.SetDuration (5000);
            objectAnimator.SetInterpolator(new Android.Views.Animations.LinearInterpolator());
            objectAnimator.Start();
        }

        private class LatLngEvaluator : Java.Lang.Object, ITypeEvaluator
        {
            public Java.Lang.Object Evaluate (float fraction, Java.Lang.Object startValue, Java.Lang.Object endValue)
            {
                var start = (LatLng)startValue;
                var end = (LatLng)endValue;
                return new LatLng (start.Latitude + fraction * (end.Latitude - start.Latitude),
                    start.Longitude + fraction * (end.Longitude - start.Longitude));
            }
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
						? BitmapDescriptorFactory.FromBitmap(DrawHelper.RotateImageByDegreesWithСenterCrop(Resource.Drawable.nearby_oriented_passenger, value.CompassCourse.Value))
                        : BitmapDescriptorFactory.FromBitmap(CreateTaxiBitmap());
                    
                    AnimateMarkerOnMap(icon, _taxiLocationPin, new LatLng(value.Latitude.Value, value.Longitude.Value), value.CompassCourse, new Position()
                        {
                            Latitude = value.Latitude.Value, 
                            Longitude = value.Longitude.Value
                        });

					if (_showVehicleNumber)
					{
						_taxiLocationPin.ShowInfoWindow();
					}
                }

                // Create Marker the first time
                else
                {
                    try
                    {
                        var mapOptions = new MarkerOptions()
							.Anchor(0.5f, ViewModel.Settings.ShowOrientedPins && value.CompassCourse.HasValue ? 0.5f : 1f)
                            .SetPosition(new LatLng(value.Latitude.Value, value.Longitude.Value))
                            .InvokeIcon(
								ViewModel.Settings.ShowOrientedPins && value.CompassCourse.HasValue
								? BitmapDescriptorFactory.FromBitmap(DrawHelper.RotateImageByDegreesWithСenterCrop(Resource.Drawable.nearby_oriented_passenger, value.CompassCourse.Value))
                                : BitmapDescriptorFactory.FromBitmap(CreateTaxiBitmap()))
                            .Visible(true);

                        if (_showVehicleNumber)
                        {
							var inflater = Application.Context.GetSystemService(Context.LayoutInflaterService) as LayoutInflater;
							var addBottomMargin = !(ViewModel.Settings.ShowOrientedPins && value.CompassCourse.HasValue);
							Map.SetInfoWindowAdapter(new CustomMarkerPopupAdapter(inflater, addBottomMargin, _resources, value.Market));
							mapOptions.SetTitle(value.VehicleNumber);
                        }

                        _taxiLocationPin = Map.AddMarker(mapOptions);

                        if (_showVehicleNumber)
                        {
                            _taxiLocationPin.ShowInfoWindow();
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
                _taxiLocationPin.Visible = false;
				_taxiLocationPin.Remove();
			    _taxiLocationPin = null;
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
				.Do(_ =>
				{
					if (!_bypassCameraChangeEvent)
					{
						ViewModel.DisableBooking();
					}
				})
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
            _vehicleIcons.Add("nearby_wheelchair", BitmapDescriptorFactory.FromBitmap(DrawHelper.ApplyColorToMapIcon(Resource.Drawable.@nearby_wheelchair, companyColor, false)));
            _vehicleIcons.Add("cluster_taxi", BitmapDescriptorFactory.FromBitmap(DrawHelper.ApplyColorToMapIcon(Resource.Drawable.@cluster_taxi, companyColor, false)));
            _vehicleIcons.Add("cluster_suv", BitmapDescriptorFactory.FromBitmap(DrawHelper.ApplyColorToMapIcon(Resource.Drawable.@cluster_suv, companyColor, false)));
            _vehicleIcons.Add("cluster_blackcar", BitmapDescriptorFactory.FromBitmap(DrawHelper.ApplyColorToMapIcon(Resource.Drawable.@cluster_blackcar, companyColor, false)));
            _vehicleIcons.Add("cluster_wheelchair", BitmapDescriptorFactory.FromBitmap(DrawHelper.ApplyColorToMapIcon(Resource.Drawable.@cluster_wheelchair, companyColor, false)));
        }

        private MapBounds GetMapBoundsFromProjection()
        {
            var bounds = Map.Projection.VisibleRegion.LatLngBounds;

            var newMapBounds = new MapBounds
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

        private void ShowMarkers()
        {
            if (AddressSelectionMode == AddressSelectionMode.DropoffSelection)
            {
                var position = new Position { Latitude = PickupAddress.Latitude, Longitude = PickupAddress.Longitude };

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
                    var position = new Position { Latitude = DestinationAddress.Latitude, Longitude = DestinationAddress.Longitude };
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
					var position = new Position { Latitude = PickupAddress.Latitude, Longitude = PickupAddress.Longitude };
					_pickupPin.Visible = true;
					_pickupPin.Position = new LatLng(position.Latitude, position.Longitude);  
	            }
	            else
	            {
					_pickupPin.Visible = false;
	            }

	            if (DestinationAddress.HasValidCoordinate())
	            {
					var position = new Position { Latitude = DestinationAddress.Latitude, Longitude = DestinationAddress.Longitude };
					_destinationPin.Visible = true;
					_destinationPin.Position = new LatLng(position.Latitude, position.Longitude); 
	            }
	            else
	            {
					_destinationPin.Visible = false;
	            }
            }
        }

        private void OnCameraChanged(EventPattern<GoogleMap.CameraChangeEventArgs> e)
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
				? string.Format ("cluster_{0}", vehicle.LogoName ?? defaultLogoName)
				: string.Format ("nearby_{0}", vehicle.LogoName ?? defaultLogoName);

			// if it uses pin message in future - it has to use RotateImageByDegreesWithСenterCrop instead of RotateImageByDegrees
	        var markerOptions = new MarkerOptions()
		        .SetPosition(new LatLng(vehicle.Latitude, vehicle.Longitude))
		        .SetTitle(vehicle.VehicleName)
		        .Anchor(.5f, ViewModel.Settings.ShowOrientedPins ? .5f : 1f)
		        .InvokeIcon(ViewModel.Settings.ShowOrientedPins
			        ? BitmapDescriptorFactory.FromBitmap(
				        DrawHelper.RotateImageByDegrees(Resource.Drawable.nearby_oriented_available, vehicle.CompassCourse))
			        : _vehicleIcons[logoKey]);

			var vehicleMarker = Map.AddMarker(markerOptions);

			vehicleMarker.Snippet = vehicle.Market;

            _availableVehicleMarkers.Add(vehicleMarker);
        }

        // Update Marker and Animate it to see it move on the map
        private void UpdateMarker(Marker markerToUpdate, AvailableVehicle vehicle, Position oldPosition)
        {
            var isCluster = vehicle is AvailableVehicleCluster;
            const string defaultLogoName = "taxi";
            var logoKey = isCluster
                ? string.Format ("cluster_{0}", vehicle.LogoName ?? defaultLogoName)
                : string.Format ("nearby_{0}", vehicle.LogoName ?? defaultLogoName);

            var icon = ViewModel.Settings.ShowOrientedPins
                ? BitmapDescriptorFactory.FromBitmap(DrawHelper.RotateImageByDegrees(Resource.Drawable.nearby_oriented_available, vehicle.CompassCourse))
                : _vehicleIcons[logoKey];

            AnimateMarkerOnMap(icon, markerToUpdate, new LatLng(vehicle.Latitude, vehicle.Longitude), vehicle.CompassCourse, oldPosition);
        }

        private void ShowAvailableVehicles(IEnumerable<AvailableVehicle> vehicles)
        {
            if (vehicles == null || _isBookingMode)
            {
                ClearAllMarkers ();
                return;
            }

	        var vehicleArray = vehicles.ToArray();

			var vehicleNumbersToBeShown = vehicleArray.Select(x => x.VehicleName);

            // check for markers that needs to be removed
            var markersToRemove = _availableVehicleMarkers.Where(x => !vehicleNumbersToBeShown.Contains(x.Title)).ToList();
            foreach (var marker in markersToRemove)
            {
                DeleteMarker(marker);
            }

            // check for updated or new
			foreach (var vehicle in vehicleArray)
            {
                var existingMarkerForVehicle = _availableVehicleMarkers.FirstOrDefault (x => x.Title == vehicle.VehicleName);

                if (existingMarkerForVehicle != null)
                {
                    if (Math.Abs(existingMarkerForVehicle.Position.Latitude - vehicle.Latitude) < double.Epsilon 
						&& Math.Abs(existingMarkerForVehicle.Position.Longitude - vehicle.Longitude) < double.Epsilon)
                    {
                        // vehicle not updated, nothing to do
                        continue;
                    }

                    var oldPosition = new Position()
                    {
                        Latitude = existingMarkerForVehicle.Position.Latitude,
                        Longitude = existingMarkerForVehicle.Position.Longitude,
                    };
                    
                    // coordinates were updated, remove and add later with new position
                    UpdateMarker(existingMarkerForVehicle, vehicle, oldPosition);
                }
                else
                {
                    CreateMarker(vehicle);
                }
            }
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
					// add a negative padding to counterbalance the map padding done for the "Google" legal logo on the map
					Map.AnimateCamera(CameraUpdateFactory.NewLatLngBounds (GetRegionFromMapBounds(newBounds), -MapPadding.ToPixels())); 
				}
			}

            var centerHint = hint as CenterMapPresentationHint;
            if(centerHint != null)
            {
                MoveCameraTo(centerHint.Latitude, centerHint.Longitude);
            }
        }

		private void MoveCameraTo(double lat, double lng, float zoom)
		{
			Map.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(new LatLng(lat, lng), zoom));
		}

		private void MoveCameraTo(double lat, double lng)
		{
			Map.AnimateCamera(CameraUpdateFactory.NewLatLng(new LatLng(lat, lng)));
		}

		private void SetZoom(IEnumerable<CoordinateViewModel> addresseesToDisplay)
		{
			var coordinateViewModels = addresseesToDisplay as CoordinateViewModel[] ?? addresseesToDisplay.SelectOrDefault(addresses => addresses.ToArray(), new CoordinateViewModel[0]);
            if(!coordinateViewModels.Any())
            {
                return;
            }
			
			
			// We should not trigger the camera change event since this is an automated camera change.
			_bypassCameraChangeEvent = true;

			if (coordinateViewModels.Length == 1)
			{
				var lat = coordinateViewModels[0].Coordinate.Latitude;
				var lon = coordinateViewModels[0].Coordinate.Longitude;

				if (coordinateViewModels[0].Zoom != ZoomLevel.DontChange)
				{
					MoveCameraTo(lat, lon, 16);
				}
				else
				{
					MoveCameraTo(lat, lon);
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

            var overlayOffset = OverlayOffsetProvider != null
                ? OverlayOffsetProvider() + _pickupOverlay.Height
                : 0;

            Map.AnimateCamera(CameraUpdateFactory.NewLatLngBounds(new LatLngBounds(new LatLng(minLat, minLon), new LatLng(maxLat, maxLon)), TouchableMap.View.Width, TouchableMap.View.Height - overlayOffset, DrawHelper.ToPixels(50)));

            overlayOffset = (TouchableMap.View.Height / 2) - (((TouchableMap.View.Height - overlayOffset)/2) + overlayOffset);

            animateLatLngZoom(Map.CameraPosition.Target, 0, overlayOffset);
		}

        private void animateLatLngZoom(LatLng latlng, int offsetX, int offsetY) 
        {
            Point pointInScreen = Map.Projection.ToScreenLocation(latlng);

            Point newPoint = new Point();
            newPoint.X = pointInScreen.X + offsetX;
            newPoint.Y = pointInScreen.Y + offsetY;

            LatLng newCenterLatLng = Map.Projection.FromScreenLocation(newPoint);

            // Animate a camera with new latlng center and required zoom.
            Map.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(newCenterLatLng, Map.CameraPosition.Zoom));
        }
    }
}