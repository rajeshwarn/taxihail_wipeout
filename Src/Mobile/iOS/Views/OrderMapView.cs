using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Cirrious.MvvmCross.Binding.BindingContext;
using MonoTouch.CoreLocation;
using MonoTouch.Foundation;
using MonoTouch.MapKit;
using MonoTouch.UIKit;
using TinyIoC;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.MapUtitilties;
using apcurium.MK.Booking.Mobile.Client.Controls;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Booking.Mobile.Client.Helper;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    [Register("OrderMapView")]
    public class OrderMapView: BindableMapView, IChangePresentation
    {
        private AddressAnnotation _pickupAnnotation;
        private AddressAnnotation _destinationAnnotation;
        private UIImageView _pickupCenterPin;
        private UIImageView _dropoffCenterPin;
        private UIImageView _mapBlurOverlay;
        private List<AddressAnnotation> _availableVehicleAnnotations = new List<AddressAnnotation> ();
        private TouchGesture _gesture;
        private readonly SerialDisposable _userMovedMapSubsciption = new SerialDisposable();

        public OrderMapView(IntPtr handle)
            :base(handle)
        {
            Initialize();

            _dropoffCenterPin = new UIImageView(AddressAnnotation.GetImage(AddressAnnotationType.Destination))
            {
                BackgroundColor = UIColor.Clear,
                ContentMode = UIViewContentMode.Center,
                Hidden = true,
            };
            _pickupCenterPin = new UIImageView(AddressAnnotation.GetImage(AddressAnnotationType.Pickup))
            {
                BackgroundColor = UIColor.Clear,
                ContentMode = UIViewContentMode.Center,
                Hidden = true,
            };

            AddSubviews(_pickupCenterPin, _dropoffCenterPin);
        }

        public override void Draw(RectangleF rect)
        {
            base.Draw(rect);
            InitOverlays();
        }

        private bool _useThemeColorForPickupAndDestinationMapIcons;

        private void Initialize()
        {
			_useThemeColorForPickupAndDestinationMapIcons = this.Services().Settings.UseThemeColorForMapIcons;

            this.DelayBind(() => {

                var set = this.CreateBindingSet<OrderMapView, MapViewModel>();

                set.Bind()
                    .For(v => v.PickupAddress)
                    .To(vm => vm.PickupAddress);

                set.Bind()
                    .For(v => v.DestinationAddress)
                    .To(vm => vm.DestinationAddress);

                set.Bind()
                    .For(v => v.UserMovedMap)
                    .To(vm => vm.UserMovedMap);

                set.Bind()
                    .For(v => v.AddressSelectionMode)
                    .To(vm => vm.AddressSelectionMode);

                set.Bind()
                    .For("AvailableVehicles")
                    .To(vm => vm.AvailableVehicles);

                set.Apply();

            });

            _pickupAnnotation = GetAnnotation(new CLLocationCoordinate2D(), AddressAnnotationType.Pickup, _useThemeColorForPickupAndDestinationMapIcons);
            _destinationAnnotation = GetAnnotation(new CLLocationCoordinate2D(), AddressAnnotationType.Destination, _useThemeColorForPickupAndDestinationMapIcons);

            // TODO: When refactoring StatusView, get rid of AddressMapDelegate
            // and use helper class to get GetViewForAnnotation delegate
            this.GetViewForAnnotation = new AddressMapDelegate().GetViewForAnnotation;

            InitializeGesture();
        }
                  

        private void InitializeGesture()
        {
            if (_gesture == null)
            {
                _gesture = new TouchGesture();              
                _gesture.TouchBegin += HandleTouchBegin;
                _gesture.TouchMove += HandleTouchMove;
                _gesture.TouchEndOrCancel += HandleTouchEnded;

                AddGestureRecognizer(_gesture);
            }
        }

        public AddressAnnotation GetAnnotation(CLLocationCoordinate2D coordinates, AddressAnnotationType addressType, bool useThemeColorForPickupAndDestinationMapIcons)
        {
            return new AddressAnnotation(coordinates,
                addressType,
                string.Empty,
                string.Empty,
                useThemeColorForPickupAndDestinationMapIcons);
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

        public IEnumerable<AvailableVehicle> AvailableVehicles
        {
            set
            {
                var mapBounds = GetMapBoundsFromProjection();
                ShowAvailableVehicles (VehicleClusterHelper.Clusterize(value != null ? value.ToArray() : null, mapBounds));
            }
        }

        private void OnPickupAddressChanged()
        {
            ShowMarkers();
        }

        private void OnDestinationAddressChanged()
        {
            ShowMarkers();
        }

        void SetAnnotation(Address address, AddressAnnotation addressAnnotation, bool visible)
        {
            if (address.HasValidCoordinate() && visible)
            {
                RemoveAnnotation(addressAnnotation);
                addressAnnotation.Coordinate = address.GetCoordinate();
                AddAnnotation(addressAnnotation);
            }
            else
            {
                RemoveAnnotation (addressAnnotation);
            }
        }

        void SetOverlay(UIImageView overlay, bool visible)
        {
            overlay.Hidden = !visible;
        }

        void InitOverlays()
        {
            var pinSize = _pickupCenterPin.IntrinsicContentSize;

            _pickupCenterPin.Frame = 
                _dropoffCenterPin.Frame = 
                    new RectangleF((this.Bounds.Width - pinSize.Width) / 2, (this.Bounds.Height - pinSize.Height) / 2, pinSize.Width, pinSize.Height);

            // change position of Legal link on map
            var legalView = Subviews.FirstOrDefault(x => x is UILabel);
            if (legalView != null)
            {
                var leftMargin = 8;
                var bottomMargin = 13;
                var menuButtonWidth = 39;
                legalView.SetX(leftMargin + menuButtonWidth + legalView.Frame.Width).SetY(this.Frame.Bottom - legalView.Frame.Height - bottomMargin); 
            }
        }

        void ShowMarkers()
        {
            if (AddressSelectionMode == AddressSelectionMode.DropoffSelection)
            {
                if (!DestinationAddress.HasValidCoordinate())
                {
                    SetAnnotation(DestinationAddress, _destinationAnnotation, false);
                }

                SetOverlay(_pickupCenterPin, false);
                SetOverlay(_dropoffCenterPin, true);

                if (PickupAddress.HasValidCoordinate())
                {
                    SetAnnotation(PickupAddress, _pickupAnnotation, true);
                }
                else
                {
                    SetAnnotation(PickupAddress, _pickupAnnotation, false);
                }
            }
            else
            {
                SetAnnotation(PickupAddress, _pickupAnnotation, false);
                SetOverlay(_dropoffCenterPin, false);
                SetOverlay(_pickupCenterPin, true);

                if (DestinationAddress.HasValidCoordinate())
                {
                    SetAnnotation(DestinationAddress, _destinationAnnotation, true);
                }
                else
                {
                    SetAnnotation(DestinationAddress, _destinationAnnotation, false);
                }
            }            
        }

        public ICommand UserMovedMap { get; set; }
                      

        void HandleTouchBegin (object sender, EventArgs e)
        {
            CancelAddressSearch();
        }

        private MapBounds GetMapBoundsFromProjection()
        {
            var bounds = new MapBounds()
            { 
                NorthBound = Region.Center.Latitude + (Region.Span.LatitudeDelta / 2), 
                SouthBound = Region.Center.Latitude - (Region.Span.LatitudeDelta / 2), 
                EastBound = Region.Center.Longitude + (Region.Span.LongitudeDelta / 2),
                WestBound = Region.Center.Longitude - (Region.Span.LongitudeDelta / 2)
            };

            return bounds;
        }

        void HandleTouchMove (object sender, EventArgs e)
        {
            CancelAddressSearch();
        }

        void HandleTouchEnded(object sender, EventArgs e)
        {
            _userMovedMapSubsciption.Disposable = Observable.FromEventPattern<MKMapViewChangeEventArgs>(eh =>  this.RegionChanged += eh, eh => this.RegionChanged -= eh)
                .Throttle(TimeSpan.FromMilliseconds(1000))
                .Take(1)
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(ep =>
            {
                    var bounds = GetMapBoundsFromProjection();
                    if (UserMovedMap != null && UserMovedMap.CanExecute(bounds))
                    {
                        UserMovedMap.Execute(bounds);

                    }
            });
        }

        private void ShowAvailableVehicles(IEnumerable<AvailableVehicle> vehicles)
        {
            foreach (var vehicleAnnotation in _availableVehicleAnnotations)
            {
                RemoveAnnotation(vehicleAnnotation);
            }
            _availableVehicleAnnotations.Clear ();

            if (vehicles == null)
                return;

            foreach (var v in vehicles)
            {
                var annotationType = (v is AvailableVehicleCluster) 
                                     ? AddressAnnotationType.NearbyTaxiCluster 
                                     : AddressAnnotationType.NearbyTaxi;

                var vehicleAnnotation = new AddressAnnotation (
                                new CLLocationCoordinate2D(v.Latitude, v.Longitude),
                                annotationType, 
                                string.Empty, 
                                string.Empty, 
                                _useThemeColorForPickupAndDestinationMapIcons,
                                v.LogoName);

                AddAnnotation (vehicleAnnotation);
                _availableVehicleAnnotations.Add (vehicleAnnotation);
            }
        }


        public void SetEnabled(bool enabled)
        {
            if (this.ZoomEnabled == enabled)
            {
                // already in the good state, no need to change
                return;
            }

            this.ZoomEnabled = enabled;
            this.ScrollEnabled = enabled;
            this.UserInteractionEnabled = enabled;                       

            if (_mapBlurOverlay == null)
            {
                var _size = this.Bounds.Size;
                _mapBlurOverlay = new UIImageView(new RectangleF(new PointF(0, 0), new SizeF(_size.Width, _size.Height)));
                _mapBlurOverlay.ContentMode = UIViewContentMode.ScaleToFill;
                _mapBlurOverlay.Frame = this.Frame;
                this.AddSubview(_mapBlurOverlay);
            }

            if (!enabled)
            {
                _mapBlurOverlay.Image = ImageHelper.CreateBlurImageFromView(this);    

                _mapBlurOverlay.Alpha = 0;
                _mapBlurOverlay.Hidden = false;
                UIView.Animate(0.3f, () => _mapBlurOverlay.Alpha = 1);       
            }
            else
            {
                UIView.Animate(0.3f, () => _mapBlurOverlay.Alpha = 0, () => _mapBlurOverlay.Hidden = true);
            }
        }

        private void CancelAddressSearch()
        {
            ((MapViewModel.CancellableCommand<MapBounds>)UserMovedMap).Cancel();
            _userMovedMapSubsciption.Disposable = null;
        }

        void ChangeState(HomeViewModelPresentationHint hint)
        {
            switch (hint.State)
            {
                case HomeViewModelState.Initial:
                    SetEnabled(true);
                    break;
                default:
                    SetEnabled(false);
                    break;
            }
        }

        public void ChangePresentation(ChangePresentationHint hint)
        {
            if (hint is HomeViewModelPresentationHint)
            {
                ChangeState((HomeViewModelPresentationHint)hint);
            }
            var zoomHint = hint as ZoomToStreetLevelPresentationHint;
            if (zoomHint != null)
            {
				this.SetCenterCoordinate(new CLLocationCoordinate2D(zoomHint.Latitude, zoomHint.Longitude), 14, true); 
            }

            var centerHint = hint as CenterMapPresentationHint;
            if (centerHint != null)
            {
                // Set the new region center, but keep current span                
                SetRegion(new MKCoordinateRegion(new CLLocationCoordinate2D(centerHint.Latitude, centerHint.Longitude), Region.Span), true);
            }
        }
    }
}