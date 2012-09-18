using System;
using System.Linq;
using System.Threading;
using Android.App;
using Android.Content;
using Android.GoogleMaps;
using Android.OS;
using Android.Views;
using Android.Widget;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Client.MapUtitilties;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Client.Converters;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common;
using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.Client.Helpers;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "Book Status", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class BookingStatusActivity : MapActivity
    {
        private const string _doneStatus = "wosDONE";
        private const string _loadedStatus = "wosLOADED";
        
        private bool _closeScreenWhenCompleted;
        private Guid _lastOrder;
        private Timer _timer;
        private bool _isInit = false;

        public OrderStatusDetail OrderStatus { get; private set; }
        public Order Order { get; private set; }


        protected override bool IsRouteDisplayed
        {
            get { return false; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.BookingStatus);

            LoadParameters();

            if (OrderStatus.IBSOrderId.HasValue)
            {
                FindViewById<TextView>(Resource.Id.confirmationNo).Text = string.Format(GetString(Resource.String.StatusDescription), OrderStatus.IBSOrderId.Value);
            }

            SetStatusText(GetString(Resource.String.LoadingMessage));
            FindViewById<Button>(Resource.Id.CancelBtn).Enabled = true;
			FindViewById<Button>(Resource.Id.CancelBtn).Click += delegate {	CancelOrder(); };
			FindViewById<Button>(Resource.Id.CallBtn).Click += delegate { CallCompany(); };
			FindViewById<Button>(Resource.Id.NewRideBtn).Click += delegate { CloseActivity(); };

            var map = FindViewById<MapView>(Resource.Id.mapStatus);

            ThreadHelper.ExecuteInThread(this, () =>
                {
                    DisplayStatus(Order, OrderStatus);
                }, false);

        }

        protected override void OnResume()
        {
            base.OnResume();
            InitMap();
        }

        protected void InitMap()
        {
            if (_isInit)
            {
                return;
            }

            _isInit = true;

            var map = FindViewById<MapView>(Resource.Id.mapStatus);
            map.SetBuiltInZoomControls(false);
            map.Clickable = true;
            map.Traffic = false;
            map.Satellite = false;

        }
        protected override void OnStart()
        {
            base.OnStart();
            _timer = new Timer(o => RefreshStatus(), null, 0, 6000);

        }
        protected override void OnStop()
        {
            base.OnStop();
            try
            {
                _timer.Change(int.MaxValue, int.MaxValue);
                _timer.Dispose();
                _timer = null;
            }
            catch
            {

            }

        }

        private void LoadParameters()
        {
            var serialized = Intent.GetStringExtra("OrderStatusDetail");
            var status = SerializerHelper.DeserializeObject<OrderStatusDetail>(serialized);
            OrderStatus = status;

            serialized = Intent.GetStringExtra("Order");
            var order = SerializerHelper.DeserializeObject<Order>(serialized);
            Order = order;

        }

        private void AddMapPin(MapView map, Address loc, int graphic, int titleId)
        {
            if (loc.HasValidCoordinate())
            {
                var point = new GeoPoint(CoordinatesHelper.ConvertToE6(loc.Latitude), CoordinatesHelper.ConvertToE6(loc.Longitude));
                var pushpin = Resources.GetDrawable(graphic);
                var title = GetString(titleId);
                var pushpinOverlay = new PushPinOverlay(map, pushpin, title, point);
                map.Overlays.Add(pushpinOverlay);
            }
        }


        private void SetStatusText(string status)
        {
			FindViewById<TextView>(Resource.Id.statusInfoText).Text = string.Format(GetString(Resource.String.StatusStatusLabel), status);
        }

        private void CloseActivity()
        {
            _timer.Dispose();
            _timer = null;
            RunOnUiThread(() =>
                {                    
                    Finish();
                });
        }

        private void CallCompany()
        {

            Intent callIntent = new Intent(Intent.ActionCall);
            callIntent.SetData(Android.Net.Uri.Parse("tel:" + TinyIoCContainer.Current.Resolve<IAppSettings>().PhoneNumber(Order.Settings.ProviderId.Value )));
            StartActivity(callIntent);
        }

        private void CancelOrder()
        {

            if ((OrderStatus.IBSStatusId == _doneStatus) || (OrderStatus.IBSStatusId == _loadedStatus))
            {
                this.ShowAlert(Resource.String.CannotCancelOrderTitle, Resource.String.CannotCancelOrderMessage);                
                return;
            }
                    


            var newBooking = new Confirmation();
            newBooking.Action(this, Resource.String.StatusConfirmCancelRide, () =>
            {
                ThreadHelper.ExecuteInThread(this, () =>
                {
                    var isSuccess = TinyIoCContainer.Current.Resolve<IBookingService>().CancelOrder(Order.Id);

                    if (isSuccess)
                    {
                        CloseActivity();
                    }
                    else
                    {
                        RunOnUiThread(() => this.ShowAlert(Resource.String.StatusConfirmCancelRideErrorTitle, Resource.String.StatusConfirmCancelRideError));
                    }

                }, true);
            });
	
        }

        private void RefreshStatus()
        {
            ThreadHelper.ExecuteInThread(this, () =>
            {
                try
                {
                    var status = TinyIoCContainer.Current.Resolve<IBookingService>().GetOrderStatus(Order.Id);

                
                    _lastOrder = OrderStatus.OrderId;

                    if (status != null)
                    {                        
                        OrderStatus = status;
                        DisplayStatus(Order, status);
                    }

                }
                catch (Exception ex)
                {
                    TinyIoCContainer.Current.Resolve<ILogger>().LogError(ex);
                }
            }, false);
        }

        private void DisplayStatus(Order order, OrderStatusDetail status)
        {
            if (status.IBSStatusDescription.HasValue())
            {
                RunOnUiThread(() =>
                    {

                        SetStatusText(status.IBSStatusDescription);
                    });
            }


            RunOnUiThread(() =>
                {
                    FindViewById<TextView>(Resource.Id.confirmationNo).Text = string.Format(GetString(Resource.String.StatusDescription), OrderStatus.IBSOrderId.Value);

                    var map = FindViewById<MapView>(Resource.Id.mapStatus);
                    map.Overlays.Clear();
                    map.Invalidate();


                    if (status.VehicleLatitude.HasValue && status.VehicleLongitude.HasValue)
                    {
                        var point = new GeoPoint(CoordinatesHelper.ConvertToE6(status.VehicleLatitude.Value), CoordinatesHelper.ConvertToE6(status.VehicleLongitude.Value));
                        var taxiOverlay = Resources.GetDrawable(Resource.Drawable.taxi_label);
                        var title = GetString(Resource.String.TaxiMapTitle);
                        var pushpinOverlay = new TaxiOverlay(map, taxiOverlay, title, "#" + status.VehicleNumber , point);
                        map.Overlays.Add(pushpinOverlay);
                    }

                    AddMapPin(map, order.PickupAddress, Resource.Drawable.pin_green, Resource.String.PickupMapTitle);
                    AddMapPin(map, order.DropOffAddress, Resource.Drawable.pin_red, Resource.String.DestinationMapTitle);

                    map.Invalidate();

                    var adressesToDisplay = Params.Get<Address>(order.PickupAddress,  new Address { Longitude = status.VehicleLongitude.HasValue ? status.VehicleLongitude.Value : 0, Latitude = status.VehicleLatitude.HasValue ? status.VehicleLatitude.Value : 0 }).Where(a => a.HasValidCoordinate());
                    SetZoom(adressesToDisplay);

                });

        }

        private void SetZoom(IEnumerable<Address> adressesToDisplay)
        {
            var map = FindViewById<MapView>(Resource.Id.mapStatus);
            var mapController = map.Controller;

            if (adressesToDisplay.Count() == 1)
            {
                int lat = CoordinatesHelper.ConvertToE6(adressesToDisplay.ElementAt(0).Latitude);
                int lon = CoordinatesHelper.ConvertToE6(adressesToDisplay.ElementAt(0).Longitude);
                mapController.AnimateTo(new GeoPoint( lat, lon ));
                mapController.SetZoom(17);
                return;
            }


            int minLat = int.MaxValue;
            int maxLat = int.MinValue;
            int minLon = int.MaxValue;
            int maxLon = int.MinValue;

            foreach (var item in adressesToDisplay)
            {
                int lat = CoordinatesHelper.ConvertToE6(item.Latitude);
                int lon = CoordinatesHelper.ConvertToE6(item.Longitude);
                maxLat = Math.Max(lat, maxLat);
                minLat = Math.Min(lat, minLat);
                maxLon = Math.Max(lon, maxLon);
                minLon = Math.Min(lon, minLon);
            }

            if ((Math.Abs(maxLat - minLat) < 0.004) && (Math.Abs(maxLon - minLon) < 0.004))
            {
                mapController.AnimateTo(new GeoPoint((maxLat + minLat) / 2, (maxLon + minLon) / 2));
                mapController.SetZoom(17);
            }
            else
            {
                double fitFactor = 1.5;

                mapController.ZoomToSpan((int)(Math.Abs(maxLat - minLat) * fitFactor), (int)(Math.Abs(maxLon - minLon) * fitFactor));
                mapController.AnimateTo(new GeoPoint((maxLat + minLat) / 2, (maxLon + minLon) / 2));
            }

        }

    }
}
