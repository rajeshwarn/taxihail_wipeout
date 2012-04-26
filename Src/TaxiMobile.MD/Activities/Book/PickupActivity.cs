using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.GoogleMaps;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using TaxiMobile.Models;
using TaxiMobileApp;

namespace TaxiMobile.Activities.Book
{
    [Activity(Label = "Pickup", ScreenOrientation = ScreenOrientation.Portrait)]
    public class PickupActivity : AddressActivity, IAddress
    {

        const int TIME_DIALOG_ID = 0;
        const int DATE_DIALOG_ID = 1;
        const double DELTA_DISTANCE = 0.001;


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.SetContentView(Resource.Layout.Pickup);
            
            Time.Hint = GetString(Resource.String.PickupDateTextPlaceholder) + "  -  " + GetString(Resource.String.PickupTimeTextPlaceholder);
            Time.Enabled = false;
            
            //FindViewById<Button>(Resource.Id.pickupTimeButton).Click += new EventHandler(PickTime_Click);
            FindViewById<Button>(Resource.Id.pickupDateButton).Click += new EventHandler(PickDate_Click);


            FindViewById<EditText>(Resource.Id.pickupTimeText).EditorAction += new EventHandler<TextView.EditorActionEventArgs>(PickupTimeText_EditorAction);

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
            //ShowDialog(DATE_DIALOG_ID);
            var intent = new Intent(this, typeof(DateTimePickerActivity));
            if (ParentActivity.Model.Data.PickupDate.HasValue)
            {
                intent.PutExtra("SelectedDate", ParentActivity.Model.Data.PickupDate.Value.Ticks);
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
                        ParentActivity.Model.Data.PickupDate = new DateTime(selectedDateTicks);
                    }
                    else
                    {
                        ParentActivity.Model.Data.PickupDate = null;
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
            get { return FindViewById<MapView>(Resource.Id.mapPickup); }
        }


        protected EditText Time
        {
            get { return FindViewById<EditText>(Resource.Id.pickupTimeText); }
        }

        protected override Drawable MapPin
        {
            get { return Resources.GetDrawable(Resource.Drawable.pin_green); }
        }

        protected override Button SelectAddressButton
        {
            get { return FindViewById<Button>(Resource.Id.pickupAddressButton); }
        }

        protected override bool ShowUserLocation
        {
            get { return true; }
        }



        protected override LocationData Location
        {
            get { return ParentActivity.Model.Data.PickupLocation; }
            set { ParentActivity.Model.Data.PickupLocation = value; }
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

        public override void SetLocationData(LocationData location, bool changeZoom)
        {
            base.SetLocationData(location, changeZoom);
            RunOnUiThread(() =>
                              {
                                  FindViewById<EditText>(Resource.Id.ringCodeText).Text = location.RingCode;
                                  FindViewById<EditText>(Resource.Id.aptNumberText).Text = location.Apartment;
                              });
        }

        public void RefreshDateTime()
        {
            if (ParentActivity.Model.Data.PickupDate.HasValue)
            {
                string d = GetString(Resource.String.Date);
                string t = GetString(Resource.String.Time);
                Time.Text = d + " : " + ParentActivity.Model.Data.PickupDate.Value.ToShortDateString() + @"  -  " + t + " : " +ParentActivity.Model.Data.PickupDate.Value.ToShortTimeString();                                
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
