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
        private View _view;
        private Marker _pickupPin;
        private Marker _destinationPin;
        private CompositeDisposable _subscriptions = new CompositeDisposable();

        private List<Marker> _availableVehicleMarkers = new List<Marker> ();

        public OrderMapFragment(SupportMapFragment _map)
        {
            this.CreateBindingContext();
            Map = _map.Map;
            _view = _map.View;
            this.DelayBind(() => InitializeBinding());
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

            _view.Touch += (object sender, View.TouchEventArgs e) => 
            {
                var _target = Map.CameraPosition.Target;
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

                var _bounds =  Map.Projection.VisibleRegion.LatLngBounds;

                var bounds = new MapBounds()
                { 
                    SouthBound = _bounds.Southwest.Latitude, 
                    WestBound = _bounds.Southwest.Longitude, 
                    NorthBound = _bounds.Northeast.Latitude, 
                    EastBound = _bounds.Northeast.Longitude
                };

                if(UserMovedMap != null && UserMovedMap.CanExecute(bounds))
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

        public event EventHandler UserMapBoundsChanged;

        private MapBounds _userMapBounds;
        public MapBounds UserMapBounds
        {
            get
            {
                return _userMapBounds;
            }

            set
            {
                if (_userMapBounds != value)
                {
                    _userMapBounds = value;

                    if (AddressSelectionMode == AddressSelectionMode.PickupSelection)
                    {
                        CreatePins();
                        _pickupPin.Visible = true;
                        _pickupPin.Position = new LatLng(_userMapBounds.GetCenter().Latitude, _userMapBounds.GetCenter().Longitude);
                    }
                }
            }
        }

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

            CreatePins();

            if (AddressSelectionMode == AddressSelectionMode.PickupSelection)
            {
                _pickupPin.Visible = true;
                _pickupPin.Position = new LatLng(MapBounds.GetCenter().Latitude, MapBounds.GetCenter().Longitude);
            }
            else
            {
                if (PickupAddress.HasValidCoordinate())
                {
                    _pickupPin.Visible = true;
                    _pickupPin.Position = new LatLng(PickupAddress.Latitude, PickupAddress.Longitude);
                }
                else
                {
                    _pickupPin.Visible = false;
                }
            }
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

        private void OnMapBoundsChanged()
        {
            if (MapBounds != null)
            {
                bypassCameraChangeEvent = true;
                Map.AnimateCamera(
                    CameraUpdateFactory.NewLatLngBounds(
                        new LatLngBounds(new LatLng(MapBounds.SouthBound, MapBounds.WestBound), 
                            new LatLng(MapBounds.NorthBound, MapBounds.EastBound)),
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