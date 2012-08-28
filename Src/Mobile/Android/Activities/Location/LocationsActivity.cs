using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
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
using apcurium.MK.Booking.Mobile.Client.Activities.Book;
using System.Globalization;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Location
{
    [Activity(Label = "Locations", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class LocationsActivity : Activity
    {
        private ParentScreens _parent = ParentScreens.LocationScreen;
		private LocationTypes _viewMode;
        private ListView _listView;
        public static string ITEM_TITLE = "TITLE";
        public static string ITEM_SUBTITLE = "SUBTITLE";
        public static string ITEM_DATA = "DATA";
		private LocationService _locService;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            if (Intent.Extras != null)
            {
                _parent = (ParentScreens)Intent.Extras.GetInt(NavigationStrings.ParentScreen.ToString());
				_viewMode = (LocationTypes)Intent.Extras.GetInt(NavigationStrings.LocationType.ToString() );
            }

			if( _viewMode == LocationTypes.NearbyPlaces )
			{
				_locService = new LocationService();
				_locService.Start();
			}

            UpdateUI();
        }

        private void SetAdapter()
        {
            ThreadHelper.ExecuteInThread(this, () => {

				var adapter = new GroupedLocationListAdapter(this);

				if( _viewMode == LocationTypes.NearbyPlaces )
				{
					SetNearbyPlacesAdapter( adapter );
				}
				else
				{
					SetFavoriteAdapter( adapter );
				}

                RunOnUiThread(() => {
					_listView.Adapter = adapter;
                    _listView.Divider = null;
                    _listView.DividerHeight = 0;
                    _listView.SetPadding(10, 0, 10, 0);
				});
                
			}, true);
        }

        private void UpdateUI()
        {
            if (_parent != ParentScreens.BookScreen)
            {
                this.SetContentView(Resource.Layout.LocationList);     
                _listView = FindViewById<ListView>(Resource.Id.LocationListView);
                _listView.CacheColorHint = Color.Transparent;
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
            var adapter = _listView.Adapter as GroupedLocationListAdapter;
            if (adapter == null || adapter.GetItem(e.Position) == null)
            {
                return;
            }

            var item = adapter.GetItem(e.Position).Cast<AddressItemListModel>();

            if ((item.Address != null))
            {

                string data = item.Address.Serialize();

                Intent i = new Intent(this, typeof(LocationDetailActivity));
                i.PutExtra(NavigationStrings.LocationSelectedId.ToString(), data);
                StartActivity(i);
            }
        }
        private void listView_ItemClickFromBook(object sender, AdapterView.ItemClickEventArgs e)
        {
            var adapter = _listView.Adapter as GroupedLocationListAdapter;
            if (adapter == null || adapter.GetItem(e.Position) == null)
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
			if( type == LocationTypes.NearbyPlaces )
			{
				bool timeoutExpired;
				_locService.WaitForAccurateLocation(6000, 200, out timeoutExpired);
				addresses = TinyIoCContainer.Current.Resolve<IGoogleService>().GetNearbyPlaces( _locService.LastLocation.Latitude, _locService.LastLocation.Longitude );
			}
            else if (type == LocationTypes.Favorite)
            {
                addresses = TinyIoCContainer.Current.Resolve<IAccountService>().GetFavoriteAddresses();
            }
            else
            {
                addresses = TinyIoCContainer.Current.Resolve<IAccountService>().GetHistoryAddresses();
            }
            List<AddressItemListModel> ailm = addresses.Select(address => new AddressItemListModel()
                                                                              {
                                                                                  Address = address,
                                                                                  BackgroundImageResource = Resource.Drawable.cell_middle_state,
                                                                                  NavigationIconResource = Resource.Drawable.right_arrow
                                                                              }).ToList();
            if (ailm.Any())
            {
                ailm.First().BackgroundImageResource = Resource.Drawable.cell_top_state;
                if (type.Equals(LocationTypes.History))
                {
                    if (ailm.Count().Equals(1))
                    {
                        ailm.First().BackgroundImageResource = Resource.Drawable.blank_single_state;
                    }
                    else
                    {
                        ailm.Last().BackgroundImageResource = Resource.Drawable.blank_bottom_state;
                    }
                }
            }

            return ailm;
        }

        protected override void OnResume()
        {
            base.OnResume();
            SetAdapter();
        }

		private void SetFavoriteAdapter( GroupedLocationListAdapter adapter )
		{
			var historyAddresses = GetLocations(LocationTypes.History);
			var favoriteAddresses = GetLocations(LocationTypes.Favorite);
			
			if (_parent != ParentScreens.BookScreen)
			{
				int bgId = favoriteAddresses.Count >= 1 ? Resource.Drawable.cell_bottom_state : Resource.Drawable.add_single_state;
				
				favoriteAddresses.Add(new AddressItemListModel() {
					Address = new Address() { Id = Guid.Empty, FullAddress = Resources.GetString(Resource.String.LocationAddFavoriteSubtitle), FriendlyName = Resources.GetString(Resource.String.LocationAddFavoriteTitle) },
					BackgroundImageResource = bgId,
					NavigationIconResource = Resource.Drawable.add_button
				});
			}
			
			adapter.AddSection(Resources.GetString(Resource.String.FavoriteLocationsTitle), new LocationListAdapter(this, favoriteAddresses));
			adapter.AddSection(Resources.GetString(Resource.String.LocationHistoryTitle), new LocationListAdapter(this, historyAddresses));
		}

		private void SetNearbyPlacesAdapter( GroupedLocationListAdapter adapter )
		{
			var places = GetLocations(_viewMode);

			adapter.AddSection(Resources.GetString(Resource.String.NearbyPlacesTitle), new LocationListAdapter(this, places));
		}
    }
}