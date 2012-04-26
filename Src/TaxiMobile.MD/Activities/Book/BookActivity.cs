using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Microsoft.Practices.ServiceLocation;
using TaxiMobile.Helpers;
using TaxiMobile.Models;
using TaxiMobileApp;
using apcurium.Framework.Extensions;

namespace TaxiMobile.Activities.Book
{
    [Activity(Label = "Book", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = ScreenOrientation.Portrait)]
    public class BookActivity : TabActivity
    {
        private enum Tab
        {
            Pickup,
            Destination
        }



        public LocationService LocationService
        {
            get;
            private set;
        }

        private BookingModel _model;

        public BookingModel Model
        {
            get { return _model; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            LocationService = new LocationService();
            LocationService.Start();

            SetContentView(Resource.Layout.Book);

            ResetBookingInfo();

            var pickupButton = FindViewById<Button>(Resource.Id.PickupBtn);
            pickupButton.Click += new EventHandler(pickupButton_Click);

            var destinationButton = FindViewById<Button>(Resource.Id.DestinationBtn);
            destinationButton.Click += new EventHandler(destinationButton_Click);

            AddTab<PickupActivity>(Tab.Pickup.ToString(), Resource.String.PickupMapTitle, Resource.Drawable.book);
            AddTab<DestinationActivity>(Tab.Destination.ToString(), Resource.String.DestinationMapTitle, Resource.Drawable.book);

            TogglePickupDestination(true);

            TabWidget.Visibility = ViewStates.Gone;
            Parent.FindViewById<Button>(Resource.Id.BookItBtn).Click += new EventHandler(BookItBtn_Click);


        }

        protected override void OnResume()
        {
            base.OnResume();
            LocationService.Start();
            //LocalActivityManager.GetActivity()
            var pickup = (AddressActivity)LocalActivityManager.GetActivity(Tab.Pickup.ToSafeString());
            var dest = (AddressActivity)LocalActivityManager.GetActivity(Tab.Destination.ToSafeString());

            pickup.Maybe(() => pickup.ParentResume());
            dest.Maybe(() => pickup.ParentResume());


        }

        void BookItBtn_Click(object sender, EventArgs e)
        {
            UpdateModel();

            if (_model.Origin.IsNullOrEmpty())
            {
                this.ShowAlert(Resource.String.InvalidBookinInfoTitle, Resource.String.InvalidBookinInfo);
            }
            else
            {
                Intent i = new Intent(this, typeof(BookDetailActivity));
                var serializedModel = _model.Serialize();
                i.PutExtra("BookingModel", serializedModel);
                StartActivityForResult(i, (int)ActivityEnum.BookConfirmation);
            }
        }

        

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == (int)ActivityEnum.BookConfirmation)
            {
                if (data != null)
                {

                    ThreadHelper.ExecuteInThread(this, () =>
                    {
                        var xmlBInfo = data.GetStringExtra("ConfirmedBookingInfo");
                        var bookingInfo = SerializerHelper.DeserializeObject<BookingInfoData>(xmlBInfo);

                        var service = ServiceLocator.Current.GetInstance<IBookingService>();
                        string error;

                        var id = service.CreateOrder(AppContext.Current.LoggedUser, bookingInfo, out error);
                        if (id > 0)
                        {
                            AppContext.Current.LastOrder = id;
                            bookingInfo.Id = id;
                            bookingInfo.RequestedDateTime = DateTime.Now;
                            AppContext.Current.LoggedUser.AddBooking(bookingInfo);
                            AppContext.Current.UpdateLoggedInUser(AppContext.Current.LoggedUser, false);
                            ShowStatusActivity(bookingInfo);
                            ResetBookingInfo();
                        }
                        else
                        {
                            RunOnUiThread(() =>
                                {
                                    string err = GetString(Resource.String.ErrorCreatingOrderMessage);
                                    err += error.HasValue() ? " (" + error + ")" : "";
                                    this.ShowAlert(GetString(Resource.String.ErrorCreatingOrderTitle), err);
                                });
                        }
                    }, true);
                }
            }
            else if (requestCode == (int)ActivityEnum.DateTimePicked)
            {

                if ((data != null) && (LocalActivityManager.GetActivity(this.TabHost.CurrentTabTag) is PickupActivity))
                {
                    var selectedDateTicks = data.GetLongExtra("ResultSelectedDate", 0);
                    if (selectedDateTicks > 0)
                    {
                        Model.Data.PickupDate = new DateTime(selectedDateTicks);
                    }
                    else
                    {
                        Model.Data.PickupDate = null;
                    }
                    var activity = (PickupActivity)LocalActivityManager.GetActivity(this.TabHost.CurrentTabTag);
                    activity.RefreshDateTime( );
                }
            }
            else if ((data != null) && (data.Extras != null))
            {
                var address = data.GetStringExtra("SelectedAddress");

                if (address.HasValue())
                {
                    var adrs = SerializerHelper.DeserializeObject<LocationData>(address);
                    var activity = (AddressActivity)LocalActivityManager.GetActivity(this.TabHost.CurrentTabTag);

                    
                    //var adrs = GetLocation(LocationTypes.Favorite, address);
                    //if (adrs == null)
                    //{
                    //    adrs = GetLocation(LocationTypes.History, address);
                    //}

                    if (adrs != null)
                    {
                        activity.SetLocationData(adrs, true);
                    }
                }
            }
        }

