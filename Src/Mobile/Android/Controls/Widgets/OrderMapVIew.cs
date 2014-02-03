using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Common.Entity;
//using apcurium.MK.Booking.Mobile.Client.MapUtitilties;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Util;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Localization;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class OrderMapView: BindableMapView
    {
        //private AddressAnnotation _pickupAnnotation;
        private readonly Stack<Action> _deferedMapActions = new Stack<Action>();
        private ImageView _pickupCenterPin;
        private Marker _pickupPin;
        //private ImageView _dropoffCenterPin;
        //private Marker _dropoffPin;
        private bool _mapReady;


        public OrderMapView(Context context)
            : base(context)
        {
            Initialize();
        }

        public OrderMapView(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            Initialize();
        }

        public OrderMapView(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {
            Initialize();
        }

        private void Initialize()
        {
            this.DelayBind(() => {

                var set = this.CreateBindingSet<OrderMapView, MapViewModel>();

                set.Bind()
                    .For(v => v.PickupAddress)
                        .To(vm => vm.PickupAddress);

                set.Bind()
                    .For(v => v.MapBounds)
                        .To(vm => vm.MapBounds);

                set.Apply();

            });


            //_pickupAnnotation = new AddressAnnotation(new CLLocationCoordinate2D(), AddressAnnotationType.Pickup, string.Empty, string.Empty);
        }

        public void SetMapReady()
        {
            _mapReady = true;
            while (_deferedMapActions.Count > 0)
            {
                _deferedMapActions.Pop().Invoke();
            }
        }

        public void Pause()
        {
          base.OnPause();
          _mapReady = false;
        }


        public class LayoutObserverForMap : Java.Lang.Object, ViewTreeObserver.IOnGlobalLayoutListener
        {
            private readonly OrderMapView _touchMap;

            public LayoutObserverForMap(OrderMapView touchMap)
            {
                _touchMap = touchMap;
            }

            public void OnGlobalLayout()
            {
                _touchMap.SetMapReady();
            }
        }

        private Address _pickupAddress;
        public Address PickupAddress
        {
            get { return _pickupAddress; }
            set
            { 
                _pickupAddress = value;
                DeferWhenMapReady(() =>
                {
                    //if (AddressSelectionMode == AddressSelectionMode.None)
                    {
                        OnPickupAddressChanged();
                    }
                });
            }
        }

        private void DeferWhenMapReady(Action action)
        {
            if (_mapReady)
            {
                action.Invoke();
            }
            else
            {
                _deferedMapActions.Push(action);
            }
        }

        private MapBounds _mapBounds;
        public MapBounds MapBounds
        {
            get
            {
                return _mapBounds;
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

        private void OnPickupAddressChanged()
        {
            if (PickupAddress.HasValidCoordinate())
            {
                //RemoveAnnotation(_pickupAnnotation);

                //_pickupAnnotation.Coordinate = PickupAddress.GetCoordinate();
         
                // TODO: Former Android address validation method is inside ShowPickupPin
                ShowPickupPin(PickupAddress);

                //AddAnnotation(_pickupAnnotation);
            }
            else
            {
                //RemoveAnnotation (_pickupAnnotation);
            }
        }

        private void OnMapBoundsChanged()
        {
            if (MapBounds != null)
            {
                var center = MapBounds.GetCenter();
                Map.AnimateCamera(CameraUpdateFactory.NewLatLng(new LatLng(center.Latitude, center.Longitude)));

                //SetRegion(new MKCoordinateRegion( new CLLocationCoordinate2D(center.Latitude, center.Longitude), new MKCoordinateSpan(MapBounds.LatitudeDelta, MapBounds.LongitudeDelta)), true);
            }
        }

        public void SetMapCenterPins(ImageView pickup, ImageView dropoff)
        {
            _pickupCenterPin = pickup;
            //_dropoffCenterPin = dropoff;

            //if (AddressSelectionMode == AddressSelectionMode.PickupSelection)
                _pickupCenterPin.Visibility = ViewStates.Visible;
            //if (AddressSelectionMode == AddressSelectionMode.DropoffSelection)
           //  _dropoffCenterPin.Visibility = ViewStates.Visible;
        }

        private void ShowPickupPin(Address address)
        {
            if (_pickupPin == null)
            {
                _pickupPin = Map.AddMarker(new MarkerOptions()
                                           .SetPosition(new LatLng(0, 0))
                                           .Anchor(.5f, 1f)
                                           .InvokeIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.pin_hail))
                                           .Visible(false));
            }

            if (address == null)
                return;

            if (address.Latitude != 0 && address.Longitude != 0)            
            {
                _pickupPin.Position = new LatLng(address.Latitude, address.Longitude);
                _pickupPin.Visible = true;
            }
            if (_pickupCenterPin != null) _pickupCenterPin.Visibility = ViewStates.Gone;
        }

    }
}


