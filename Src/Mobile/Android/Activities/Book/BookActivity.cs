using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Views.Animations;
using Android.GoogleMaps;
using Android.Locations;
using apcurium.Framework.Extensions;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Client.Models;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Extensions;

using WS = apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "Book", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
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

        private CreateOrder _bookingInfo;

        public CreateOrder BookingInfo
        {
            get { return _bookingInfo; }
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
            ThreadHelper.ExecuteInThread(this, () =>
                {

                    var pickup = (AddressActivity)LocalActivityManager.GetActivity(Tab.Pickup.ToSafeString());
                    pickup.Maybe(() => pickup.ValidateAddress(false));
                    var dest = (AddressActivity)LocalActivityManager.GetActivity(Tab.Destination.ToSafeString());
                    dest.Maybe(() => dest.ValidateAddress(false));

                    UpdateModel();
                    
                    if ((_bookingInfo.PickupAddress.FullAddress.IsNullOrEmpty()) || (!_bookingInfo.PickupAddress.HasValidCoordinate()))
                    {
                        RunOnUiThread(() => this.ShowAlert(Resource.String.InvalidBookinInfoTitle, Resource.String.InvalidBookinInfo));
                    }
                    else
                    {
                        RunOnUiThread(() =>
                            {
                                Intent i = new Intent(this, typeof(BookDetailActivity));
                                var serializedInfo = _bookingInfo.Serialize();
                                i.PutExtra("BookingInfo", serializedInfo);
                                StartActivityForResult(i, (int)ActivityEnum.BookConfirmation);
                            });
                    }
                }, true);
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
                        var bookingInfo = SerializerHelper.DeserializeObject<CreateOrder>(xmlBInfo);

                        var service = TinyIoCContainer.Current.Resolve<IBookingService>();
                        bookingInfo.Id = Guid.NewGuid();

                        try
                        {

                            var orderInfo = service.CreateOrder(bookingInfo);
                            AppContext.Current.LastOrder = bookingInfo.Id;

                            var order = new Order { CreatedDate = DateTime.Now, DropOffAddress = bookingInfo.DropOffAddress, IBSOrderId = orderInfo.IBSOrderId, Id = bookingInfo.Id, PickupAddress = bookingInfo.PickupAddress, Note = bookingInfo.Note, PickupDate = bookingInfo.PickupDate.HasValue ? bookingInfo.PickupDate.Value: DateTime.Now, Settings = bookingInfo.Settings };  
                            
                            ShowStatusActivity( order , orderInfo);
                            
                            ResetBookingInfo();

                        }
                        catch (Exception ex)
                        {
                            RunOnUiThread(() =>
                            {
                                string error = ex.Message;
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
                        BookingInfo.PickupDate = new DateTime(selectedDateTicks);
                    }
                    else
                    {
                        BookingInfo.PickupDate = null;
                    }
                    var activity = (PickupActivity)LocalActivityManager.GetActivity(this.TabHost.CurrentTabTag);
                    activity.RefreshDateTime();
                }
            }
            else if (requestCode == 101)
            {
                Reset();
            }
            else if ((data != null) && (data.Extras != null))
            {
                var address = data.GetStringExtra("SelectedAddress");

                if (address.HasValue())
                {
                    var adrs = SerializerHelper.DeserializeObject<WS.Address>(address);
                    var activity = (AddressActivity)LocalActivityManager.GetActivity(this.TabHost.CurrentTabTag);
                    if (adrs != null)
                    {
                        activity.SetLocationData(adrs, true);
                    }
                }
            }
            else if (requestCode == 42)
            {
                string id = data.Data.LastPathSegment;
                var address = "";
                var contacts = ManagedQuery(ContactsContract.CommonDataKinds.StructuredPostal.ContentUri, null, "_id = ?", new string[] { id }, null);
                if (contacts.MoveToFirst())
                {
                    address = contacts
                        .GetString(contacts
                                       .GetColumnIndex(
                                           ContactsContract.CommonDataKinds.StructuredPostal.FormattedAddress));
                }
                //this.TabHost.SetCurrentTabByTag(Tab.Destination.ToString());
                TogglePickupDestination(false);
                var activity = (DestinationActivity)LocalActivityManager.GetActivity(this.TabHost.CurrentTabTag);

                if (!string.IsNullOrEmpty(address))
                {
                    activity.SetLocationDataAndValidate(new WS.Address() { FullAddress = address }, true);
                   
                }
            }
        }



        private WS.Address GetLocation(LocationTypes type, int id)
        {

            return null;
            //if (type == LocationTypes.Favorite)
            //{
            //    return AppContext.Current.LoggedUser.FavoriteLocations.FirstOrDefault(a => a.Id == id);
            //}
            //else
            //{
            //    return new LocationData();
            //    //TODO:
            //   // var history = AppContext.Current.LoggedUser.BookingHistory.Where(b => !b.Hide && b.PickupLocation.f .IsNullOrEmpty())
            //   //.OrderByDescending(b => b.RequestedDateTime)
            //   //.Select(b => b.PickupLocation).ToArray();
            //   // var locationsData = history.Where(h => h.Address.HasValue()).GroupBy(l => l.Address + "_" + l.Apartment.ToSafeString() + "_" + l.RingCode.ToSafeString()).Select(g => g.ElementAt(0));
            //   // return locationsData.FirstOrDefault(a => a.Id == id);

            //}
        }


        private void ShowStatusActivity(Order data, OrderStatusDetail orderInfo)
        {
            RunOnUiThread(() =>
            {
                Intent i = new Intent(this, typeof(BookingStatusActivity));
                var serialized = data.Serialize();
                i.PutExtra("Order", serialized);

                serialized = orderInfo.Serialize();
                i.PutExtra("OrderStatusDetail", serialized);


                StartActivityForResult(i, 101);
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

            if (selectedViewIsPickup && !selectPickup && !BookingInfo.PickupAddress.FullAddress.HasValue())
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
                _bookingInfo.PickupAddress.FullAddress = TabHost.CurrentView.FindViewById<AutoCompleteTextView>(Resource.Id.pickupAddressText).Text;
                _bookingInfo.PickupAddress.RingCode = TabHost.CurrentView.FindViewById<EditText>(Resource.Id.ringCodeText).Text;
                _bookingInfo.PickupAddress.Apartment = TabHost.CurrentView.FindViewById<EditText>(Resource.Id.aptNumberText).Text;
                //s_model.Date = TabHost.CurrentView.FindViewById<EditText>(Resource.Id.pickupDateText).Text + " " + TabHost.CurrentView.FindViewById<EditText>(Resource.Id.pickupTimeText).Text;
            }
            else if (TabHost.CurrentView != null && TabHost.CurrentTabTag == Tab.Destination.ToString())
            {
                _bookingInfo.DropOffAddress.FullAddress = this.TabHost.CurrentView.FindViewById<EditText>(Resource.Id.destAddressText).Text;
            }
        }




        public void Reset()
        {
            _bookingInfo = new CreateOrder();
            var pickupActivity = (PickupActivity)LocalActivityManager.GetActivity("Pickup");
            if (pickupActivity != null)
            {
                pickupActivity.FindViewById<AutoCompleteTextView>(Resource.Id.pickupAddressText).Text = "";
                pickupActivity.FindViewById<EditText>(Resource.Id.ringCodeText).Text = "";
                pickupActivity.FindViewById<EditText>(Resource.Id.aptNumberText).Text = "";
                pickupActivity.RefreshDateTime();
                RunOnUiThread(() => pickupActivity.OnAddressChanged("", true));
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
                                  _bookingInfo = new CreateOrder();
                                  var currentActivity = (IAddress)LocalActivityManager.GetActivity(this.TabHost.CurrentTabTag);
                                  currentActivity.Maybe(() => currentActivity.OnResumeEvent());
                              });
        }


        public void RebookTrip(Guid tripId)
        {

            //TODO:Fix this
            //var tripData = AppContext.Current.LoggedUser.BookingHistory.SingleOrDefault(o => o.Id == tripId);
            //if (tripData != null)
            //{
            //    _bookingInfo = tripData.Copy();
            //    _bookingInfo.Id = 0;
            //    _bookingInfo.PickupDate = null;
            //    _bookingInfo.Status = null;
            //    _bookingInfo.RequestedDateTime = null;


            //    var pickupActivity = (PickupActivity)LocalActivityManager.GetActivity("Pickup");
            //    if (pickupActivity != null)
            //    {
            //        pickupActivity.SetLocationData(_bookingInfo.PickupLocation, true);
            //        pickupActivity.RefreshDateTime();                    
            //    }

            //    var destActivity = (DestinationActivity)LocalActivityManager.GetActivity("Destination");
            //    if (destActivity != null)
            //    {
            //        destActivity.SetLocationData(_bookingInfo.DestinationLocation, true);
            //    }
            //    TogglePickupDestination(true);

            //}
        }

        internal void StartStatusActivity(Guid id)
        {
            //TODO:Fix this
            //var booking = AppContext.Current.LoggedUser.BookingHistory.FirstOrDefault(b => b.Id == id);
            //if (booking != null)
            //{

            ThreadHelper.ExecuteInThread(this, () =>
            {
                var orderStatus = TinyIoCContainer.Current.Resolve<IBookingService>().GetOrderStatus(id);
                var order = TinyIoCContainer.Current.Resolve<IAccountService>().GetHistoryOrder(id);
                ShowStatusActivity(order, orderStatus);

            }, false);
            //}

        }
    }
}
