using System;
using System.Collections.Generic;
using CoreGraphics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Input;
using Cirrious.MvvmCross.Binding.BindingContext;
using CoreLocation;
using Foundation;
using MapKit;
using UIKit;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using MapBounds = apcurium.MK.Booking.Maps.Geo.MapBounds;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Helper;
using apcurium.MK.Booking.Mobile.Client.MapUtitilties;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using System.ComponentModel;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Common;
using TinyIoC;
using apcurium.MK.Common.Configuration;

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

        private bool _useThemeColorForPickupAndDestinationMapIcons;
        private bool _showAssignedVehicleNumberOnPin;

        public OrderMapView(IntPtr handle) :base(handle)
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
             
			this.RegionChanged += (s, e) => 
			{
				ShowAvailableVehicles (VehicleClusterHelper.Clusterize(AvailableVehicles != null ? AvailableVehicles.ToArray () : null, GetMapBoundsFromProjection()));
			};
        }

        public MapViewModel ViewModel
        {
            get
            {
                return (MapViewModel)DataContext;
            }
        }

        public override void Draw(CGRect rect)
        {
            base.Draw(rect);
            InitOverlays();
        }

        private void Initialize()
        {
            var settings = TinyIoCContainer.Current.Resolve<IAppSettings> ().Data;
            _showAssignedVehicleNumberOnPin = settings.ShowAssignedVehicleNumberOnPin;
            _useThemeColorForPickupAndDestinationMapIcons = this.Services().Settings.UseThemeColorForMapIcons;

            this.DelayBind(() => 
            {
                var set = this.CreateBindingSet<OrderMapView, MapViewModel>();

                set.Bind()
                    .For(v => v.PickupAddress)
                    .To(vm => vm.PickupAddress);

                set.Bind()
                    .For(v => v.DestinationAddress)
                    .To(vm => vm.DestinationAddress);

                set.Bind()
                    .For(v => v.AddressSelectionMode)
                    .To(vm => vm.AddressSelectionMode);

                set.Bind()
                    .For("AvailableVehicles")
                    .To(vm => vm.AvailableVehicles);

                set.Bind()
                   .For("IsMapDisabled")
                   .To(vm => vm.IsMapDisabled);

                set.Apply();
            });

            _pickupAnnotation = GetAnnotation(new CLLocationCoordinate2D(), AddressAnnotationType.Pickup, _useThemeColorForPickupAndDestinationMapIcons);
            _destinationAnnotation = GetAnnotation(new CLLocationCoordinate2D(), AddressAnnotationType.Destination, _useThemeColorForPickupAndDestinationMapIcons);

            InitializeGesture();
        }

        private void InitializeGesture()
        {
            // disable on map since we're handling gestures ourselves
            if (UIHelper.IsOS7orHigher)
            {
                this.PitchEnabled = false;
                this.RotateEnabled = false;
            }

            this.ZoomEnabled = false;

            if (_gesture == null)
            {
                _gesture = new TouchGesture();
                _gesture.TouchBegin += HandleTouchBegin;
                _gesture.TouchMove += HandleTouchMove;
                _gesture.TouchEndOrCancel += HandleTouchEnded;
                AddGestureRecognizer(_gesture);

                var pinchRecognizer = new UIPinchGestureRecognizer ();
                pinchRecognizer.ShouldRecognizeSimultaneously = (g1, g2) => !(g2 is UITapGestureRecognizer);
                pinchRecognizer.AddTarget (() => OnPinch (pinchRecognizer));
                AddGestureRecognizer (pinchRecognizer);

                var doubleTapRecognizer = new UITapGestureRecognizer ();
                doubleTapRecognizer.ShouldRecognizeSimultaneously = (g1, g2) => false;
                doubleTapRecognizer.NumberOfTapsRequired = 2;
                doubleTapRecognizer.AddTarget (() => this.ChangeZoomLevel(true));
                AddGestureRecognizer (doubleTapRecognizer);

                var doubleTapMultitouchRecognizer = new UITapGestureRecognizer ();
                doubleTapMultitouchRecognizer.ShouldRecognizeSimultaneously = (g1, g2) => false;
                doubleTapMultitouchRecognizer.NumberOfTapsRequired = 2;
                doubleTapMultitouchRecognizer.NumberOfTouchesRequired = 2;
                doubleTapMultitouchRecognizer.AddTarget (() => this.ChangeZoomLevel(false));
                AddGestureRecognizer (doubleTapMultitouchRecognizer);
            }
        }

        private MKCoordinateSpan _span;
        private void OnPinch (UIPinchGestureRecognizer sender)
        {
            if (sender.State == UIGestureRecognizerState.Began)
            {
                _span = this.Region.Span;
            }

            this.ChangeRegionSpanDependingOnPinchScale (_span, sender.Scale);
        }

        public AddressAnnotation GetAnnotation(CLLocationCoordinate2D coordinates, AddressAnnotationType addressType, bool useThemeColorForPickupAndDestinationMapIcons)
        {
            return new AddressAnnotation(coordinates,
                addressType,
                string.Empty,
                string.Empty,
                useThemeColorForPickupAndDestinationMapIcons,
				false);
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
            get { return _addressSelectionMode; }
            set
            {
                _addressSelectionMode = value;
                ShowMarkers();
            }
        }

        private IEnumerable<AvailableVehicle> _availableVehicles = new List<AvailableVehicle>();
        public IEnumerable<AvailableVehicle> AvailableVehicles
        {
            get { return _availableVehicles; }
            set
            {
                if (_availableVehicles != value)
                {
                    _availableVehicles = value;
                    ShowAvailableVehicles (VehicleClusterHelper.Clusterize(value != null ? value.ToArray () : null, GetMapBoundsFromProjection()));
                }
            }
        }

        private bool _isMapDisabled;
        public bool IsMapDisabled
        {
            get { return _isMapDisabled; }
            set
            {
                _isMapDisabled = value;
                SetEnabled(!value);
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

        private void SetAnnotation(Address address, AddressAnnotation addressAnnotation, bool visible)
        {
            if (address.HasValidCoordinate() && visible)
            {
                RemoveAnnotation(addressAnnotation);
                addressAnnotation.SetCoordinate(address.GetCoordinate());
                AddAnnotation(addressAnnotation);
            }
            else
            {
                RemoveAnnotation (addressAnnotation);
            }
        }

        private void SetOverlay(UIImageView overlay, bool visible)
        {
            overlay.Hidden = !visible;
        }

        private void InitOverlays()
        {
            var pinSize = _pickupCenterPin.IntrinsicContentSize;
			var mkMapPadding = 12f;

            _pickupCenterPin.Frame = 
                _dropoffCenterPin.Frame = 
					new CGRect((this.Bounds.Width - pinSize.Width) / 2, (this.Bounds.Height / 2) - pinSize.Height + mkMapPadding, pinSize.Width, pinSize.Height);

            if (_pickupCenterPin.Superview == null)
            {
                AddSubviews(_pickupCenterPin, _dropoffCenterPin);
                GetViewForAnnotation = MKMapViewHelper.GetViewForAnnotation;
            }
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            // change position of Legal link on map
            var legalView = Subviews.FirstOrDefault(x => x is UILabel);
            if (legalView != null)
            {
                var leftMargin = 8;
                var bottomMargin = 13;
                var menuButtonWidth = 39;
                legalView
                    .SetX(leftMargin + menuButtonWidth + 15)
                    .SetY(this.Frame.Bottom - legalView.Frame.Height - bottomMargin); 
            }
        }

        private void ShowMarkers()
        {
            if (AddressSelectionMode == AddressSelectionMode.DropoffSelection)
            {
				SetAnnotation (DestinationAddress, _destinationAnnotation, false);
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
                if (((HomeViewModel)ViewModel.Parent).CurrentViewState == HomeViewModelState.BookingStatus)
                {
                    // Hide movable pickup pin
                    SetAnnotation(PickupAddress, _pickupAnnotation, false);
                    SetOverlay(_pickupCenterPin, false);

                    // Show static pickup pic
                    SetAnnotation(PickupAddress, _pickupAnnotation, true);
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
        }
                     
        private void HandleTouchBegin (object sender, EventArgs e)
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

		private MKCoordinateRegion GetRegionFromMapBounds(MapBounds bounds)
		{
			return new MKCoordinateRegion (
				new CLLocationCoordinate2D (bounds.GetCenter().Latitude, bounds.GetCenter().Longitude),
				new MKCoordinateSpan (bounds.LatitudeDelta, bounds.LongitudeDelta));
		}

        private void HandleTouchMove (object sender, EventArgs e)
        {
            CancelAddressSearch();
        }

        private void HandleTouchEnded(object sender, EventArgs e)
        {
            _userMovedMapSubsciption.Disposable = Observable.FromEventPattern<MKMapViewChangeEventArgs>(eh =>  this.RegionChanged += eh, eh => this.RegionChanged -= eh)
                .Throttle(TimeSpan.FromMilliseconds(1000))
                .Take(1)
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(ep =>
            {
                var bounds = GetMapBoundsFromProjection();
                
                ViewModel.UserMovedMap.ExecuteIfPossible(bounds);                
            });
        }

        private void ClearAllAnnotations()
        {
            foreach (var vehicleAnnotation in _availableVehicleAnnotations)
            {
                RemoveAnnotation(vehicleAnnotation);
            }

            _availableVehicleAnnotations.Clear ();
        }

        private void DeleteAnnotation(AddressAnnotation annotationToRemove)
        {
            RemoveAnnotation(annotationToRemove);
            _availableVehicleAnnotations.Remove (annotationToRemove);
        }

        private void CreateAnnotation(AvailableVehicle vehicle)
        {
            var annotationType = (vehicle is AvailableVehicleCluster) 
                ? AddressAnnotationType.NearbyTaxiCluster 
                : AddressAnnotationType.NearbyTaxi;

                var vehicleAnnotation = new AddressAnnotation (
                                new CLLocationCoordinate2D(vehicle.Latitude, vehicle.Longitude),
                                annotationType, 
								vehicle.VehicleNumber.ToString(),             
                                string.Empty, 
                                _useThemeColorForPickupAndDestinationMapIcons,
								false,
                                vehicle.LogoName);

            AddAnnotation (vehicleAnnotation);
            _availableVehicleAnnotations.Add (vehicleAnnotation);
        }

        private void ShowAvailableVehicles(IEnumerable<AvailableVehicle> vehicles)
        {
            if (vehicles == null)
            {
                ClearAllAnnotations ();
                return;
            }

            var vehicleNumbersToBeShown = vehicles.Select (x => x.VehicleNumber.ToString());

            // check for annotations that needs to be removed
            var annotationsToRemove = _availableVehicleAnnotations.Where(x => !vehicleNumbersToBeShown.Contains(x.Title)).ToList();
            foreach (var annotation in annotationsToRemove)
            {
                DeleteAnnotation(annotation);
            }

            // check for updated or new
            foreach (var vehicle in vehicles)
            {
                var existingAnnotationForVehicle = _availableVehicleAnnotations.FirstOrDefault (x => x.Title == vehicle.VehicleNumber.ToString());
                if (existingAnnotationForVehicle != null)
                {
                    if (existingAnnotationForVehicle.Coordinate.Latitude == vehicle.Latitude && existingAnnotationForVehicle.Coordinate.Longitude == vehicle.Longitude)
                    {
                        // vehicle not updated, nothing to do
                        continue;
                    }

                    // coordinates were updated, remove and add later with new position
                    DeleteAnnotation (existingAnnotationForVehicle);
                }

                CreateAnnotation (vehicle);
            }
        }

        public void SetEnabled(bool enabled)
        {
            this.ScrollEnabled = enabled;
            this.UserInteractionEnabled = enabled;                       

            if (_mapBlurOverlay == null)
            {
                var _size = this.Bounds.Size;
                _mapBlurOverlay = new UIImageView(new CGRect(new CGPoint(0, 0), new CGSize(_size.Width, _size.Height)));
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

			InitOverlays ();
        }

        private void CancelAddressSearch()
        {
            ((HomeViewModel)(ViewModel.Parent)).LocateMe.Cancel();
            ((HomeViewModel)(ViewModel.Parent)).AutomaticLocateMeAtPickup.Cancel();
            ViewModel.UserMovedMap.Cancel();
            _userMovedMapSubsciption.Disposable = null;
        }

        public void ChangePresentation(ChangePresentationHint hint)
        {
            var streetLevelZoomHint = hint as ZoomToStreetLevelPresentationHint;
			if (streetLevelZoomHint != null)
            {
                var zoomLevel = streetLevelZoomHint.InitialZoom 
                    ? this.Services().Settings.InitialZoomLevel 
                    : MapViewModel.ZoomStreetLevel;

                if (this.Services().Settings.DisableAutomaticZoomOnLocation && !streetLevelZoomHint.InitialZoom)
                {
                    this.SetCenterCoordinate(new CLLocationCoordinate2D(streetLevelZoomHint.Latitude, streetLevelZoomHint.Longitude), true);
                }
                else
                {
                    this.SetCenterCoordinate(new CLLocationCoordinate2D(streetLevelZoomHint.Latitude, streetLevelZoomHint.Longitude), zoomLevel, true);
                }
            }

			var zoomHint = hint as ChangeZoomPresentationHint;
			if (zoomHint != null) 
			{
				var newBounds = zoomHint.Bounds;
				var currentBounds = this.GetMapBoundsFromProjection();

				if (Math.Abs(currentBounds.LongitudeDelta) <= Math.Abs(newBounds.LongitudeDelta))
				{
					this.SetRegion(GetRegionFromMapBounds(newBounds), true);
				}
			}

            var centerHint = hint as CenterMapPresentationHint;
            if (centerHint != null)
            {
                // Set the new region center, but keep current span                
                this.SetRegion(new MKCoordinateRegion(new CLLocationCoordinate2D(centerHint.Latitude, centerHint.Longitude), Region.Span), true);
            }
        }

        private MKAnnotation _taxiLocationPin;

        private OrderStatusDetail _taxiLocation;
        public OrderStatusDetail TaxiLocation
        {
            get { return _taxiLocation; }
            set
            {
                _taxiLocation = value;
                if (_taxiLocationPin != null)
                {
                    RemoveAnnotation(_taxiLocationPin);
                    _taxiLocationPin = null;
                }

                if ((value != null))
                {
                    var coord = new CLLocationCoordinate2D(0,0);            
                    if (value.VehicleLatitude.HasValue
                        && value.VehicleLongitude.HasValue
                        && value.VehicleLongitude.Value != 0
                        && value.VehicleLatitude.Value != 0
                        && !string.IsNullOrEmpty(value.VehicleNumber) 
                        && VehicleStatuses.ShowOnMapStatuses.Contains(value.IBSStatusId))
                    {
                        coord = new CLLocationCoordinate2D(value.VehicleLatitude.Value, value.VehicleLongitude.Value);
                    }
                    _taxiLocationPin = new AddressAnnotation(coord, AddressAnnotationType.Taxi, Localize.GetValue("TaxiMapTitle"), value.VehicleNumber, _useThemeColorForPickupAndDestinationMapIcons, _showAssignedVehicleNumberOnPin);
                    AddAnnotation(_taxiLocationPin);
                }
                SetNeedsDisplay();
            }
        }
    }
}