        private LocationData GetLocation(LocationTypes type, int id)
        {

            if (type == LocationTypes.Favorite)
            {
                return AppContext.Current.LoggedUser.FavoriteLocations.FirstOrDefault(a => a.Id == id);
            }
            else
            {
                var history = AppContext.Current.LoggedUser.BookingHistory.Where(b => !b.Hide && b.PickupLocation.Name.IsNullOrEmpty())
               .OrderByDescending(b => b.RequestedDateTime)
               .Select(b => b.PickupLocation).ToArray();
                var locationsData = history.Where(h => h.Address.HasValue()).GroupBy(l => l.Address + "_" + l.Apartment.ToSafeString() + "_" + l.RingCode.ToSafeString()).Select(g => g.ElementAt(0));
                return locationsData.FirstOrDefault(a => a.Id == id);

            }
        }
        private void ShowStatusActivity(BookingInfoData data)
        {
            RunOnUiThread(() =>
            {
                Intent i = new Intent(this, typeof(BookingStatusActivity));
                var serialized = data.Serialize();
                i.PutExtra("BookingData", serialized);
                StartActivity(i);
            });
        }

        void destinationButton_Click(object sender, EventArgs e)
        {
            TogglePickupDestination(false);
        }

        void pickupButton_Click(object sender, EventArgs e)
        {
            TogglePickupDestination(true);
        }

        private void TogglePickupDestination(bool selectPickup)
        {
            UpdateModel();


            var selectedViewIsPickup = TabHost.CurrentTabTag == Tab.Pickup.ToString();

            if (selectedViewIsPickup && !selectPickup && !Model.Origin.HasValue())
            {
                AlertDialogHelper.ShowAlert(this, "", Resources.GetString(Resource.String.InvalidPickupAddress));
            }
            else
            {
                var pickupButton = FindViewById<Button>(Resource.Id.PickupBtn);
                var destinationButton = FindViewById<Button>(Resource.Id.DestinationBtn);
                pickupButton.Selected = selectPickup;

                destinationButton.Selected = !selectPickup;

                if ((selectPickup) && (!selectedViewIsPickup))
                {
                    TabHost.SetCurrentTabByTag(Tab.Pickup.ToString());

                }
                else if ((!selectPickup) && (selectedViewIsPickup))
                {
                    TabHost.SetCurrentTabByTag(Tab.Destination.ToString());
                }
            }

        }

