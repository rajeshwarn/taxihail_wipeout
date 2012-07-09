using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using TaxiMobile.Models;
using TaxiMobile.Adapters;
using apcurium.Framework.Extensions;
using TaxiMobileApp;
using Android.Graphics;
using TaxiMobile.Activities.Book;
using TaxiMobile.Helpers;
namespace TaxiMobile.Activities.Location
{
    [Activity(Label = "Locations", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class LocationsActivity : Activity
    {
        private ParentScreens _parent = ParentScreens.LocationScreen;
        private ListView _listView;
        public static string ITEM_TITLE = "TITLE";
        //public static string ITEM = "ITEM";
        public static string ITEM_DATA = "DATA";
        //public static string ITEM_ADDRESS = "ADDRESS";
        //   public CustomListAdapter Adapter { get; set; }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            if (Intent.Extras != null)
            {
                _parent = (ParentScreens)Intent.Extras.GetInt(NavigationStrings.ParentScreen.ToString());
            }

            //SetAdapter();
            UpdateUI();
        }

        private void SetAdapter()
        {
            var historyLocationsView = PopulateLocationView(LocationTypes.History);
            var favoriteLocationsView = PopulateLocationView(LocationTypes.Favorite);
            if (_parent != ParentScreens.BookScreen)
            {
                favoriteLocationsView.Add(CreateItem(new LocationData
                {
                    Id = -1,
                    IsFromHistory = false,
                    Address = Resources.GetString(Resource.String.LocationAddFavorite),

                }));
            }


            var adapter = new CustomListAdapter(this);
            adapter.AddSection(Resources.GetString(Resource.String.FavoriteLocationsTitle), new SimpleAdapter(this, favoriteLocationsView, Resource.Layout.SimpleListItem, new string[] { ITEM_TITLE }, new int[] { Resource.Id.ListComplexTitle }));
            adapter.AddSection(Resources.GetString(Resource.String.LocationHistoryTitle), new SimpleAdapter(this, historyLocationsView, Resource.Layout.SimpleListItem, new string[] { ITEM_TITLE }, new int[] { Resource.Id.ListComplexTitle }));
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

        //private void CreateTemplate(ListView listView)
        //{
        //    var header = new RelativeLayout(this);
        //    header.LayoutParameters = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
        //    header.SetGravity(GravityFlags.Center);
        //    var imageHeader = new ImageView(this);
        //    imageHeader.LayoutParameters = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.FillParent, RelativeLayout.LayoutParams.WrapContent);
        //    imageHeader.SetScaleType(ImageView.ScaleType.CenterInside);
        //    imageHeader.SetAdjustViewBounds(true);
        //    imageHeader.SetImageResource(Resource.Drawable.header);
        //    var title = new TextView(this);
        //    var lp = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MatchParent, RelativeLayout.LayoutParams.WrapContent)
        //        {
        //            AlignWithParent = true,
        //        };
        //    lp.AddRule(LayoutRules.CenterInParent);
        //    title.LayoutParameters = lp;
        //    title.SetTextAppearance(this, Resource.Style.BigTitleText);
        //    title.Gravity = GravityFlags.CenterHorizontal | GravityFlags.CenterVertical;
        //    title.Text = Resources.GetString(Resource.String.TabLocations);
        //    header.AddView(imageHeader);
        //    header.AddView(title);
        //    var body = new RelativeLayout(this);
        //    body.LayoutParameters = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
        //    var backgroundImage = new ImageView(this);
        //    backgroundImage.LayoutParameters = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.FillParent, RelativeLayout.LayoutParams.WrapContent);
        //    backgroundImage.SetScaleType(ImageView.ScaleType.Center);
        //    // backgroundImage.SetAdjustViewBounds(true);

        //    backgroundImage.SetImageResource(Resource.Drawable.background);
        //    listView.LayoutParameters = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.FillParent, RelativeLayout.LayoutParams.FillParent);
        //    body.AddView(backgroundImage);
        //    body.AddView(listView);
        //    LinearLayout parent = new LinearLayout(this);
        //    parent.LayoutParameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.FillParent);

        //    //android:layout_height="0dip"        android:layout_weight="1"

        //    parent.SetGravity(GravityFlags.CenterHorizontal);
        //    parent.Orientation = Orientation.Vertical;
        //    parent.AddView(header);
        //    parent.AddView(body);

        //    this.SetContentView(parent);
        //}
        private void listView_ItemClickNormal(object sender, AdapterView.ItemClickEventArgs e)
        {
            var adapter = _listView.Adapter as CustomListAdapter;
            if (adapter == null)
            {
                return;
            }

            var item = adapter.GetItem(e.Position).Cast<IDictionary<string, object>>(); 
            if ((item != null) && (item is IDictionary<string, object>))
            {
                IDictionary<string, object> model = (IDictionary<string, object>)item;

                string data = (string)model.GetValueOrDefault<string, object>(ITEM_DATA);

                Intent i = new Intent(this, typeof(LocationDetailActivity));
                i.PutExtra(NavigationStrings.LocationSelectedId.ToString(), data);
                StartActivity(i);
            }
        }
        private void listView_ItemClickFromBook(object sender, AdapterView.ItemClickEventArgs e)
        {
            var adapter = _listView.Adapter as CustomListAdapter;
            if (adapter == null)
            {
                return;
            }
            var item = adapter.GetItem(e.Position).Cast<IDictionary<string, object>>();
            if ((item != null))
            {
                var data = (string)item.GetValueOrDefault<string, object>(ITEM_DATA);
                Intent intent = new Intent();
                intent.SetFlags(ActivityFlags.ForwardResult);
                intent.PutExtra("SelectedAddress", data);
                SetResult(Result.Ok, intent);
                Finish();
            }
        }


        private List<IDictionary<string, object>> PopulateLocationView(LocationTypes locationType)
        {
            var locations = GetLocations(locationType);
            var result = new List<IDictionary<string, object>>();
            if (locationType == LocationTypes.History)
            {
                locations = locations.Where(o => o.IsFromHistory == true).ToList();
            }
            else
            {
                locations = locations.Where(o => o.IsFromHistory == false).ToList();
            }
            locations.ForEach(loc =>
                {
                    result.Add(CreateItem(loc));
                });

            return result;
        }

        private IDictionary<string, object> CreateItem(LocationData location)
        {
            IDictionary<string, object> item = new Dictionary<string, object>();
            item.Add(ITEM_TITLE, location.Display);
            item.Add(ITEM_DATA, location.Serialize());
            return item;
        }


        private List<LocationData> GetLocations(LocationTypes type)
        {
            LocationData[] locationsData;
            if (type == LocationTypes.Favorite)
            {
                locationsData = AppContext.Current.LoggedUser.FavoriteLocations;
            }
            else
            {
                var history = AppContext.Current.LoggedUser.BookingHistory.Where(b => !b.Hide && b.PickupLocation.Name.IsNullOrEmpty() && b.PickupLocation.Address.HasValue()).OrderByDescending(b => b.RequestedDateTime).GroupBy(l => l.PickupLocation.Address + "_" + l.PickupLocation.Apartment.ToSafeString() + "_" + l.PickupLocation.RingCode.ToSafeString());

                locationsData = history.Select(h =>
                                                   {
                                                       var r = h.ElementAt(0).PickupLocation;
                                                       r.Id = h.ElementAt(0).Id;
                                                       return r;
                                                   }).ToArray();


                if (locationsData.Count() == 0)
                {
                    locationsData = new LocationData[1];
                    locationsData[0] = new LocationData { IsHistoricEmptyItem = true, Id = -1 };
                }

                locationsData.ForEach(h =>
                {
                    h.IsFromHistory = true;

                });
            }
            return locationsData.ToList();
        }

        protected override void OnResume()
        {
            base.OnResume();
            SetAdapter();            
        }

    }
}