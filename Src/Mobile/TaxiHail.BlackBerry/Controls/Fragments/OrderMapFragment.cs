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
using MK.Common.Configuration;
using apcurium.MK.Booking.Mobile.ViewModels.Map;
using apcurium.MK.Common;
using Android.Animation;
using Android.App;
using Android.Content;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using Com.Mapbox.Mapboxsdk.Annotations;
using Com.Mapbox.Mapboxsdk.Views;
using Com.Mapbox.Mapboxsdk.Geometry;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    /*
     * PARTIAL CLASS : the other part of the code is situated in the TaxiHail.Shared Project 
    */
    public partial class OrderMapFragment
    {
        public MapView Map { get; set;} 

        private MarkerOptions _pickupPin;
        private MarkerOptions _destinationPin;

        private MarkerOptions _taxiLocationPin;

        private readonly List<MarkerOptions> _availableVehicleMarkers = new List<MarkerOptions> ();

        private Sprite _destinationIcon;
        private Sprite _hailIcon;

        private IDictionary<string, Sprite> _vehicleIcons; 

        public OrderMapFragment(TouchableMap mapFragment, Resources resources, TaxiHailSetting settings)
        {
            _resources = resources;
            _settings = settings;
            _showVehicleNumber = settings.ShowAssignedVehicleNumberOnPin;

            this.CreateBindingContext();  

            Map = mapFragment.Map;
            Map.MyLocationEnabled = true;

            TouchableMap = mapFragment;

            InitializeOverlayIcons();

            this.DelayBind(InitializeBinding);

            CreatePins();

            InitDrawables();
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

        private void MoveMarker(MarkerOptions pin, Sprite icon, double lat, double lng)
        {
            pin.InvokeIcon(icon);
            pin.InvokePosition(new LatLng(lat, lng));
        }

        // Animate Marker on the map between retrieving positions
        private void AnimateMarkerOnMap(Sprite icon, MarkerOptions markerToUpdate, LatLng newPosition, double? compassCourse, LatLng oldPosition)
        {
            // Animation doesn't work on MapBox for now, so we just move the marker
            MoveMarker(markerToUpdate, icon, newPosition.Latitude, newPosition.Longitude);

            //            markerToUpdate.InvokeIcon(icon);
            //
            //            var evaluator = new LatLngEvaluator ();
            //            var valueAnimator = ValueAnimator.OfObject (evaluator, new LatLng(oldPosition.Latitude, oldPosition.Longitude), newPosition);
            //            valueAnimator.AddUpdateListener(new MarkerAnimatorAdapter(markerToUpdate));
            //            valueAnimator.SetDuration (5000);
            //            valueAnimator.SetInterpolator(new Android.Views.Animations.LinearInterpolator());
            //            valueAnimator.Start();
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

        private class MarkerAnimatorAdapter : Java.Lang.Object, ValueAnimator.IAnimatorUpdateListener
        {

            private MarkerOptions _markerOptions;

            public MarkerAnimatorAdapter(MarkerOptions markerOptions)
            {
                this._markerOptions = markerOptions;
            }

            public void OnAnimationUpdate(ValueAnimator animation)
            {
                var value = animation.AnimatedValue;
                _markerOptions.InvokePosition((LatLng)value);
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
                        ? Map.SpriteFactory.FromBitmap(DrawHelper.RotateImageByDegreesWithСenterCrop(Resource.Drawable.nearby_oriented_passenger, value.CompassCourse.Value))
                        : Map.SpriteFactory.FromBitmap(CreateTaxiBitmap());

                    AnimateMarkerOnMap(icon, _taxiLocationPin, new LatLng(value.Latitude.Value, value.Longitude.Value), value.CompassCourse, new LatLng(value.Latitude.Value, value.Longitude.Value));

                    if (_showVehicleNumber)
                    {
                        Map.SelectMarker(_taxiLocationPin.Marker);
                    }
                }

                // Create Marker the first time
                else
                {
                    try
                    {
                        var icon = ViewModel.Settings.ShowOrientedPins && value.CompassCourse.HasValue
                            ? Map.SpriteFactory.FromBitmap(DrawHelper.RotateImageByDegreesWithСenterCrop(Resource.Drawable.nearby_oriented_passenger, value.CompassCourse.Value))
                            : Map.SpriteFactory.FromBitmap(CreateTaxiBitmap());

                        var mapOptions = new MarkerOptions()
                            .InvokePosition(new LatLng(value.Latitude.Value, value.Longitude.Value))
                            .InvokeIcon(icon);

                        if (_showVehicleNumber)
                        {
                            var inflater = Application.Context.GetSystemService(Context.LayoutInflaterService) as LayoutInflater;
                            var addBottomMargin = !(ViewModel.Settings.ShowOrientedPins && value.CompassCourse.HasValue);
                            var markerAdapter = new CustomMarkerPopupAdapter(inflater, addBottomMargin, _resources, value.Market);
                            Map.InfoWindowAdapter = markerAdapter;
                            mapOptions.InvokeTitle(value.VehicleNumber);
                        }
                        _taxiLocationPin = mapOptions;

                        Map.AddMarker(mapOptions);

                        if (_showVehicleNumber)
                        {
                            Map.SelectMarker(_taxiLocationPin.Marker);
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
                _taxiLocationPin.Marker.Remove();
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

                    var currentLocationPoint = Map.ToScreenLocation(TouchableMap.Map.CenterCoordinate);

                    TouchableMap.Map.SetCenterCoordinate(Map.FromScreenLocation(new PointF(deltaX + currentLocationPoint.X, deltaY + currentLocationPoint.Y)), false);
                };

            TouchableMap.Surface.ZoomBy = (animate, zoomByAmount) =>
                {
                    var zoomLevel = TouchableMap.Map.ZoomLevel + zoomByAmount;
                    if(zoomLevel< MapView.MaximumZoomLevel && zoomLevel > 0)
                    {
                        try
                        {
                            TouchableMap.Map.SetZoomLevel(zoomLevel, animate); 
                        }
                        catch(Exception ex)
                        {
                            Logger.LogError(ex);
                        }
                    }
                };

            Observable
                .FromEventPattern<MapView.MapChangedEventArgs>(Map, "MapChanged")
                .Where(arg =>
                    {
                        return arg.EventArgs.Change == MapView.RegionDidChange;
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

        private void InitDrawables()
        {     
            _vehicleIcons = new Dictionary<string, Sprite>();

            var useCompanyColor = _settings.UseThemeColorForMapIcons;
            var companyColor = _resources.GetColor (Resource.Color.company_color);

            var red = Color.Argb(255, 255, 0, 23);
            var green = Color.Argb(255, 30, 192, 34);

            _destinationIcon = Map.SpriteFactory.FromBitmap(DrawHelper.ApplyColorToMapIcon(Resource.Drawable.@destination_icon, red, true));
            _hailIcon = Map.SpriteFactory.FromBitmap(DrawHelper.ApplyColorToMapIcon(Resource.Drawable.@hail_icon, green, true));

            _vehicleIcons.Add("nearby_taxi", Map.SpriteFactory.FromBitmap(DrawHelper.ApplyColorToMapIcon(Resource.Drawable.@nearby_taxi, companyColor, false)));
            _vehicleIcons.Add("nearby_suv", Map.SpriteFactory.FromBitmap(DrawHelper.ApplyColorToMapIcon(Resource.Drawable.@nearby_suv, companyColor, false)));
            _vehicleIcons.Add("nearby_blackcar", Map.SpriteFactory.FromBitmap(DrawHelper.ApplyColorToMapIcon(Resource.Drawable.@nearby_blackcar, companyColor, false)));
            _vehicleIcons.Add("nearby_wheelchair", Map.SpriteFactory.FromBitmap(DrawHelper.ApplyColorToMapIcon(Resource.Drawable.@nearby_wheelchair, companyColor, false)));
            _vehicleIcons.Add("cluster_taxi", Map.SpriteFactory.FromBitmap(DrawHelper.ApplyColorToMapIcon(Resource.Drawable.@cluster_taxi, companyColor, false)));
            _vehicleIcons.Add("cluster_suv", Map.SpriteFactory.FromBitmap(DrawHelper.ApplyColorToMapIcon(Resource.Drawable.@cluster_suv, companyColor, false)));
            _vehicleIcons.Add("cluster_blackcar", Map.SpriteFactory.FromBitmap(DrawHelper.ApplyColorToMapIcon(Resource.Drawable.@cluster_blackcar, companyColor, false)));
            _vehicleIcons.Add("cluster_wheelchair", Map.SpriteFactory.FromBitmap(DrawHelper.ApplyColorToMapIcon(Resource.Drawable.@cluster_wheelchair, companyColor, false)));
        }

        private MapBounds GetMapBoundsFromProjection()
        {
            var nePoint = new PointF(TouchableMap.Map.GetX() + TouchableMap.Map.Width, TouchableMap.Map.GetY());
            var swPoint = new PointF(TouchableMap.Map.GetX(), TouchableMap.Map.GetY() + TouchableMap.Map.Height );

            var neCoord = Map.FromScreenLocation(nePoint);
            var swCoord = Map.FromScreenLocation(swPoint);

            var newMapBounds = new MapBounds()
                { 
                    SouthBound = swCoord.Latitude, 
                    WestBound = swCoord.Longitude, 
                    NorthBound = neCoord.Latitude, 
                    EastBound = neCoord.Longitude
                };
            return newMapBounds;
        }

        private BoundingBox GetRegionFromMapBounds(MapBounds bounds)
        {
            return new BoundingBox (bounds.NorthBound, bounds.EastBound, bounds.SouthBound, bounds.WestBound);
        }

        private void CreatePins()
        {
            if (_pickupPin == null)
            {
                _pickupPin = new MarkerOptions()
                    .InvokePosition(new LatLng(0, 0))
                    .InvokeIcon(_hailIcon);

                Map.AddMarker(_pickupPin);
            }     

            if (_destinationPin == null)
            {
                _destinationPin = new MarkerOptions()
                    .InvokePosition(new LatLng(0, 0))
                    .InvokeIcon(_destinationIcon);

                Map.AddMarker(_destinationPin);
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
                    _pickupPin.InvokeIcon(_hailIcon);
                    _pickupPin.InvokePosition(GetMarkerPositionWithAnchor(position, _pickupPin.Icon));
                    Map.AddMarker(_pickupPin);
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
                    _destinationPin.InvokeIcon(_destinationIcon);
                    _destinationPin.InvokePosition(GetMarkerPositionWithAnchor(position, _destinationPin.Icon));             
                    Map.AddMarker(_destinationPin);
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
                    _pickupPin.InvokeIcon(_hailIcon);
                    _pickupPin.InvokePosition(GetMarkerPositionWithAnchor(position, _pickupPin.Icon));  
                    Map.AddMarker(_pickupPin);
                }
                else
                {
                    _pickupPin.Marker.Remove();
                }

                if (DestinationAddress.HasValidCoordinate())
                {
                    var position = new LatLng(DestinationAddress.Latitude, DestinationAddress.Longitude);
                    _destinationPin.Marker.Remove();
                    _destinationPin.InvokeIcon(_destinationIcon);
                    _destinationPin.InvokePosition(GetMarkerPositionWithAnchor(position, _destinationPin.Icon)); 
                    Map.AddMarker(_destinationPin);
                }
                else
                {
                    _destinationPin.Marker.Remove();
                }
            }
        }

        private LatLng GetMarkerPositionWithAnchor(LatLng position, Sprite image)
        {
            var point = Map.ToScreenLocation(position);
            point.Y = point.Y - image.Bitmap.Height / 2;

            return Map.FromScreenLocation(point);
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
                ? Map.SpriteFactory.FromBitmap(DrawHelper.RotateImageByDegreesWithСenterCrop(Resource.Drawable.nearby_oriented_available, vehicle.CompassCourse))
                : _vehicleIcons[logoKey];

            var vehicleMarker = new MarkerOptions()
                .InvokePosition(new LatLng(vehicle.Latitude, vehicle.Longitude))
                .InvokeTitle(vehicle.VehicleName)
                .InvokeSnippet(vehicle.Market)
                .InvokeIcon(icon);

            Map.AddMarker(vehicleMarker);

            _availableVehicleMarkers.Add(vehicleMarker);
        }

        // Update Marker and Animate it to see it move on the map
        private void UpdateMarker(MarkerOptions markerToUpdate, AvailableVehicle vehicle, LatLng oldPosition)
        {
            var isCluster = vehicle is AvailableVehicleCluster;
            const string defaultLogoName = "taxi";
            var logoKey = isCluster
                ? string.Format ("cluster_{0}", vehicle.LogoName ?? defaultLogoName)
                : string.Format ("nearby_{0}", vehicle.LogoName ?? defaultLogoName);

            var icon = ViewModel.Settings.ShowOrientedPins
                ? Map.SpriteFactory.FromBitmap(DrawHelper.RotateImageByDegreesWithСenterCrop(Resource.Drawable.nearby_oriented_available, vehicle.CompassCourse))
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

                    var oldPosition = new LatLng()
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
                    Map.SetCenterCoordinate(GetRegionFromMapBounds(newBounds).Center, true); 
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
            Map.SetCenterCoordinate(new LatLngZoom(new LatLng(lat, lng), zoom), true);
        }

        private void MoveCameraTo(double lat, double lng)
        {
            Map.SetCenterCoordinate(new LatLng(lat, lng), true);
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

            double northLat = 90;
            double southLat = -90;
            double eastLon = 180;
            double westLon = -180;

            foreach (var item in coordinateViewModels)
            {
                var lat = item.Coordinate.Latitude;
                var lon = item.Coordinate.Longitude;
                southLat = Math.Max(lat, southLat);
                northLat = Math.Min(lat, northLat);
                westLon = Math.Max(lon, westLon);
                eastLon = Math.Min(lon, eastLon);
            }

            var overlayOffset = OverlayOffsetProvider != null
                ? OverlayOffsetProvider() + _pickupOverlay.Height
                : 0;

            Map.SetVisibleCoordinateBounds(new CoordinateBounds(new LatLng(southLat, westLon), new LatLng(northLat, eastLon)), new RectF(40, overlayOffset, 40, 40), true);
        }
    }
}