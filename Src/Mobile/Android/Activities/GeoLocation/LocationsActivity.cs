using System.Linq;
using System.Collections.Generic;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Models;
using apcurium.MK.Booking.Mobile.ViewModels;
using TinyIoC;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Messages;
using TinyMessenger;
using apcurium.MK.Common.Entity;
using Cirrious.MvvmCross.Binding.Android.Views;

namespace apcurium.MK.Booking.Mobile.Client.Activities.GeoLocation
{
    [Activity(Label = "Locations", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
	public class LocationListActivity : BaseBindingActivity<MyLocationsViewModel>
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

		protected override void OnViewModelSet()
		{
            this.SetContentView(Resource.Layout.View_LocationList);
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
            var adapter = new MvxBindableListAdapter(this);

                RunOnUiThread(() =>
                {
					((MvxBindableListView)_listView).Adapter = adapter;
                    _listView.Divider = null;
                    _listView.DividerHeight = 0;
                    _listView.SetPadding(10, 0, 10, 0);
                });
        }

        private void UpdateUI()
        {
            _listView = FindViewById<ListView>(Resource.Id.LocationListView);
            _listView.CacheColorHint = Color.Transparent;
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
            

            return ailm;
        }

        protected override void OnResume()
        {
            base.OnResume();
            SetAdapter();
        }
    }
}