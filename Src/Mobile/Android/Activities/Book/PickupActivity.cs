using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.GoogleMaps;
using Android.Locations;
using TinyIoC;
using apcurium.Framework.Extensions;
using Android.Views.InputMethods;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Client.Models;
using WS = apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "Pickup", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class PickupActivity : AddressActivity, IAddress
    {

        const int TIME_DIALOG_ID = 0;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.SetContentView(Resource.Layout.Pickup);
            
            Time.Hint = GetString(Resource.String.PickupDateTextPlaceholder) + "  -  " + GetString(Resource.String.PickupTimeTextPlaceholder);
            Time.Enabled = false;

            Time.FocusChange += (e,s) => ResizeDownIconActionControl();

            //FindViewById<Button>(Resource.Id.pickupTimeButton).Click += new EventHandler(PickTime_Click);
            FindViewById<Button>(Resource.Id.pickupDateButton).Click += new EventHandler(PickDate_Click);

            FindViewById<EditText>(Resource.Id.pickupTimeText).EditorAction += new EventHandler<TextView.EditorActionEventArgs>(PickupTimeText_EditorAction);
            FindViewById<EditText>(Resource.Id.pickupTimeText).FocusChange += (e, s) => ResizeDownIconActionControl();

            FindViewById<EditText>(Resource.Id.aptNumberText).FocusChange += (e, s) => ResizeDownIconActionControl();
            FindViewById<EditText>(Resource.Id.ringCodeText).FocusChange += (e, s) => ResizeDownIconActionControl();

            this.InitializeDropDownMenu();
            
        }

        protected override int TitleResourceId
        {
            get { return Resource.String.PickupMapTitle; }
        }


        void PickupTimeText_EditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            if (e.ActionId == ImeAction.Done)
            {
                HideKeyboards();
            }
        }


        void PickTime_Click(object sender, EventArgs e)
        {
            ShowDialog(TIME_DIALOG_ID);
        }

        void PickDate_Click(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(DateTimePickerActivity));
            if (ParentActivity.BookingInfo.PickupDate.HasValue)
            {
                intent.PutExtra("SelectedDate", ParentActivity.BookingInfo.PickupDate.Value.Ticks);
            }

            Parent.StartActivityForResult(intent, (int)ActivityEnum.DateTimePicked);

        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == (int)ActivityEnum.DateTimePicked)
            {

                
                    var selectedDateTicks = data.GetLongExtra("ResultSelectedDate", 0);
                    if (selectedDateTicks > 0)
                    {
                        ParentActivity.BookingInfo.PickupDate = new DateTime(selectedDateTicks);
                    }
                    else
                    {
                        ParentActivity.BookingInfo.PickupDate = null;
                    }
                    RefreshDateTime();
            }
        }

        protected override AutoCompleteTextView Address
        {
            get { return FindViewById<AutoCompleteTextView>(Resource.Id.pickupAddressText); }
        }


        protected override MapView Map
        {
            get { return FindViewById<MapView>(Resource.Id.mapPickup);}
        }


        protected EditText Time
        {
            get { return FindViewById<EditText>(Resource.Id.pickupTimeText); }
        }

        protected override Android.Graphics.Drawables.Drawable MapPin
        {
            get { return Resources.GetDrawable(Resource.Drawable.pin_green); }
        }

        protected override Button SelectAddressButton
        {
            //get { return FindViewById<Button>(Resource.Id.pickupAddressButton); }
            get { return null; }
        }

        protected override bool ShowUserLocation
        {
            get { return true; }
        }



        protected override WS.Address Location
        {
            get { return ParentActivity.BookingInfo.PickupAddress; }
            set { ParentActivity.BookingInfo.PickupAddress = value; }
        }

        protected override bool NeedFindCurrentLocation
        {
            get
            {
                return true;
            }
        }

        public void OnResumeEvent()
        {
            OnResume();
        }

        protected override View[] GetHideableControls()
        {
            var list = new List<View>(base.GetHideableControls());
            list.Add(FindViewById<EditText>(Resource.Id.ringCodeText));
            list.Add(FindViewById<EditText>(Resource.Id.aptNumberText));
            return list.ToArray();
        }

        public override void SetLocationData(WS.Address location, bool changeZoom)
        {
            base.SetLocationData(location, changeZoom);
            RunOnUiThread(() =>
                              {
                                  if(location != null)
                                  {
                                      FindViewById<EditText>(Resource.Id.ringCodeText).Text = location.RingCode;
                                      FindViewById<EditText>(Resource.Id.aptNumberText).Text = location.Apartment;
                                  }
                              });
        }

        public void SetLocationDataAndValidate(WS.Address location, bool changeZoom)
        {
            Address.Text = location.FullAddress;
            this.ValidateAddress(true);
            base.SetLocationData(location, changeZoom);

            RunOnUiThread(() =>
            {
                if (location != null)
                {
                    FindViewById<EditText>(Resource.Id.ringCodeText).Text = location.RingCode;
                    FindViewById<EditText>(Resource.Id.aptNumberText).Text = location.Apartment;
                }
            });
        }

        public void RefreshDateTime()
        {
            if (ParentActivity.BookingInfo.PickupDate.HasValue)
            {
                string d = GetString(Resource.String.Date);
                string t = GetString(Resource.String.Time);
                Time.Text = d + " : " + ParentActivity.BookingInfo.PickupDate.Value.ToShortDateString() + @"  -  " + t + " : " +ParentActivity.BookingInfo.PickupDate.Value.ToShortTimeString();                                
            }
            else
            {
                Time.Text = "";
            }
            //Date.Text = ParentActivity.Model.Data.PickupDate == null ? "" : ParentActivity.Model.Data.PickupDate.Value.ToShortDateString();
        }

        protected override void OnResume()
        {
            base.OnResume();
            FindViewById<EditText>(Resource.Id.ringCodeText).Text = Location.RingCode;
            FindViewById<EditText>(Resource.Id.aptNumberText).Text = Location.Apartment;
            RefreshDateTime();


        }

    }
}
