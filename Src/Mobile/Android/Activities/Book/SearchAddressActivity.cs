using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Widget;
using TinyIoC;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Client.Activities.History;
using apcurium.MK.Booking.Mobile.Client.Activities.Location;
using apcurium.MK.Booking.Mobile.Client.Adapters;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Models;
using apcurium.MK.Common.Extensions;
using Cirrious.MvvmCross.Binding.Android.Views;
using apcurium.MK.Booking.Mobile.ViewModels;
namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
	[Activity(Label = "SearchActivity", Theme = "@android:style/Theme.NoTitleBar")]
    public class SearchAddressActivity : MvxBindingActivityView<AddressSearchViewModel>
    {
        private LocationService _locService;
        private DateTime dateEvent;


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
          
        }

        protected override void OnViewModelSet()
        {
            _locService = new LocationService();
            _locService.Start();

            //var parent = (MainActivity)Parent;
            //parent.HideMainLayout(true);

            SetContentView(Resource.Layout.SearchAddress);

			FindViewById<Button>(Resource.Id.favbtn).Click += HandleClick;
			FindViewById<Button>(Resource.Id.placesbtn).Click += HandleClick;
			FindViewById<Button>(Resource.Id.contactbtn).Click += HandleClick;
			FindViewById<Button>(Resource.Id.searchbtn).Click += HandleClick;
            FindViewById<Button>(Resource.Id.searchbtn).Selected = true;

//            var validateSearchButton = FindViewById<Button>(Resource.Id.validateSearchButton);
//            validateSearchButton.Click += ValidateSearchButtonOnClick;
//            var searchTextView = FindViewById<AutoCompleteTextView>(Resource.Id.SearchAddressText);
//            searchTextView.TextChanged += SearchTextViewOnTextChanged;
            var _listView = FindViewById<MvxBindableListView>(Resource.Id.SearchAddressListView);
//            searchButton.Selected = true;
            _listView.Divider = null;
            _listView.DividerHeight = 0;
//            _listView.SetPadding(10, 0, 10, 0);


        }

        void HandleClick (object sender, EventArgs e)
        {
			FindViewById<Button>(Resource.Id.favbtn).Selected = false;
			FindViewById<Button>(Resource.Id.placesbtn).Selected = false;
			FindViewById<Button>(Resource.Id.contactbtn).Selected = false;
			FindViewById<Button>(Resource.Id.searchbtn).Selected = false;  
			((Button)sender).Selected = true;
        }
        

        private void SearchTextViewOnTextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            /*if ( textChangedEventArgs.AfterCount>4)
            {
                dateEvent = DateTime.Now
                var dateNow = DateTime.Now;
                dateNow.
            }*/
        }

//        private void ValidateSearchButtonOnClick(object sender, EventArgs eventArgs)
//        {
//            var _listView = FindViewById<ListView>(Resource.Id.SearchAddressListView);
//            var searchTextView = FindViewById<AutoCompleteTextView>(Resource.Id.SearchAddressText);
//            var addresses = new List<Address>();
//            if(searchTextView.Text.Any())
//            {
//                if (searchTextView.Text.ElementAt(0).SelectOrDefault(Char.IsNumber))
//                {
//                    addresses = TinyIoCContainer.Current.Resolve<IGeolocService>().SearchAddress(searchTextView.Text).ToList();
//                }
//                else
//                {
//                    addresses = TinyIoCContainer.Current.Resolve<IGeolocService>().SearchAddress(searchTextView.Text).ToList();
//                }
//            }
//            
//
//            _listView.Adapter = new LocationListAdapter(this, addressToAddressItemListModel(addresses));
//        }
//
//        private void SearchButtonOnClick(object sender, EventArgs eventArgs)
//        {
//            UnselectButton();
//            var searchButton = FindViewById<Button>(Resource.Id.searchbtn);
//            searchButton.Selected = true;
//            var _listView = FindViewById<ListView>(Resource.Id.SearchAddressListView);
//            _listView.Adapter = new LocationListAdapter(this, new List<AddressItemListModel>());
//        }
//
//        private void ContactsButtonOnClick(object sender, EventArgs eventArgs)
//        {
//            UnselectButton();
//            var _listView = FindViewById<ListView>(Resource.Id.SearchAddressListView);
//            var contactsButton = FindViewById<Button>(Resource.Id.contactbtn);
//            contactsButton.Selected = true;
//            var addresses = TinyIoCContainer.Current.Resolve<IBookingService>().GetAddressFromAddressBook();
//            _listView.Adapter = new LocationListAdapter(this, addressToAddressItemListModel(addresses));
//        }
//
//        private void PlacesButtonOnClick(object sender, EventArgs eventArgs)
//        {
//            UnselectButton();
//            var _listView = FindViewById<ListView>(Resource.Id.SearchAddressListView);
//            var placesButton = FindViewById<Button>(Resource.Id.placesbtn);
//            placesButton.Selected = true;
//            bool timeoutExpired;
//            _locService.WaitForAccurateLocation(6000, 200, out timeoutExpired);
//            var addresses = TinyIoCContainer.Current.Resolve<IGoogleService>().GetNearbyPlaces(_locService.LastLocation.Latitude, _locService.LastLocation.Longitude);
//            _listView.Adapter = new LocationListAdapter(this, addressToAddressItemListModel(addresses));
//        }
//
//        private void FavoriteButtonOnClick(object sender, EventArgs eventArgs)
//        {
//            UnselectButton();
//            var _listView = FindViewById<ListView>(Resource.Id.SearchAddressListView);
//            var favoriteButton = FindViewById<Button>(Resource.Id.favbtn);
//            favoriteButton.Selected = true;
//            var addresses = TinyIoCContainer.Current.Resolve<IAccountService>().GetFavoriteAddresses();
//            _listView.Adapter = new LocationListAdapter(this, addressToAddressItemListModel(addresses));
//        }
//
//        private List<AddressItemListModel> addressToAddressItemListModel(IEnumerable<Address> addresses)
//        {
//            var ailm = addresses.Select(address => new AddressItemListModel()
//            {
//                Address = address,
//                BackgroundImageResource = Resource.Drawable.cell_middle_state,
//                NavigationIconResource = Resource.Drawable.right_arrow
//            }).ToList();
//
//            if (ailm.Any())
//            {
//                ailm.First().BackgroundImageResource = Resource.Drawable.cell_top_state;
//
//                    if (ailm.Count().Equals(1))
//                    {
//                        ailm.First().BackgroundImageResource = Resource.Drawable.blank_single_state;
//                    }
//                    else
//                    {
//                        ailm.Last().BackgroundImageResource = Resource.Drawable.blank_bottom_state;
//                    }
//                
//            }
//
//            return ailm;
//        }
//
//        private void UnselectButton()
//        {
//            var favoriteButton = FindViewById<Button>(Resource.Id.favbtn);
//            favoriteButton.Selected = false;
//            var placesButton = FindViewById<Button>(Resource.Id.placesbtn);
//            placesButton.Selected = false;
//            var contactsButton = FindViewById<Button>(Resource.Id.contactbtn);
//            contactsButton.Selected = false;
//            var searchButton = FindViewById<Button>(Resource.Id.searchbtn);
//            searchButton.Selected = false;
//        }
    }
}