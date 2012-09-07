using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.GoogleMaps;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Client.Activities.Location;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Models;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "Bookv2", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class Bookv2Activity : MapActivity, IAddress
    {

        public LocationService LocationService
        {
            get;
            private set;
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            LocationService = new LocationService();
            LocationService.Start();

            SetContentView(Resource.Layout.Bookv2);
            // Create your application here
            this.InitializeDropDownMenu();
        }

        /*protected override MapView Map
        {
            get { return FindViewById<MapView>(Resource.Id.mapPickup); }
        }

        protected override int TitleResourceId
        {
            get { return Resource.String.PickupMapTitle; }
        }

        protected override Address Location { get; set; }

        protected override bool NeedFindCurrentLocation
        {
            get
            {
                return false;
            }
        }

        protected override bool AddressCanBeEmpty
        {
            get { return true; }
        }

        protected override AutoCompleteTextView Address
        {
            get { return new AutoCompleteTextView(this); }
        }

        protected override Drawable MapPin
        {
            get { return Resources.GetDrawable(Resource.Drawable.pin_green); }
        }*/

        public void OnResumeEvent()
        {
            OnResume();
        }

        protected override bool IsRouteDisplayed
        {
            get { return true; }
        }

        protected void InitializeDropDownMenu()
        {
            //Initialize dropdown control

            // Address book intent
            var contactIntent = new Intent(Intent.ActionPick, ContactsContract.CommonDataKinds.StructuredPostal.ContentUri);
            // Favorite address intent
            var locationIntent = new Intent(this, typeof(LocationsActivity));
            locationIntent.PutExtra(NavigationStrings.ParentScreen.ToString(), (int)ParentScreens.BookScreen);
            //gps intent
            var gpsIntent = new Intent(LocationBroadcastReceiver.ACTION_RESP);
            //nearby places intent
            var placesIntent = new Intent(this, typeof(LocationsActivity));
            placesIntent.PutExtra(NavigationStrings.ParentScreen.ToString(), (int)ParentScreens.BookScreen);
            placesIntent.PutExtra(NavigationStrings.LocationType.ToString(), (int)LocationTypes.NearbyPlaces);

            IntentFilter filter = new IntentFilter(LocationBroadcastReceiver.ACTION_RESP);
            filter.AddCategory(Intent.CategoryDefault);
            //var receiver = new LocationBroadcastReceiver(this);
            //RegisterReceiver(receiver, filter);

            //DropDownMenu definition
            var iconActionControl = new IconActionControl(this, "images/arrow-right@2x.png", new List<IconAction>() { 
				new IconAction("images/location-icon@2x.png", gpsIntent, null), 
				new IconAction("images/favorite-icon@2x.png", locationIntent, (int)ActivityEnum.Pickup), 
				new IconAction("images/contacts@2x.png", contactIntent, 42),
				new IconAction("images/nearby-icon@2x.png", placesIntent, (int)ActivityEnum.NearbyPlaces )
			}, false);
            var dropDownControlLayout = FindViewById<LinearLayout>(Resource.Id.linear_iconaction);
            dropDownControlLayout.AddView(iconActionControl);
        }
    }
}