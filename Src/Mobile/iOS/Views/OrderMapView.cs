using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Input;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Maps.Geo;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Diagnostics;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using apcurium.MK.Booking.Mobile.Client.Helper;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Client.MapUtitilties;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Booking.Mobile.ViewModels.Map;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using Cirrious.MvvmCross.Binding.BindingContext;
using CoreGraphics;
using CoreLocation;
using Foundation;
using MapKit;
using TinyIoC;
using UIKit;
using apcurium.MK.Booking.Mobile.Infrastructure;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    [Register("OrderMapView")]
    public class OrderMapView: BindableMapView, IChangePresentation
    {
        private AddressAnnotation _pickupAnnotation;
        private AddressAnnotation _destinationAnnotation;
        private readonly UIImageView _pickupCenterPin;
        private readonly UIImageView _dropoffCenterPin;
        private UIImageView _mapBlurOverlay;
        private readonly List<AddressAnnotation> _availableVehicleAnnotations = new List<AddressAnnotation> ();
        private TouchGesture _gesture;
        private readonly SerialDisposable _userMovedMapSubsciption = new SerialDisposable();

        private bool _useThemeColorForPickupAndDestinationMapIcons;
        private bool _showAssignedVehicleNumberOnPin;
        private bool _automatedMapChanged;

        public OrderMapView(IntPtr handle) :base(handle)
        {
            Initialize();

            _dropoffCenterPin = new UIImageView(AddressAnnotation.GetImage(AddressAnnotationType.Destination))
            {
                BackgroundColor = UIColor.Clear,
                ContentMode = UIViewContentMode.Center,
                Hidden = true
            };

            _pickupCenterPin = new UIImageView(AddressAnnotation.GetImage(AddressAnnotationType.Pickup))
            {
                BackgroundColor = UIColor.Clear,
                ContentMode = UIViewContentMode.Center,
                Hidden = true
            };
             
			RegionChanged += (s, e) =>
			{

				var canShowClusterizedTaxiMarker = ViewModel != null && !ViewModel.Settings.ShowIndividualTaxiMarkerOnly;

				if (canShowClusterizedTaxiMarker && CanShowAvailableVehicle())
                {
                    ShowAvailableVehicles(VehicleClusterHelper.Clusterize(AvailableVehicles != null ? AvailableVehicles.ToArray() : null, GetMapBoundsFromProjection()));
                }

                if (TaxiLocation != null && !_automatedMapChanged)
                {
                    CancelAutoFollow.ExecuteIfPossible();
                }
                else if(_automatedMapChanged)
                {
                    _automatedMapChanged = false;
                }

			};
        }

	    private bool CanShowAvailableVehicle()
	    {
		    if (_orderStatusDetail == null)
		    {
			    return true;
		    }

		    return _orderStatusDetail.IBSStatusId != VehicleStatuses.Common.Assigned
		           && _orderStatusDetail.IBSStatusId != VehicleStatuses.Common.Arrived
		           && _orderStatusDetail.IBSStatusId != VehicleStatuses.Common.Loaded
		           && _orderStatusDetail.IBSStatusId != VehicleStatuses.Common.MeterOffNotPayed
		           && _orderStatusDetail.IBSStatusId != VehicleStatuses.Common.Done;
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
            var settings = TinyIoCContainer.Current.Resolve<IAppSettings>().Data;
            _showAssignedVehicleNumberOnPin = settings.ShowAssignedVehicleNumberOnPin;
            _useThemeColorForPickupAndDestinationMapIcons = this.Services().Settings.UseThemeColorForMapIcons;

            var coordonates = new[] 
            {
                    CoordinateViewModel.Create(settings.UpperRightLatitude??0, settings.UpperRightLongitude??0, true),
                    CoordinateViewModel.Create(settings.LowerLeftLatitude??0, settings.LowerLeftLongitude??0, true),
            };
            


            if (!coordonates.Any(p => p.Coordinate.Latitude == 0 || p.Coordinate.Longitude == 0))
            {
                var minLat = coordonates.Min(a => a.Coordinate.Latitude);
                var maxLat = coordonates.Max(a => a.Coordinate.Latitude);
                var minLon = coordonates.Min(a => a.Coordinate.Longitude);
                var maxLon = coordonates.Max(a => a.Coordinate.Longitude);

                var center = new CLLocationCoordinate2D(((maxLat + minLat) / 2), (maxLon + minLon) / 2);

                var region = new MKCoordinateRegion(center, new MKCoordinateSpan(0.1, 0.1));

                Region = region;
            }



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

			// Show glowing blue dot
			ShowsUserLocation = true;
        }

        private void InitializeGesture()
        {
            // disable on map since we're handling gestures ourselves
            if (UIHelper.IsOS7orHigher)
            {
                PitchEnabled = false;
                RotateEnabled = false;
            }

            ZoomEnabled = false;

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
                _span = Region.Span;
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
                    _availableVehicles = ViewModel.Settings.ShowIndividualTaxiMarkerOnly
                        ? value
                        : VehicleClusterHelper.Clusterize(value != null ? value.ToArray() : null, GetMapBoundsFromProjection());

                    ShowAvailableVehicles(CanShowAvailableVehicle() ? _availableVehicles : new AvailableVehicle[0]);
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

                if (((HomeViewModel)ViewModel.Parent).CurrentViewState == HomeViewModelState.BookingStatus)
                {
                    // Hide movable pickup pin
                    SetAnnotation(PickupAddress, _pickupAnnotation, false);
                    SetOverlay(_pickupCenterPin, false);

                    // Hide movable dropoff pin
                    SetAnnotation(PickupAddress, _destinationAnnotation, false);
                    SetOverlay(_dropoffCenterPin, false);

                    // Show static pickup pic
                    SetAnnotation(PickupAddress, _pickupAnnotation, true);

                    // Show static dropoff pic
                    SetAnnotation(DestinationAddress, _destinationAnnotation, true);
                }
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
					new CGRect((Bounds.Width - pinSize.Width) / 2, (Bounds.Height / 2) - pinSize.Height + mkMapPadding, pinSize.Width, pinSize.Height);

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
                    .SetY(Frame.Bottom - legalView.Frame.Height - bottomMargin); 
            }
        }

	    private void ShowMarkers()
	    {
		    if (AddressSelectionMode == AddressSelectionMode.DropoffSelection)
		    {
                var currentState = ((HomeViewModel)ViewModel.Parent).CurrentViewState;

                if (currentState == HomeViewModelState.BookingStatus || currentState == HomeViewModelState.ManualRidelinq)
                {
                    return;

                }

			    SetAnnotation(DestinationAddress, _destinationAnnotation, false);
			    SetOverlay(_pickupCenterPin, false);
			    SetOverlay(_dropoffCenterPin, true);

			    SetAnnotation(PickupAddress, _pickupAnnotation, PickupAddress.HasValidCoordinate());
		    }
		    else if (AddressSelectionMode == AddressSelectionMode.PickupSelection)
		    {
                var currentState = ((HomeViewModel)ViewModel.Parent).CurrentViewState;

                if (currentState == HomeViewModelState.BookingStatus || currentState == HomeViewModelState.ManualRidelinq)
			    {
                    // Don't display movable pickup or dropoff pins
                    return;
			    }

                SetAnnotation(PickupAddress, _pickupAnnotation, false);
                SetOverlay(_dropoffCenterPin, false);
                SetOverlay(_pickupCenterPin, true);

                SetAnnotation(DestinationAddress, _destinationAnnotation, DestinationAddress.HasValidCoordinate());
			}
		    else
		    {
			    SetOverlay(_pickupCenterPin, false);
			    SetOverlay(_dropoffCenterPin, false);

			    SetAnnotation(DestinationAddress, _destinationAnnotation, DestinationAddress.HasValidCoordinate());

			    SetAnnotation(PickupAddress, _pickupAnnotation, PickupAddress.HasValidCoordinate());
		    }
	    }

	    private void HandleTouchBegin (object sender, EventArgs e)
        {
            CancelAddressSearch();
        }

        private MapBounds GetMapBoundsFromProjection()
        {
            var bounds = new MapBounds
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
            ViewModel.BookCannotExecute = true;
            _userMovedMapSubsciption.Disposable = Observable
				.FromEventPattern<MKMapViewChangeEventArgs>(eh =>  RegionChanged += eh, eh => RegionChanged -= eh)
                .Throttle(TimeSpan.FromMilliseconds(1000))
                .Take(1)
                .ObserveOn(SynchronizationContext.Current)
				.Subscribe(ep => HandleUserMovedMap(), Logger.LogError);
        }

	    private void HandleUserMovedMap()
	    {
		    var bounds = GetMapBoundsFromProjection();

		    ViewModel.UserMovedMap.ExecuteIfPossible(bounds);
	    }

	    private void ClearAvailableVehiclesAnnotations()
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
								vehicle.VehicleNumber.ToString(CultureInfo.InvariantCulture),             
                                string.Empty, 
                                _useThemeColorForPickupAndDestinationMapIcons,
								false,
                                false,
                                vehicle.LogoName,
                                ViewModel.Settings.ShowOrientedPins,
                                vehicle.CompassCourse);
            
            vehicleAnnotation.HideMedaillonsCommand = new AsyncCommand(() =>
                {
                    foreach(var annotation in Annotations)
                    {
                        if(annotation != vehicleAnnotation)
                        {
                            var annotationView = ViewForAnnotation(annotation);
                            var pinAnnotationView = annotationView as PinAnnotationView;
                            if(pinAnnotationView != null)
                            {
                                pinAnnotationView.HideMedaillon();
                            }
                        }
                    }
                });
            
            AddAnnotation (vehicleAnnotation);
            _availableVehicleAnnotations.Add (vehicleAnnotation);
        }

        // Animate Annotation on the map between retrieving positions
        private void AnimateAnnotationOnMap(AddressAnnotation annotationToUpdate, Position newPosition)
        {
            var annotationToUpdateView = ViewForAnnotation(annotationToUpdate) as PinAnnotationView;
            annotationToUpdateView.RefreshPinImage();

            Animate(5, 0, UIViewAnimationOptions.CurveLinear, () =>
                {
                    annotationToUpdate.SetCoordinate(new CLLocationCoordinate2D(newPosition.Latitude, newPosition.Longitude));
                }, () => {});
        }

        // Update Annotation and Animate it to see it move on the map
        private void UpdateAnnotation(AddressAnnotation annotationToUpdate, AvailableVehicle vehicle)
        {
            var annotationType = (vehicle is AvailableVehicleCluster) 
                ? AddressAnnotationType.NearbyTaxiCluster 
                : AddressAnnotationType.NearbyTaxi;

            annotationToUpdate.Degrees = ViewModel.Settings.ShowOrientedPins 
                                            ? vehicle.CompassCourse
                                            : 0;
            annotationToUpdate.AddressType = annotationType;

            AnimateAnnotationOnMap(annotationToUpdate, new Position()
                {
                    Latitude = vehicle.Latitude,
                    Longitude = vehicle.Longitude
                });
        }

        private void ShowAvailableVehicles(IEnumerable<AvailableVehicle> vehicles)
        {
            if (vehicles == null)
            {
                ClearAvailableVehiclesAnnotations();
                return;
            }

	        var vehiclesArray = vehicles.ToArray();

            var vehicleNumbersToBeShown = vehiclesArray.Select (x => x.VehicleNumber.ToString(CultureInfo.InvariantCulture));

            // check for annotations that needs to be removed
            var annotationsToRemove = _availableVehicleAnnotations.Where(x => !vehicleNumbersToBeShown.Contains(x.Title)).ToList();
            foreach (var annotation in annotationsToRemove)
            {
                DeleteAnnotation(annotation);
            }

            // check for updated or new
			foreach (var vehicle in vehiclesArray)
            {
                var existingAnnotationForVehicle = _availableVehicleAnnotations.FirstOrDefault (x => x.Title == vehicle.VehicleNumber.ToString(CultureInfo.InvariantCulture));
                if (existingAnnotationForVehicle != null)
                {
                    if (existingAnnotationForVehicle.Coordinate.Latitude == vehicle.Latitude && existingAnnotationForVehicle.Coordinate.Longitude == vehicle.Longitude)
                    {
                        // vehicle not updated, nothing to do
                        continue;
                    }

                    // coordinates were updated, remove and add later with new position
                    UpdateAnnotation(existingAnnotationForVehicle, vehicle);
                }
                else
                {
                    CreateAnnotation(vehicle);
                }
            }
        }

        private void SetEnabled(bool enabled)
        {
            ScrollEnabled = enabled;
            UserInteractionEnabled = enabled;                       

            if (_mapBlurOverlay == null)
            {
                var size = UIScreen.MainScreen.Bounds.Size;
	            _mapBlurOverlay = new UIImageView(new CGRect(new CGPoint(0, 0), new CGSize(size.Width, size.Height)))
	            {
		            ContentMode = UIViewContentMode.ScaleToFill
	            };
	            AddSubview(_mapBlurOverlay);
            }

            if (!enabled)
            {
                _mapBlurOverlay.Image = ImageHelper.CreateBlurImageFromView(this);    

                _mapBlurOverlay.Alpha = 0;
                _mapBlurOverlay.Hidden = false;
                Animate(0.3f, () => _mapBlurOverlay.Alpha = 1);       
            }
            else
            {
                Animate(0.3f, () => _mapBlurOverlay.Alpha = 0, () => _mapBlurOverlay.Hidden = true);
            }

			InitOverlays ();
        }

        private void CancelAddressSearch()
        {
            if (!((HomeViewModel)ViewModel.Parent).FirstTime)
            {
                ((HomeViewModel)ViewModel.Parent).AutomaticLocateMeAtPickup.Cancel();
            }
            ((HomeViewModel)ViewModel.Parent).LocateMe.Cancel();
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
                    SetCenterCoordinate(new CLLocationCoordinate2D(streetLevelZoomHint.Latitude, streetLevelZoomHint.Longitude), true);
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
				var currentBounds = GetMapBoundsFromProjection();

				if (Math.Abs(currentBounds.LongitudeDelta) <= Math.Abs(newBounds.LongitudeDelta))
				{
					SetRegion(GetRegionFromMapBounds(newBounds), true);
				}
			}

            var centerHint = hint as CenterMapPresentationHint;
            if (centerHint != null)
            {
                // Set the new region center, but keep current span                
                SetRegion(new MKCoordinateRegion(new CLLocationCoordinate2D(centerHint.Latitude, centerHint.Longitude), Region.Span), true);
            }
        }

        #region OrderStatus

        private MKAnnotation _taxiLocationPin;

		private TaxiLocation _taxiLocation;
	    public TaxiLocation TaxiLocation
	    {
		    get { return _taxiLocation; }
		    set
		    {
			    _taxiLocation = value;
			    UpdateTaxiLocation(value);

                ClearAvailableVehiclesAnnotations();
		    }
	    }

        public ICommand CancelAutoFollow { get; set; }
        private void UpdateTaxiLocation(TaxiLocation value)
        {
            if (_taxiLocationPin != null && value == null)
            {
                RemoveAnnotation(_taxiLocationPin);
                _taxiLocationPin = null;

                return;
            }

            var showOrientedPins = ViewModel.Settings.ShowOrientedPins && value.CompassCourse.HasValue;

	        // Update Marker and Animate it to see it move on the map
            if (_taxiLocationPin != null && value.Longitude.HasValue && value.Latitude.HasValue)
            {
                var taxiLocationPin = (AddressAnnotation)_taxiLocationPin;
                if (showOrientedPins)
                {
                    taxiLocationPin.Degrees = value.CompassCourse.Value;
                }

                AnimateAnnotationOnMap(taxiLocationPin, new Position()
                    {
                        Latitude = value.Latitude.Value,
                        Longitude = value.Longitude.Value
                    });

	            return;
            }

            // Create Marker the first time
            var coord = new CLLocationCoordinate2D(0, 0);

            var vehicleLatitude = value.Latitude ?? 0;
            var vehicleLongitude = value.Longitude ?? 0;

            if (vehicleLatitude != 0
                && vehicleLongitude != 0
                && value.VehicleNumber.HasValue())
            {
                // Refresh vehicle position
                coord = new CLLocationCoordinate2D(vehicleLatitude, vehicleLongitude);
            }

            _taxiLocationPin = new AddressAnnotation(
                coord, 
                AddressAnnotationType.Taxi,
                Localize.GetValue("TaxiMapTitle"), 
                value.VehicleNumber, 
                _useThemeColorForPickupAndDestinationMapIcons, 
                _showAssignedVehicleNumberOnPin,
                true,
                null,
                showOrientedPins,
                value.CompassCourse??0);

            AddAnnotation(_taxiLocationPin);
            SetNeedsDisplay();



        }


	    private OrderStatusDetail _orderStatusDetail;
        public OrderStatusDetail OrderStatusDetail
        {
            get { return _orderStatusDetail; }
            set
            {
                _orderStatusDetail = value;

				

				if (_orderStatusDetail.IBSStatusId == VehicleStatuses.Common.Loaded)
				{
					RemoveAnnotation(_pickupAnnotation);
				}

                if (_orderStatusDetail.IBSStatusId == VehicleStatuses.Common.Assigned)
                {
                    ClearAvailableVehiclesAnnotations();
                }
            }
        }


        public Func<nfloat> OverlayOffsetProvider { get; set; }

        private IEnumerable<CoordinateViewModel> _center;
	    

	    public IEnumerable<CoordinateViewModel> MapCenter
        {
            get { return _center; }
            set
            {
                _center = value;                
                ShowPinsOnMap();                   
            }
        }

        private const float PinHeight = 75;
		private const float PinWidth = 95;
		private const float BottomPadding = 10;
		private const float RightPadding = 40;
		private const float LeftPadding = 40;

        private void ShowPinsOnMap()
        {
            var annotations = _center.ToArray();

            // There is nothing to do here
            if (annotations.None())
            {
                return;
            }

            if (annotations.Length == 1)
            {
                var lat = annotations[0].Coordinate.Latitude;
                var lon = annotations[0].Coordinate.Longitude;

                var region = new MKCoordinateRegion();

                if (annotations[0].Zoom == ZoomLevel.DontChange)
                {
                    region = Region;
                }

                region.Center = new CLLocationCoordinate2D(lat, lon);

                SetRegion(region, true);

                return;
            }

			var zoomRect = annotations
                .Select(coordinateViewModel => new CLLocationCoordinate2D(coordinateViewModel.Coordinate.Latitude, coordinateViewModel.Coordinate.Longitude))
                .Select(MKMapPoint.FromCoordinate)
				.Select(coord => new MKMapRect(coord.X, coord.Y, PinWidth, PinHeight))
				.Aggregate(MKMapRect.Null, MKMapRect.Union);

	        var overlayOffset = OverlayOffsetProvider != null
                ? OverlayOffsetProvider() + PinHeight
                : 0;

			SetVisibleMapRect(zoomRect, new UIEdgeInsets(overlayOffset, LeftPadding, BottomPadding, RightPadding), true);
        }

	    #endregion
    }
}