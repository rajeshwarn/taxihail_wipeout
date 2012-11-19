using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Models;
using apcurium.MK.Booking.Mobile.Client.Adapters;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using TinyIoC;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Messages;
using TinyMessenger;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Location
{
    [Activity(Label = "Locations", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class LocationListActivity : BaseActivity
    {

        private ListView _listView;
        public static string ITEM_TITLE = "TITLE";
        public static string ITEM_SUBTITLE = "SUBTITLE";
        public static string ITEM_DATA = "DATA";
        private TinyMessageSubscriptionToken _closeViewToken;


        protected override int ViewTitleResourceId
        {
            get { return Resource.String.View_LocationList; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            _closeViewToken = TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Subscribe<CloseViewsToRoot>(m => Finish());
            UpdateUI();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_closeViewToken != null)
            {
                TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Unsubscribe<CloseViewsToRoot>(_closeViewToken);
            }
        }

        private void SetAdapter()
        {
            ThreadHelper.ExecuteInThread(this, () =>
            {

                var adapter = new GroupedLocationListAdapter(this);


                SetFavoriteAdapter(adapter);


                RunOnUiThread(() =>
                {
                    _listView.Adapter = adapter;
                    _listView.Divider = null;
                    _listView.DividerHeight = 0;
                    _listView.SetPadding(10, 0, 10, 0);
                });

            }, true);
        }

        private void UpdateUI()
        {
            this.SetContentView(Resource.Layout.View_LocationList);
            _listView = FindViewById<ListView>(Resource.Id.LocationListView);
            _listView.CacheColorHint = Color.Transparent;
            _listView.ItemClick += new EventHandler<AdapterView.ItemClickEventArgs>(listView_ItemClickNormal);
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
                StartActivityForResult(i, (int)ActivityEnum.FavoriteLocations);
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


        private List<AddressItemListModel> GetLocations(LocationType type)
        {
            IEnumerable<Address> addresses = new Address[0];
            if (type == LocationType.Favorite)
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
                if (type.Equals(LocationType.History))
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

        private void SetFavoriteAdapter(GroupedLocationListAdapter adapter)
        {
            var historyAddresses = GetLocations(LocationType.History);
            var favoriteAddresses = GetLocations(LocationType.Favorite);


            int bgId = favoriteAddresses.Count >= 1 ? Resource.Drawable.cell_bottom_state : Resource.Drawable.add_single_state;

            favoriteAddresses.Add(new AddressItemListModel()
            {
                Address = new Address() { Id = Guid.Empty, FullAddress = Resources.GetString(Resource.String.LocationAddFavoriteSubtitle), FriendlyName = Resources.GetString(Resource.String.LocationAddFavoriteTitle) },
                BackgroundImageResource = bgId,
                NavigationIconResource = Resource.Drawable.add_button
            });


            adapter.AddSection(Resources.GetString(Resource.String.FavoriteLocationsTitle), new LocationListAdapter(this, favoriteAddresses));
            adapter.AddSection(Resources.GetString(Resource.String.LocationHistoryTitle), new LocationListAdapter(this, historyAddresses));
        }

      


    }
}