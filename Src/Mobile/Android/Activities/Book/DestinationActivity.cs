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
using Android.GoogleMaps;
using Android.Locations;
using Microsoft.Practices.ServiceLocation;
using apcurium.Framework.Extensions;
using Android.Views.InputMethods;
using apcurium.MK.Booking.Mobile.Data;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "Destination", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class DestinationActivity : AddressActivity, IAddress
    {


        private TextView RideDistance
        {
            get { return FindViewById<TextView>(Resource.Id.rideDistance); }
        }

        private TextView RideCost
        {
            get { return FindViewById<TextView>(Resource.Id.rideCost); }
        }

        protected override int TitleResourceId
        {
            get { return Resource.String.DestinationMapTitle; }
        }

        protected override AutoCompleteTextView Address
        {
            get { return FindViewById<AutoCompleteTextView>(Resource.Id.destAddressText); }
        }

        protected override MapView Map
        {
            get { return FindViewById<MapView>(Resource.Id.mapDestination); }
        }

        protected override Button SelectAddressButton
        {
            get { return FindViewById<Button>(Resource.Id.destAddressButton); }
        }
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.SetContentView(Resource.Layout.Destination);
            
            RefreshEstimates();
        }



        private void RefreshEstimates()
        {
            RunOnUiThread(() =>
            {
                RideDistance.Text = String.Format(Resources.GetString(Resource.String.EstimateDistance), ParentActivity.Model.Distance);
                RideCost.SetTextColor(Resources.GetColor(Resource.Color.black));
                double price;
                if (double.TryParse(ParentActivity.Model.Price, out price))
                {
                    if (price > 100)
                    {
                        RideCost.SetTextColor(Resources.GetColor(Resource.Color.red));
                        RideCost.Text = Resources.GetString(Resource.String.EstimatePriceOver100);
                    }
                    else
                    {
                        RideCost.Text = String.Format(Resources.GetString(Resource.String.EstimatePrice), price);
                    }
                }
                else
                {
                    RideCost.Text = String.Format(Resources.GetString(Resource.String.EstimatePrice), ParentActivity.Model.Price);
                }
            });
        }


        protected override LocationData Location
        {
            get { return ParentActivity.Model.Data.DestinationLocation; }
            set { ParentActivity.Model.Data.DestinationLocation = value; }
        }

        protected override bool NeedFindCurrentLocation
        {
            get { return false; }
        }

        protected override Android.Graphics.Drawables.Drawable MapPin
        {
            get { return Resources.GetDrawable(Resource.Drawable.pin_red); }
        }

        public void OnResumeEvent()
        {
            OnResume();
        }

        protected override void OnResume()
        {
            base.OnResume();
            RefreshEstimates();
        }

        public override void SetLocationData(LocationData location, bool changeZoom)
        {            
            base.SetLocationData(location, changeZoom);
            RefreshEstimates();
        }
    }
}
