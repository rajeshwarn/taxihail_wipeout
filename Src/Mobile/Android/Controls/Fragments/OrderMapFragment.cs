using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Widget;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Android.Views;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Extensions;
using Cirrious.MvvmCross.Binding.Attributes;
using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Data;
using System.Linq;
using apcurium.MK.Booking.Mobile.Infrastructure;
using System;
using System.Windows.Input;
using System.Reactive.Linq;
using System.Threading;
using System.Reactive.Disposables;
using TinyIoC;
using apcurium.MK.Common.Configuration;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class OrderMapFragment: IMvxBindable, IDisposable
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

        public OrderMapFragment(TouchableMap mapFragment, Resources resources)
        {
            _resources = resources;

            InitDrawables();

            this.CreateBindingContext();  
                      
            Map = mapFragment.Map;

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

        private MapBounds _mapBounds;
        public MapBounds MapBounds
        {
            get
            {
                return (_mapBounds == null) ? new MapBounds() : _mapBounds;
            }

            set
            {
                if (_mapBounds != value)
                {
                    _mapBounds = value;
                    OnMapBoundsChanged();
                }
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
                    ShowAvailableVehicles (VehicleClusterHelper.Clusterize(value.ToArray(), MapBounds));
                }
            }
        }

        public IMvxBindingContext BindingContext { get; set; }

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
            var binding = this.CreateBindingSet<OrderMapFragment, MapViewModel>();

            _touchableMap.Surface.Touched += (object sender, MotionEvent e) => 
            {
                switch (e.Action)
                {
                    case MotionEventActions.Down:
                        ((MapViewModel.CancellableCommand<MapBounds>)UserMovedMap).Cancel();
                        break;                    
                    case MotionEventActions.Move:
                        ((MapViewModel.CancellableCommand<MapBounds>)UserMovedMap).Cancel();
                        break;                    
                    default:
                        break;
                }               
            };


            Observable
                .FromEventPattern<GoogleMap.CameraChangeEventArgs>(Map, "CameraChange")
                .Throttle(TimeSpan.FromMilliseconds(500))
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(e => OnCameraChanged(e))
                .DisposeWith(_subscriptions);


            binding.Bind()
                .For(v => v.UserMovedMap)
                .To(vm => vm.UserMovedMap);

            binding.Bind()
                .For(v => v.AddressSelectionMode)
                .To(vm => vm.AddressSelectionMode);

            binding.Bind()
                .For(v => v.PickupAddress)
                    .To(vm => vm.PickupAddress);
            
            binding.Bind()
                .For(v => v.DestinationAddress)
                    .To(vm => vm.DestinationAddress);

            binding.Bind()
                .For(v => v.MapBounds)
                    .To(vm => vm.MapBounds);
            
            binding.Bind()
                .For(v => v.AvailableVehicles)
                    .To(vm => vm.AvailableVehicles);

            binding.Apply();

        }

        private void InitDrawables()
        {            
            var useColor = TinyIoCContainer.Current.Resolve<IAppSettings>().Data.UseThemeColorForMapIcons;
            var colorBgTheme = useColor ? (Color?)_resources.GetColor(Resource.Color.login_background_color) : (Color?)null;

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

        private void OnMapBoundsChanged()
        {
            if (MapBounds != null)
            {
                bypassCameraChangeEvent = true;
                var bounds = MapBounds;
                Map.AnimateCamera(
                    CameraUpdateFactory.NewLatLngBounds(
                        new LatLngBounds(new LatLng(bounds.SouthBound, bounds.WestBound), 
                            new LatLng(bounds.NorthBound, bounds.EastBound)),
                        DrawHelper.GetPixels(0)));
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
            if (UserMovedMap != null && UserMovedMap.CanExecute(bounds))
            {
                UserMovedMap.Execute(bounds);
            }
        }

        private void ShowAvailableVehicles(AvailableVehicle[] vehicles)
        {
            foreach (var vehicleMarker in _availableVehicleMarkers)
            {                
                vehicleMarker.Remove();
            }

            _availableVehicleMarkers.Clear();

            if (vehicles == null)
                return;

            foreach (var v in vehicles)
            {
                bool isCluster = (v is AvailableVehicleCluster) 
                    ? true 
                        : false;

                var vehicleMarker = 
                    Map.AddMarker(new MarkerOptions()
                                  .SetPosition(new LatLng(0, 0))
                                  .Anchor(.5f, 1f)
                                  .InvokeIcon(isCluster ? _nearbyClusterIcon : _nearbyTaxiIcon)
                                  .Visible(false));

                _availableVehicleMarkers.Add (vehicleMarker);
            }
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }

    }
}