        private void AddTab<TActivity>(string tag, int titleId, int drawableId)
        {
            var intent = new Intent().SetClass(this, typeof(TActivity));
            var spec = TabHost.NewTabSpec(tag).SetIndicator(GetString(titleId), Resources.GetDrawable(drawableId)).SetContent(intent);

            TabHost.AddTab(spec);

        }

        private void UpdateModel()
        {
            if (TabHost.CurrentView != null && TabHost.CurrentTabTag == Tab.Pickup.ToString())
            {
                _model.Origin = TabHost.CurrentView.FindViewById<AutoCompleteTextView>(Resource.Id.pickupAddressText).Text;
                _model.RingCode = TabHost.CurrentView.FindViewById<EditText>(Resource.Id.ringCodeText).Text;
                _model.Appartment = TabHost.CurrentView.FindViewById<EditText>(Resource.Id.aptNumberText).Text;
                //s_model.Date = TabHost.CurrentView.FindViewById<EditText>(Resource.Id.pickupDateText).Text + " " + TabHost.CurrentView.FindViewById<EditText>(Resource.Id.pickupTimeText).Text;
            }
            else if (TabHost.CurrentView != null && TabHost.CurrentTabTag == Tab.Destination.ToString())
            {
                _model.Destination = this.TabHost.CurrentView.FindViewById<EditText>(Resource.Id.destAddressText).Text;
            }
        }




        public void Reset()
        {
            _model = new BookingModel();
            var pickupActivity = (PickupActivity)LocalActivityManager.GetActivity("Pickup");
            if (pickupActivity != null)
            {
                pickupActivity.FindViewById<AutoCompleteTextView>(Resource.Id.pickupAddressText).Text = "";
                pickupActivity.FindViewById<EditText>(Resource.Id.ringCodeText).Text = "";
                pickupActivity.FindViewById<EditText>(Resource.Id.aptNumberText).Text = "";
                pickupActivity.RefreshDateTime();
                pickupActivity.OnAddressChanged("", true);
            }
            var destActivity = (DestinationActivity)LocalActivityManager.GetActivity("Destination");
            if (destActivity != null)
            {
                destActivity.FindViewById<EditText>(Resource.Id.destAddressText).Text = "";
            }
            TogglePickupDestination(true);
        }


        protected override void OnPause()
        {
            base.OnPause();
            LocationService.Stop();
        }

        public void ResetBookingInfo()
        {
            RunOnUiThread(() =>
                              {
                                  _model = new BookingModel();
                                  Model.Data = new BookingInfoData();
                                  var currentActivity = (IAddress)LocalActivityManager.GetActivity(this.TabHost.CurrentTabTag);
                                  currentActivity.Maybe(() => currentActivity.OnResumeEvent());
                              });
        }


        public void RebookTrip(int tripId)
        {

            var tripData = AppContext.Current.LoggedUser.BookingHistory.SingleOrDefault(o => o.Id == tripId);
            if (tripData != null)
            {
                _model.Data = tripData.Copy();
                _model.Data.Id = 0;
                _model.Data.PickupDate = null;
                _model.Data.Status = null;
                _model.Data.RequestedDateTime = null;


                var pickupActivity = (PickupActivity)LocalActivityManager.GetActivity("Pickup");
                if (pickupActivity != null)
                {
                    pickupActivity.SetLocationData(_model.Data.PickupLocation, true);
                    pickupActivity.RefreshDateTime();                    
                }

                var destActivity = (DestinationActivity)LocalActivityManager.GetActivity("Destination");
                if (destActivity != null)
                {
                    destActivity.SetLocationData(_model.Data.DestinationLocation, true);
                }
                TogglePickupDestination(true);

            }
        }

        internal void StartStatusActivity(int id)
        {
            var booking = AppContext.Current.LoggedUser.BookingHistory.FirstOrDefault(b => b.Id == id);
            if (booking != null)
            {
                ShowStatusActivity(booking);
            }
        }
    }
}
