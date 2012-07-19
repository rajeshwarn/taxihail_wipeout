using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using apcurium.Framework.Extensions;
using apcurium.MK.Booking.Mobile.Client.Models;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Client.Adapters;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using TinyIoC;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Location
{
    [Activity(Label = "Locations", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class LocationsActivity : Activity
    {
        private ParentScreens _parent = ParentScreens.LocationScreen;
        private ListView _listView;
        public static string ITEM_TITLE = "TITLE";
        public static string ITEM_SUBTITLE = "SUBTITLE";
        public static string ITEM_DATA = "DATA";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            if (Intent.Extras != null)
            {
                _parent = (ParentScreens)Intent.Extras.GetInt(NavigationStrings.ParentScreen.ToString());
            }
            UpdateUI();
        }

        private void SetAdapter()
        {
            var historyAddresses = GetLocations(LocationTypes.History);
            var favoriteAddresses = GetLocations(LocationTypes.Favorite);
            if (_parent != ParentScreens.BookScreen)
            {
                favoriteAddresses.Add(new AddressItemListModel()
                                          {
                                              Address = new Address()
                                                            {
                                                                Id = Guid.Empty,
                                                                FullAddress =
                                                                    Resources.GetString(
                                                                        Resource.String.LocationAddFavoriteSubtitle),
                                                                FriendlyName =
                                                                    Resources.GetString(
                                                                        Resource.String.LocationAddFavoriteTitle)
                                                            },
                                              BgResource = Resource.Drawable.cell_bottom,
                                              ImageResource = Resource.Drawable.add_button
                                          });
            }

            var adapter = new CustomLocationListAdapter(this);
            adapter.AddSection(Resources.GetString(Resource.String.FavoriteLocationsTitle), new LocationListAdapter(this, favoriteAddresses));
            adapter.AddSection(Resources.GetString(Resource.String.LocationHistoryTitle), new LocationListAdapter(this, historyAddresses));
            _listView.Adapter = adapter;
        }

        private void UpdateUI()
        {
            if (_parent != ParentScreens.BookScreen)
            {
                _listView = new ListView(this);
                _listView.CacheColorHint = Color.Transparent;
                this.SetContentView(_listView);
                _listView.ItemClick += new EventHandler<AdapterView.ItemClickEventArgs>(listView_ItemClickNormal);
            }
            else
            {
                this.SetContentView(Resource.Layout.LocationPick);
                _listView = FindViewById<ListView>(Resource.Id.LocationListView);                
                _listView.CacheColorHint = Color.Transparent;
                _listView.ItemClick += new EventHandler<AdapterView.ItemClickEventArgs>(listView_ItemClickFromBook);
            }
        }

     
        private void listView_ItemClickNormal(object sender, AdapterView.ItemClickEventArgs e)
        {
            var adapter = _listView.Adapter as CustomLocationListAdapter;
            if (adapter == null || adapter.GetItem(e.Position)==null)
            {
                return;
            }

            var item = adapter.GetItem(e.Position).Cast<AddressItemListModel>();

            if ((item.Address != null) )
            {

                string data = item.Address.Serialize();

                Intent i = new Intent(this, typeof(LocationDetailActivity));
                i.PutExtra(NavigationStrings.LocationSelectedId.ToString(), data);
                StartActivity(i);
            }
        }
        private void listView_ItemClickFromBook(object sender, AdapterView.ItemClickEventArgs e)
        {
            var adapter = _listView.Adapter as CustomLocationListAdapter;
            if (adapter == null || adapter.GetItem(e.Position)==null)
            {
                return;
            }
            var item = adapter.GetItem(e.Position).Cast<AddressItemListModel>();
            if ((item.Address != null))
            {
                var data = item.Address.Serialize();
                Intent intent = new Intent();
                intent.SetFlags(ActivityFlags.ForwardResult);
                intent.PutExtra("SelectedAddress", data);
                SetResult(Result.Ok, intent);
                Finish();
            }
        }

        private IDictionary<string, object> CreateItem(Address location)
        {
            IDictionary<string, object> item = new Dictionary<string, object>();
            //item.Add(ITEM_TITLE, location.Display());
            item.Add(ITEM_TITLE, location.FriendlyName);
            item.Add(ITEM_SUBTITLE, location.FullAddress);
            item.Add(ITEM_DATA, location.Serialize());
            return item;
        }


        private List<AddressItemListModel> GetLocations(LocationTypes type)
        {
            IEnumerable<Address> addresses = new Address[0];
            if (type == LocationTypes.Favorite)
            {
                addresses = TinyIoCContainer.Current.Resolve<IAccountService>().GetFavoriteAddresses();                                
            }
            else
            {
                addresses = TinyIoCContainer.Current.Resolve<IAccountService>().GetHistoryAddresses();                
            }
            List<AddressItemListModel> ailm = addresses.Select(address => new AddressItemListModel()
                                                                              {
                                                                                  Address = address, BgResource = Resource.Drawable.cell_middle, ImageResource = Resource.Drawable.right_arrow
                                                                              }).ToList();
            if (ailm.Any())
            {
                ailm.First().BgResource = Resource.Drawable.cell_top;
            }
            return ailm;
        }

        protected override void OnResume()
        {
            base.OnResume();
            SetAdapter();            
        }

    }
}