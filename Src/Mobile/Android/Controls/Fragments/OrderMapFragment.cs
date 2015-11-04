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
    public class OrderMapFragment: IMvxBindable, IDisposable, IChangePresentation
    {
        public MapView Map { get; set;}
	    public TouchableMap TouchableMap { get; set;}
        private ImageView _pickupOverlay;
        private ImageView _destinationOverlay;
        private MarkerOptions _pickupPin;
        private MarkerOptions _destinationPin;
        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();
	    private bool _bypassCameraChangeEvent;

		private IEnumerable<CoordinateViewModel> _center;

	    private MarkerOptions _taxiLocationPin;

        private readonly List<MarkerOptions> _availableVehicleMarkers = new List<MarkerOptions> ();

        private Sprite _destinationIcon;
        private Sprite _hailIcon;

        private readonly Resources _resources;
		private readonly TaxiHailSetting _settings;

        private IDictionary<string, Sprite> _vehicleIcons; 

		private const int MapPadding = 60;

		private readonly bool _showVehicleNumber;

	    private bool _isBookingMode;

		private bool _lockGeocoding;
		private TaxiLocation _taxiLocation;
		private OrderStatusDetail _orderStatusDetail;

		public OrderMapFragment(TouchableMap mapFragment, Resources resources, TaxiHailSetting settings)
        {
            _resources = resources;
			_settings = settings;
			_showVehicleNumber = settings.ShowAssignedVehicleNumberOnPin;

            this.CreateBindingContext();  
                      
            Map = mapFragment.Map;
		    Map.MyLocationEnabled = true;

            _lastCenterLocation = Map.CenterCoordinate;

            TouchableMap = mapFragment;

            InitializeOverlayIcons();

            this.DelayBind(InitializeBinding);

            CreatePins();

            InitDrawables();
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

	    public TaxiLocation TaxiLocation
	    {
		    get { return _taxiLocation; }
		    set
		    {
				_taxiLocation = value;
			    UpdateTaxiLocation(value);
		    }
	    }

        // Animate Marker on the map between retrieving positions
        //TODO Not working with MapBox for now
        private void AnimateMarkerOnMap(Sprite icon, MarkerOptions markerToUpdate, LatLng newPosition, double? compassCourse, Position oldPosition)
        {
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

                    //TODO fix to move marker as the animation are not currently working with MapBox
                    _taxiLocationPin.InvokeIcon(icon);
                    _taxiLocationPin.InvokePosition( new LatLng(value.Latitude.Value, value.Longitude.Value));
                    _taxiLocationPin.Marker.Remove();
                    Map.AddMarker(_taxiLocationPin);

                    AnimateMarkerOnMap(icon, _taxiLocationPin, new LatLng(value.Latitude.Value, value.Longitude.Value), value.CompassCourse, new Position()
                        {
                            Latitude = value.Latitude.Value, 
                            Longitude = value.Longitude.Value
                        });
                    
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
                .Throttle(TimeSpan.FromMilliseconds(500))
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(OnMapChanged)
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
                var position = new LatLng(PickupAddress.Latitude, PickupAddress.Longitude);

                _destinationPin.Marker.Remove();
                _pickupOverlay.Visibility = ViewStates.Invisible;
                _destinationOverlay.Visibility = ViewStates.Visible;

                if (PickupAddress.HasValidCoordinate())
                {
                    _pickupPin.InvokeIcon(_hailIcon);
                    _pickupPin.InvokePosition(GetMarkerPositionWithAnchor(position, _pickupPin.Icon));
                    _pickupPin.Marker.Remove();
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
                    _destinationPin.InvokeIcon(_destinationIcon);
                    _destinationPin.InvokePosition(GetMarkerPositionWithAnchor(position, _destinationPin.Icon));             
                    _destinationPin.Marker.Remove();
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
                    _pickupPin.InvokeIcon(_hailIcon);
                    _pickupPin.InvokePosition(GetMarkerPositionWithAnchor(position, _pickupPin.Icon));  
                    _pickupPin.Marker.Remove();
                    Map.AddMarker(_pickupPin);
	            }
	            else
	            {
                    _pickupPin.Marker.Remove();
	            }

	            if (DestinationAddress.HasValidCoordinate())
	            {
                    var position = new LatLng(DestinationAddress.Latitude, DestinationAddress.Longitude);
                    _destinationPin.InvokeIcon(_destinationIcon);
                    _destinationPin.InvokePosition(GetMarkerPositionWithAnchor(position, _destinationPin.Icon)); 
                    _destinationPin.Marker.Remove();
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

        private LatLng _lastCenterLocation;
        private void OnMapChanged(EventPattern<MapView.MapChangedEventArgs> e)
        {
            var centerLocation = ((MapView)e.Sender).CenterCoordinate;
            if (centerLocation.DistanceTo(_lastCenterLocation) > 0.1)
            {
                if (_bypassCameraChangeEvent)
                {
                    _bypassCameraChangeEvent = false;
                    return;
                }
                else
                {
                    ViewModel.DisableBooking();
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
                _lastCenterLocation = centerLocation;
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
        private void UpdateMarker(MarkerOptions markerToUpdate, AvailableVehicle vehicle, Position oldPosition)
        {
            var isCluster = vehicle is AvailableVehicleCluster;
            const string defaultLogoName = "taxi";
            var logoKey = isCluster
                ? string.Format ("cluster_{0}", vehicle.LogoName ?? defaultLogoName)
                : string.Format ("nearby_{0}", vehicle.LogoName ?? defaultLogoName);

            var icon = ViewModel.Settings.ShowOrientedPins
                ? Map.SpriteFactory.FromBitmap(DrawHelper.RotateImageByDegreesWithСenterCrop(Resource.Drawable.nearby_oriented_available, vehicle.CompassCourse))
                : _vehicleIcons[logoKey];

            //TODO fix to move marker as the animation are not currently working with MapBox
            markerToUpdate.InvokeIcon(icon);
            markerToUpdate.InvokePosition( new LatLng(vehicle.Latitude, vehicle.Longitude));
            markerToUpdate.Marker.Remove();
            Map.AddMarker(markerToUpdate);

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

        public Func<int> OverlayOffsetProvider { get; set; }

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

			if ((Math.Abs(southLat - northLat) < 0.004) && (Math.Abs(westLon - eastLon) < 0.004))
			{
				MoveCameraTo((southLat + northLat) / 2, (westLon + eastLon) / 2, 16);
				return;
			}
			
            var overlayOffset = OverlayOffsetProvider != null
                ? OverlayOffsetProvider() + _pickupOverlay.Height
                : 0;
            
            Map.SetVisibleCoordinateBounds(new CoordinateBounds(new LatLng(southLat, westLon), new LatLng(northLat, eastLon)), new RectF(40, overlayOffset, 40, 40), true);
		}
    }
}