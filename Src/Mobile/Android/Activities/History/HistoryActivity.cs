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
using apcurium.Framework.Extensions;
using Android.Graphics;
using apcurium.MK.Booking.Mobile.Client.Models;
using apcurium.MK.Booking.Mobile.Client.Adapters;
using TinyIoC;
using apcurium.MK.Booking.Mobile.AppServices;

namespace apcurium.MK.Booking.Mobile.Client.Activities.History
{
    [Activity(Label = "History", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class HistoryActivity : Activity
    {

        public static string ITEM_TITLE = "TITLE";
        public static string ITEM_ID = "ID";



        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.HistoryListView);
            FindViewById<ListView>(Resource.Id.HistoryList).ItemClick += new EventHandler<AdapterView.ItemClickEventArgs>(listView_ItemClick);
        }

        private void SetAdapter()
        {
            var historyView = GetHistory();
            var adapter = new CustomListAdapter(this);
            adapter.AddSection(Resources.GetString(Resource.String.HistoryInfo), new SimpleAdapter(this, historyView, Resource.Layout.SimpleListItem, new string[] { ITEM_TITLE }, new int[] { Resource.Id.ListComplexTitle }));
            FindViewById<ListView>(Resource.Id.HistoryList).Adapter = adapter;

        }

        private IDictionary<string, object> CreateItem(string title, HistoryModel model)
        {
            IDictionary<string, object> item = new Dictionary<string, object>();
            item.Add(ITEM_TITLE, model.Display);
            item.Add(ITEM_ID, model.Id);
            return item;
        }

        void listView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var adapter = FindViewById<ListView>(Resource.Id.HistoryList).Adapter as CustomListAdapter;
            if (adapter == null)
            {
                return;
            }

            var item = adapter.GetItem(e.Position).Cast<IDictionary<string, object>>();
            //var item = adapter.GetItem(e.Position);
            if ((item != null) && (item is IDictionary<string, object>))
            {
                IDictionary<string, object> model = (IDictionary<string, object>)item;
                var id = model.GetValueOrDefault<string, object>(ITEM_ID).ToString();
                Intent i = new Intent(this, typeof(HistoryDetailActivity));
                i.PutExtra(NavigationStrings.HistorySelectedId.ToString(), id);
                StartActivityForResult(i, (int)ActivityEnum.History);
                //StartActivity(i);
            }

        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (data != null)
            {
                switch (requestCode)
                {
                    case (int)ActivityEnum.History:
                        var rebookTripId = data.GetStringExtra("Rebook");
                        if (rebookTripId.HasValue())
                        {
                            //AppContext.Current.TripIdToRebook = rebookTripId;
                            var parent = (MainActivity)Parent;
                            parent.RebookTrip(new Guid(rebookTripId));
                            parent.MainTabHost.CurrentTab = 0;

                        }

                        var bookId = data.GetStringExtra("Book");
                        if (bookId.HasValue())
                        {
                            var parent = (MainActivity)Parent;
                            parent.MainTabHost.CurrentTab = 0;

                        }
                        break;
                        break;
                }
            }
        }

        private List<IDictionary<string, object>> GetHistory()
        {
            var result = new List<IDictionary<string, object>>();
            
            var orders = TinyIoCContainer.Current.Resolve<IAccountService>().GetHistoryOrders();
            //var historic = AppContext.Current.LoggedUser.BookingHistory.Where(b => !b.Hide).OrderByDescending(b => b.Id).ToArray();


            orders.ForEach(order =>
            {
                
                var history = new HistoryModel()
                {
                    Display = "#" + order.IBSOrderId + " - " + order.PickupAddress.FullAddress,
                    Id = order.Id,
                };
                result.Add(CreateItem(ITEM_TITLE, history));
            });
            return result;
        }

        protected override void OnResume()
        {
            base.OnResume();
            SetAdapter();
        }


    }
}