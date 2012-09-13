using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.GoogleMaps;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Client.Activities.Location;
using apcurium.MK.Booking.Mobile.Client.Adapters;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Models;
using Cirrious.MvvmCross.Binding.Android.Views;
 
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.Android.ExtensionMethods;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "Bookv2", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class Bookv2Activity : MvxBindingMapActivityView<BookViewModel>, IAddress
    {        
        public LocationService LocationService
        {
            get;
            private set;
        }


        //protected override void OnCreate(Bundle bundle)
        //{
            
        //    base.OnCreate(bundle);
        //    LocationService = new LocationService();
        //    LocationService.Start();
        //    SetContentView(Resource.Layout.Bookv2);

        //    // make bottom layout transparent
        //    var bottomLayout = FindViewById<FrameLayout>(Resource.Id.bottomLayout);
        //    AlphaAnimation alpha = new AlphaAnimation(0.1F,0.1F);
        //    alpha.Duration = 0;
        //    alpha.FillAfter = true;
        //    bottomLayout.StartAnimation(alpha);

        //    //set text on button destinationAddressButton
        //    var pickupAddressButton = FindViewById<Button>(Resource.Id.pickupAddressButton);
        //    pickupAddressButton.SetText(Html.FromHtml("<small>Pickup</small> <br/> <b>3939 rue Rivard, Montreal QC</br>").ToString(), TextView.BufferType.Editable);

        //    var destinationAddressButton = FindViewById<Button>(Resource.Id.destinationAddressButton);
        //    destinationAddressButton.SetText(Html.FromHtml("<small>Destination</small> <br/> <b>426 rue Saint Gabriel, Montreal QC</br>").ToString(), TextView.BufferType.Editable);

        //    // Create your application here
        //    // this.InitializeDropDownMenu();
        //}

        protected override void OnViewModelSet()
        {
            LocationService = new LocationService();
            LocationService.Start();
            SetContentView(Resource.Layout.Bookv2);

            // make bottom layout transparent
            var bottomLayout = FindViewById<FrameLayout>(Resource.Id.bottomLayout);
            AlphaAnimation alpha = new AlphaAnimation(0.1F, 0.1F);
            alpha.Duration = 0;
            alpha.FillAfter = true;
            bottomLayout.StartAnimation(alpha);

            //set text on button destinationAddressButton
            //var pickupAddressButton = FindViewById<Button>(Resource.Id.pickupAddressButton);
            //pickupAddressButton.SetText(Html.FromHtml("<small>Pickup</small> <br/> <b>3939 rue Rivard, Montreal QC</br>").ToString(), TextView.BufferType.Editable);
            

            //var destinationAddressButton = FindViewById<Button>(Resource.Id.destinationAddressButton);
            //destinationAddressButton.SetText(Html.FromHtml("<small>Destination</small> <br/> <b>426 rue Saint Gabriel, Montreal QC</br>").ToString(), TextView.BufferType.Editable);

             

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

       /* protected void InitializeDropDownMenu()
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
        }*/
    }
}