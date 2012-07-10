using System;
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

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "Book Status", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation=Android.Content.PM.ScreenOrientation.Portrait)]
    public class BookingStatusActivity : MapActivity
    {
        private bool _closeScreenWhenCompleted;
        private int _lastOrder;
        private Timer _timer;
        private BookingInfoData _bookingInfo;
        public BookingInfoData BookingInfo
        {
            get { return _bookingInfo; }
            private set { _bookingInfo = value; }
        }


        protected override bool IsRouteDisplayed
        {
            get { return false; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.BookingStatus);

            var serialized = Intent.GetStringExtra("BookingData");
            var data = SerializerHelper.DeserializeObject<BookingInfoData>(serialized);
            _bookingInfo = data;



            FindViewById<TextView>(Resource.Id.statusInfoText).Text = string.Format(GetString(Resource.String.StatusDescription), _bookingInfo.Id);

            SetStatusText(GetString(Resource.String.LoadingMessage));

            FindViewById<Button>(Resource.Id.CallBookCancelBtn).Click += new EventHandler(BookingStatusActivity_Click);


            var map = FindViewById<MapView>(Resource.Id.mapStatus);

            AddMapPin(map, _bookingInfo.PickupLocation, Resource.Drawable.pin_green, Resource.String.PickupMapTitle);
            AddMapPin(map, _bookingInfo.DestinationLocation, Resource.Drawable.pin_red, Resource.String.DestinationMapTitle);

            _timer = new Timer(o => RefreshStatus(), null, 0, 6000);
        }

        private void AddMapPin(MapView map, Address loc, int graphic, int titleId)
        {
            if (loc.HasValidCoordinate())
            {
                var point = new GeoPoint(CoordinatesConverter.ConvertToE6(loc.Latitude), CoordinatesConverter.ConvertToE6(loc.Longitude));
                var pushpin = Resources.GetDrawable(graphic);
                var title = GetString(titleId);
                var pushpinOverlay = new PushPinOverlay(map, pushpin, title, point);
                map.Overlays.Add(pushpinOverlay);
            }
        }


        private void SetStatusText(string status)
        {
            FindViewById<TextView>(Resource.Id.CallStatusText).Text = string.Format(GetString(Resource.String.StatusStatusLabel), status);
        }

        void BookingStatusActivity_Click(object sender, EventArgs e)
        {
            RegisterForContextMenu((View)sender);
            OpenContextMenu((View)sender);
            UnregisterForContextMenu((View)sender);

            //this.OpenContextMenu(FindViewById<Button>(Resource.Id.CallBookCancelBtn));
        }

        public override void OnCreateContextMenu(Android.Views.IContextMenu menu, Android.Views.View v, Android.Views.IContextMenuContextMenuInfo menuInfo)
        {
            base.OnCreateContextMenu(menu, v, menuInfo);

            //TODO:Fix this

            //var callCompany = new Java.Lang.String(string.Format(GetString(Resource.String.CallCompanyButton), BookingInfo.Settings.CompanyName));
            //menu.SetHeaderTitle(Resource.String.StatusActionButton);
            //menu.Add(0, 1, 0, callCompany);
            //menu.Add(0, 2, 1, Resource.String.StatusActionBookButton);
            //menu.Add(0, 3, 2, Resource.String.StatusActionCancelButton);
            //menu.Add(0, 4, 3, Resource.String.Close);
            

        }


        public override bool OnContextItemSelected(IMenuItem item)
        {
            if (item.ItemId == 1)
            {
                CallCompany();
            }
            else if (item.ItemId == 2)
            {
                CloseActivity();
            }
            else if (item.ItemId == 3)
            {
                CancelOrder();
            }


            return base.OnContextItemSelected(item);

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
            callIntent.SetData(Android.Net.Uri.Parse("tel:" + AppSettings.PhoneNumber(BookingInfo.Settings.ProviderId)));
            StartActivity(callIntent);
        }

        private void CancelOrder()
        {
            var newBooking = new Confirmation();
            newBooking.Action(this, Resource.String.StatusConfirmCancelRide, () =>
            {
                ThreadHelper.ExecuteInThread(this, () =>
                {
                    var isSuccess = TinyIoCContainer.Current.Resolve<IBookingService>().CancelOrder(AppContext.Current.LoggedUser, BookingInfo.Id);

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
            //		
        }

        private void RefreshStatus()
        {
            ThreadHelper.ExecuteInThread(this, () =>
            {
                try
                {
                    var isCompleted = TinyIoCContainer.Current.Resolve<IBookingService>().IsCompleted(AppContext.Current.LoggedUser, BookingInfo.Id);
                    if (isCompleted)
                    {
                        AppContext.Current.LastOrder = null;
                        CloseActivity();
                        return;
                    }


                    var status = TinyIoCContainer.Current.Resolve<IBookingService>().GetOrderStatus(AppContext.Current.LoggedUser, BookingInfo.Id);
                    _lastOrder = BookingInfo.Id;

                    if (status != null)
                    {
                        BookingInfo.Status = status.Status;

                        RunOnUiThread(() => SetStatusText(status.Status));

                        if ((status.Latitude != 0) && (status.Longitude != 0))
                        {
                            RunOnUiThread(() =>
                                {
                                    var map = FindViewById<MapView>(Resource.Id.mapStatus);
                                    var point = new GeoPoint(CoordinatesConverter.ConvertToE6(status.Latitude), CoordinatesConverter.ConvertToE6(status.Longitude));
                                    var pushpin = Resources.GetDrawable(Resource.Drawable.pin_yellow);
                                    var title = GetString(Resource.String.TaxiMapTitle);
                                    var pushpinOverlay = new PushPinOverlay(map, pushpin, title, point);

                                    map.Overlays.Clear();
                                    map.Invalidate();

                                    AddMapPin(map, _bookingInfo.PickupLocation, Resource.Drawable.pin_green, Resource.String.PickupMapTitle);
                                    AddMapPin(map, _bookingInfo.DestinationLocation, Resource.Drawable.pin_red, Resource.String.DestinationMapTitle);
                                    map.Overlays.Add(pushpinOverlay);
                                    map.Invalidate();

                                    map.Controller.AnimateTo(point);


                                });



                        }
                    }

                }
                catch (Exception ex)
                {
                    TinyIoCContainer.Current.Resolve<ILogger>().LogError(ex);
                }
            }, false);
        }
    }


}
