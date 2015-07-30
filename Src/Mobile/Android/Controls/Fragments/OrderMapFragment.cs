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
using Color = Android.Graphics.Color;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class OrderMapFragment: IMvxBindable, IDisposable, IChangePresentation
    {
        public GoogleMap Map { get; set;}
        public TouchableMap _touchableMap { get; set;}
        private ImageView _pickupOverlay;
        private ImageView _destinationOverlay;
        private Marker _pickupPin;
        private Marker _destinationPin;
        private CompositeDisposable _subscriptions = new CompositeDisposable();
        private bool bypassCameraChangeEvent = false;

        private List<Marker> _availableVehicleMarkers = new List<Marker> ();

        private BitmapDescriptor _destinationIcon;
        private BitmapDescriptor _hailIcon;

        private Resources _resources;
		private TaxiHailSetting _settings;

        private IDictionary<string, BitmapDescriptor> _vehicleIcons; 

		private const int _mapPadding = 60;

		public OrderMapFragment(TouchableMap mapFragment, Resources resources, TaxiHailSetting settings)
        {
            _resources = resources;
			_settings = settings;

            InitDrawables();

            this.CreateBindingContext();  
                      
            Map = mapFragment.Map;
		    Map.MyLocationEnabled = true;
		    Map.UiSettings.MyLocationButtonEnabled = false;

            // NOTE: wasn't working on some devices, reverted to standard padding and moved the buttons up in the layout
            // add padding to the map to move the Google logo around
            // the padding must be the same for left/right and top/bottom for the pins to be correctly aligned
			Map.SetPadding (6.ToPixels(), 6.ToPixels(), 6.ToPixels(), 6.ToPixels());

            _touchableMap = mapFragment;

            InitializeOverlayIcons();

            this.DelayBind(() => InitializeBinding());

            CreatePins();
        }

        private void InitializeOverlayIcons()
        {
            var useCompanyColor = _settings.UseThemeColorForMapIcons;
            var companyColor = _resources.GetColor (Resource.Color.company_color);

            var red = Color.Argb(255, 255, 0, 23);
            var green = Color.Argb(255, 30, 192, 34);

            _pickupOverlay = (ImageView)_touchableMap.Activity.FindViewById(Resource.Id.pickupOverlay);
            _pickupOverlay.Visibility = ViewStates.Visible;
            _pickupOverlay.SetPadding(0, 0, 0, _pickupOverlay.Drawable.IntrinsicHeight / 2);
            _pickupOverlay.SetImageBitmap(DrawHelper.ApplyColorToMapIcon(Resource.Drawable.hail_icon, useCompanyColor ? companyColor : green, true));

            _destinationOverlay = (ImageView)_touchableMap.Activity.FindViewById(Resource.Id.destinationOverlay);
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

        private IList<AvailableVehicle> _availableVehicles = new List<AvailableVehicle>();
        public IList<AvailableVehicle> AvailableVehicles
        {
            get
            {
                return _availableVehicles;
            }
            set
            {
                if (_availableVehicles == value)
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

        private bool lockGeocoding = false;

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
            lockGeocoding = true;
            ((HomeViewModel)(ViewModel.Parent)).LocateMe.Cancel();
            ((HomeViewModel)(ViewModel.Parent)).AutomaticLocateMeAtPickup.Cancel();
            ViewModel.UserMovedMap.Cancel();
        }

        public void InitializeBinding()
        {
            var set = this.CreateBindingSet<OrderMapFragment, MapViewModel>();

            _touchableMap.Surface.Touched += (object sender, MotionEvent e) =>
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
                        lockGeocoding = false;
                        break;
                }               
            };

            _touchableMap.Surface.MoveBy = (deltaX, deltaY) =>
            {
                _touchableMap.Map.MoveCamera(CameraUpdateFactory.ScrollBy(deltaX, deltaY));
            };

            _touchableMap.Surface.ZoomBy = (animate, zoomByAmount) =>
            {
                if(animate)
                {
                    _touchableMap.Map.AnimateCamera (CameraUpdateFactory.ZoomBy(zoomByAmount));
                }
                else
                {
                    _touchableMap.Map.MoveCamera (CameraUpdateFactory.ZoomBy(zoomByAmount));
                }
            };

            Observable
                .FromEventPattern<GoogleMap.CameraChangeEventArgs>(Map, "CameraChange")
                .Throttle(TimeSpan.FromMilliseconds(500))
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(e => OnCameraChanged(e))
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
            var _bounds = Map.Projection.VisibleRegion.LatLngBounds;

            var bounds = new MapBounds()
            { 
                SouthBound = _bounds.Southwest.Latitude, 
                WestBound = _bounds.Southwest.Longitude, 
                NorthBound = _bounds.Northeast.Latitude, 
                EastBound = _bounds.Northeast.Longitude
            };
            return bounds;
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

        void ShowMarkers()
        {
            if (AddressSelectionMode == AddressSelectionMode.DropoffSelection)
            {
                Position position = new Position(){ Latitude = PickupAddress.Latitude, Longitude = PickupAddress.Longitude };

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
            else
            {
                _pickupPin.Visible = false;
                _destinationOverlay.Visibility = ViewStates.Invisible;
                _pickupOverlay.Visibility = ViewStates.Visible;

                if (DestinationAddress.HasValidCoordinate())
                {
                    Position position = new Position(){ Latitude = DestinationAddress.Latitude, Longitude = DestinationAddress.Longitude };
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
            if (bypassCameraChangeEvent)
            {
                bypassCameraChangeEvent = false;
                return;
            }

            var bounds = GetMapBoundsFromProjection();
            if (!lockGeocoding)
            {
                ViewModel.UserMovedMap.ExecuteIfPossible(bounds);
            }

            if (!_settings.ShowIndividualTaxiMarkerOnly)
            {
                ShowAvailableVehicles(VehicleClusterHelper.Clusterize(AvailableVehicles, bounds)); 
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
                bool isCluster = vehicle is AvailableVehicleCluster;
                const string defaultLogoName = "taxi";
                string logoKey = isCluster
                                 ? string.Format("cluster_{0}", vehicle.LogoName ?? defaultLogoName)
                                 : string.Format("nearby_{0}", vehicle.LogoName ?? defaultLogoName);

                var vehicleMarker =
                Map.AddMarker(new MarkerOptions()
                  .SetPosition(new LatLng(vehicle.Latitude, vehicle.Longitude))
                  .Anchor(.5f, 1f)
                  .InvokeIcon(_vehicleIcons[logoKey]));

            _availableVehicleMarkers.Add (vehicleMarker);
        }

        private void ShowAvailableVehicles(IEnumerable<AvailableVehicle> vehicles)
        {
            if (vehicles == null)
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
                bypassCameraChangeEvent = true;
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
					Map.AnimateCamera(CameraUpdateFactory.NewLatLngBounds (GetRegionFromMapBounds(newBounds), -_mapPadding.ToPixels())); 
				}
			}

            var centerHint = hint as CenterMapPresentationHint;
            if(centerHint != null)
            {
                Map.AnimateCamera(CameraUpdateFactory.NewLatLng(new LatLng(centerHint.Latitude, centerHint.Longitude)));
            }
        }
    }
}