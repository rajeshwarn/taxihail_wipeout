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

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class OrderMapFragment: IMvxBindable, IDisposable
    {
        public GoogleMap Map { get; set;}
        public TouchableMap _touchableMap { get; set;}
        private ImageView _pickupOverlay;
        private Marker _pickupPin;
        private Marker _destinationPin;
        private CompositeDisposable _subscriptions = new CompositeDisposable();

        private List<Marker> _availableVehicleMarkers = new List<Marker> ();

        public OrderMapFragment(TouchableMap _map)
        {
            this.CreateBindingContext();
            Map = _map.Map;
            _touchableMap = _map;
            _pickupOverlay = (ImageView)_map.Activity.FindViewById(Resource.Id.pickupOverlay);
            _pickupOverlay.Visibility = ViewStates.Visible;
            _pickupOverlay.SetPadding(0, 0, 0, _pickupOverlay.Drawable.IntrinsicHeight / 2);
            this.DelayBind(() => InitializeBinding());
            CreatePins();
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
                .Subscribe(e =>
            {
                if (bypassCameraChangeEvent)
                {
                    bypassCameraChangeEvent = false;
                    return;
                }

                var _target = Map.CameraPosition.Target;

                var bounds = GetMapBoundsFromProjection();
                                        
                if (UserMovedMap != null && UserMovedMap.CanExecute(bounds))
                {
                    UserMovedMap.Execute(bounds);
                }

                    }).DisposeWith(_subscriptions);


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

        public AddressSelectionMode AddressSelectionMode { get; set;}

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

        private void CreatePins()
        {
            if (_pickupPin == null)
            {
                _pickupPin = Map.AddMarker(new MarkerOptions()
                    .SetPosition(new LatLng(0, 0))
                    .Anchor(.5f, 1f)
                    .InvokeIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.hail_icon))
                    .Visible(false));
            }     

            if (_destinationPin == null)
            {
                _destinationPin = Map.AddMarker(new MarkerOptions()
                    .SetPosition(new LatLng(0, 0))
                    .Anchor(.5f, 1f)
                    .InvokeIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.destination_icon))
                    .Visible(false));
            }     
        }

        private void OnPickupAddressChanged()
        {
            if (PickupAddress == null)
                return; 
        }

        private void OnDestinationAddressChanged()
        {
            if (DestinationAddress == null)
                return; 

            if (DestinationAddress.HasValidCoordinate())
            {
                _destinationPin.Visible = true;
                _destinationPin.Position = new LatLng(DestinationAddress.Latitude, DestinationAddress.Longitude);
            }
            else
            {
                _destinationPin.Visible = false;
            }
        }

        private bool bypassCameraChangeEvent = false;

        void StickPickupPinAt(Position position)
        {
            _pickupOverlay.Visibility = ViewStates.Invisible;
            _pickupPin.Visible = true;
            _pickupPin.Position = new LatLng(position.Latitude, position.Longitude);
        }

        void ShowPickupOverlay(Position position)
        {
            _pickupPin.Visible = false;
            _pickupOverlay.Visibility = ViewStates.Visible;
        }

        private void OnMapBoundsChanged()
        {
            if (MapBounds != null)
            {
                bypassCameraChangeEvent = true;
                var _bounds = MapBounds;
                Map.AnimateCamera(
                    CameraUpdateFactory.NewLatLngBounds(
                        new LatLngBounds(new LatLng(_bounds.SouthBound, _bounds.WestBound), 
                            new LatLng(_bounds.NorthBound, _bounds.EastBound)),
                        DrawHelper.GetPixels(0)));
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
                                  .InvokeIcon(BitmapDescriptorFactory.FromResource(isCluster ? Resource.Drawable.cluster : Resource.Drawable.nearby_taxi))
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