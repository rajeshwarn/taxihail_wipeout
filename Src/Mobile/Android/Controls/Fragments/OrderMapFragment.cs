using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Input;
using Android.Content.Res;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using Cirrious.MvvmCross.Binding.Attributes;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.Views;
using TinyIoC;
using MK.Common.Configuration;

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
        private BitmapDescriptor _nearbyTaxiIcon;
        private BitmapDescriptor _nearbyClusterIcon;
        private BitmapDescriptor _hailIcon;

        private Resources _resources;
		private TaxiHailSetting _settings;

		public OrderMapFragment(TouchableMap mapFragment, Resources resources, TaxiHailSetting settings)
        {
            _resources = resources;
			_settings = settings;

            InitDrawables();

            this.CreateBindingContext();  
                      
            Map = mapFragment.Map;

            // add padding to the map to move the Google logo around
            // the padding must be the same for left/right and top/bottom for the pins to be correctly aligned
            Map.SetPadding (60.ToPixels(), 6.ToPixels(), 60.ToPixels(), 6.ToPixels());

            _touchableMap = mapFragment;

            _pickupOverlay = (ImageView)mapFragment.Activity.FindViewById(Resource.Id.pickupOverlay);
            _pickupOverlay.Visibility = ViewStates.Visible;
            _pickupOverlay.SetPadding(0, 0, 0, _pickupOverlay.Drawable.IntrinsicHeight / 2);

            _destinationOverlay = (ImageView)mapFragment.Activity.FindViewById(Resource.Id.destinationOverlay);
            _destinationOverlay.Visibility = ViewStates.Visible;
            _destinationOverlay.SetPadding(0, 0, 0, _destinationOverlay.Drawable.IntrinsicHeight / 2);

            this.DelayBind(() => InitializeBinding());

            CreatePins();
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

        public ICommand UserMovedMap { get; set; }

        private IEnumerable<AvailableVehicle> _availableVehicles = new List<AvailableVehicle>();
        public IEnumerable<AvailableVehicle> AvailableVehicles
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
                    ShowAvailableVehicles (VehicleClusterHelper.Clusterize(value != null ? value.ToArray () : null, GetMapBoundsFromProjection()));
                }
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

        public void InitializeBinding()
        {
            var set = this.CreateBindingSet<OrderMapFragment, MapViewModel>();

            _touchableMap.Surface.Touched += (object sender, MotionEvent e) =>
            {
                switch (e.Action)
                {
                    case MotionEventActions.Down:
                        lockGeocoding = true;
                        ((MapViewModel.CancellableCommand<MapBounds>)UserMovedMap).Cancel();
                        break;                    
                    case MotionEventActions.Move:                
                        lockGeocoding = true;
                        ((MapViewModel.CancellableCommand<MapBounds>)UserMovedMap).Cancel();
                        break;                       
                    default:
                        lockGeocoding = false;
                        break;
                }               
            };

            Observable
                .FromEventPattern<GoogleMap.CameraChangeEventArgs>(Map, "CameraChange")
                .Throttle(TimeSpan.FromMilliseconds(500))
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(e => OnCameraChanged(e))
                .DisposeWith(_subscriptions);

            set.Bind()
                .For(v => v.UserMovedMap)
                .To(vm => vm.UserMovedMap);

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
			var useColor = _settings.UseThemeColorForMapIcons;
            var colorBgTheme = useColor ? (Color?)_resources.GetColor(Resource.Color.company_color) : (Color?)null;

            var destinationIcon =  _resources.GetDrawable(Resource.Drawable.@destination_icon);
            var nearbyTaxiIcon = _resources.GetDrawable(Resource.Drawable.@nearby_taxi);                                
            var nearbyClusterIcon = _resources.GetDrawable(Resource.Drawable.@cluster); 
            var hailIcon = _resources.GetDrawable(Resource.Drawable.@hail_icon);                                

            _destinationIcon = DrawHelper.DrawableToBitmapDescriptor(destinationIcon, colorBgTheme);
            _nearbyTaxiIcon = DrawHelper.DrawableToBitmapDescriptor(nearbyTaxiIcon, colorBgTheme);
            _nearbyClusterIcon = DrawHelper.DrawableToBitmapDescriptor(nearbyClusterIcon, colorBgTheme);
            _hailIcon = DrawHelper.DrawableToBitmapDescriptor(hailIcon, colorBgTheme);
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
                return;

            ShowMarkers();
        }

        private void OnDestinationAddressChanged()
        {
            if (DestinationAddress == null)
                return; 

            ShowMarkers();
        }

        void ShowMarkers()
        {
            if (AddressSelectionMode == AddressSelectionMode.DropoffSelection)
            {
                Position position = new Position(){ Latitude = PickupAddress.Latitude, Longitude = PickupAddress.Longitude };

                if (!DestinationAddress.HasValidCoordinate())
                {
                    _destinationPin.Visible = false;
                }

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
            if (UserMovedMap != null && UserMovedMap.CanExecute(bounds) && !lockGeocoding)
            {
                UserMovedMap.Execute(bounds);
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
            var isCluster = (vehicle is AvailableVehicleCluster) 
                ? true 
                : false;

            var vehicleMarker = 
                Map.AddMarker(new MarkerOptions()
                    .SetTitle(vehicle.VehicleNumber.ToString())
                  .SetPosition(new LatLng(vehicle.Latitude, vehicle.Longitude))
                  .Anchor(.5f, 1f)
                  .InvokeIcon(isCluster ? _nearbyClusterIcon : _nearbyTaxiIcon));

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
            var zoomHint = hint as ZoomToStreetLevelPresentationHint;
            if (zoomHint != null)
            {
				Map.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(new LatLng(zoomHint.Latitude, zoomHint.Longitude), 15));
            }

            var centerHint = hint as CenterMapPresentationHint;
            if(centerHint != null)
            {
                Map.AnimateCamera(CameraUpdateFactory.NewLatLng(new LatLng(centerHint.Latitude, centerHint.Longitude)));
            }
        }
    }
}