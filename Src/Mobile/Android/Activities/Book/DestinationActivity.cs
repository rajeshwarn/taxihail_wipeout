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
using Android.GoogleMaps;
using Android.Locations;
using TinyIoC;
using apcurium.Framework.Extensions;
using Android.Views.InputMethods;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Models;
using apcurium.MK.Booking.Mobile.Data;
using WS = apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Client.Helpers;
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
            get { /*return FindViewById<Button>(Resource.Id.destAddressButton);*/
                return null;
            }
        }
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.SetContentView(Resource.Layout.Destination);
            this.InitializeDropDownMenu();
            RefreshEstimates();
        }

        private void RefreshEstimates()
        {
            RunOnUiThread(() =>
            {
                var directionInfo = new DirectionInfo();

                if (ParentActivity.BookingInfo.PickupAddress.HasValidCoordinate() && ParentActivity.BookingInfo.DropOffAddress.HasValidCoordinate())
                {
                    directionInfo = TinyIoCContainer.Current.Resolve<IGeolocService>().GetDirectionInfo(ParentActivity.BookingInfo.PickupAddress.Latitude, ParentActivity.BookingInfo.PickupAddress.Longitude, ParentActivity.BookingInfo.DropOffAddress.Latitude, ParentActivity.BookingInfo.DropOffAddress.Longitude);
                }

                if (directionInfo.Distance.HasValue)
                {
                    RideDistance.Text = String.Format(Resources.GetString(Resource.String.EstimateDistance), directionInfo.FormattedDistance);                 
                }
                else
                {
                    RideDistance.Text = String.Format(Resources.GetString(Resource.String.EstimateDistance), Resources.GetString(Resource.String.NotAvailable));                    
                }
                
                
                
                if ( directionInfo.Price.HasValue )
                {
                    if (directionInfo.Price.Value > 100)
                    {
                        RideCost.SetTextColor(Resources.GetColor(Resource.Color.red));
                        RideCost.Text = Resources.GetString(Resource.String.EstimatePriceOver100);
                    }
                    else
                    {
                        RideCost.SetTextColor(Resources.GetColor(Resource.Color.black));
                        RideCost.Text = String.Format(Resources.GetString(Resource.String.EstimatePrice), directionInfo.FormattedPrice);
                    }
                }
                else
                {
                    RideCost.SetTextColor(Resources.GetColor(Resource.Color.black));
                    RideCost.Text = String.Format(Resources.GetString(Resource.String.EstimatePrice), Resources.GetString(Resource.String.NotAvailable));
                }
            });
        }


        protected override WS.Address Location
        {
            get { return ParentActivity.BookingInfo.DropOffAddress; }
            set { ParentActivity.BookingInfo.DropOffAddress = value; }
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

        public void SetLocationDataAndValidate(WS.Address location, bool changeZoom)
        {
            Address.Text = location.FullAddress;
            this.ValidateAddress(true);
            base.SetLocationData(location, changeZoom);

            ThreadHelper.ExecuteInThread(this, RefreshEstimates, false);
        }

        public override void SetLocationData(WS.Address location, bool changeZoom)
        {            
            base.SetLocationData(location, changeZoom);

            ThreadHelper.ExecuteInThread(this, RefreshEstimates, false);
        }
    }
}
