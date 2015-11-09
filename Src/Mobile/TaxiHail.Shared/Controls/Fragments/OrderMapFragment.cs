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

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public partial class OrderMapFragment: IMvxBindable, IDisposable, IChangePresentation
    {
        public TouchableMap TouchableMap { get; set;}

        private ImageView _pickupOverlay;
        private ImageView _destinationOverlay;
        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();
	    private bool _bypassCameraChangeEvent;

		private IEnumerable<CoordinateViewModel> _center;


        private readonly Resources _resources;
		private readonly TaxiHailSetting _settings;

		private const int MapPadding = 60;

		private readonly bool _showVehicleNumber;

	    private bool _isBookingMode;

		private bool _lockGeocoding;
		private TaxiLocation _taxiLocation;
		private OrderStatusDetail _orderStatusDetail;

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

	    public TaxiLocation TaxiLocation
	    {
		    get { return _taxiLocation; }
		    set
		    {
				_taxiLocation = value;
			    UpdateTaxiLocation(value);
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
                if (_availableVehicles == null || (value != null && _availableVehicles.SequenceEqual(value)))
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

        public void Dispose()
        {
            _subscriptions.Dispose();
        }

        public Func<int> OverlayOffsetProvider { get; set; }
    }
}