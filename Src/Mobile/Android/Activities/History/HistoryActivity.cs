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
using apcurium.MK.Booking.Api.Contract.Resources;
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
        private ListView _listView;


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.HistoryListView);
            _listView = FindViewById<ListView>(Resource.Id.HistoryList);
            _listView.ItemClick += new EventHandler<AdapterView.ItemClickEventArgs>(listView_ItemClick);
        }

        private void SetAdapter()
        {
            var historyView = GetHistory();

            var adapter = new CustomOrderListAdapter(this);
            adapter.AddSection(Resources.GetString(Resource.String.HistoryInfo), new OrderListAdapter(this, historyView));
            FindViewById<ListView>(Resource.Id.HistoryList).Adapter = adapter;
            _listView.Divider = null;
            _listView.DividerHeight = 0;
            _listView.SetPadding(10, 0, 10, 0);

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
            var adapter = FindViewById<ListView>(Resource.Id.HistoryList).Adapter as CustomOrderListAdapter;
            if (adapter == null)
            {
                return;
            }

            var item = adapter.GetItem(e.Position).Cast<OrderItemListModel>();
            if (item != null)
            {
                var id = item.Order.Id;
                Intent i = new Intent(this,typeof(HistoryDetailActivity));
                i.PutExtra(NavigationStrings.HistorySelectedId.ToString(), id.ToString());
                StartActivityForResult(i, (int)ActivityEnum.History);
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

        private List<OrderItemListModel> GetHistory()
        {

            IEnumerable<Order> orders = new Order[0];
            orders = TinyIoCContainer.Current.Resolve<IAccountService>().GetHistoryOrders().ToList();
            List<OrderItemListModel> ailm = orders.Select(order => new OrderItemListModel()
            {
                Order = order,
                BgResource = Resource.Drawable.cell_middle_state,
                ImageResource = Resource.Drawable.right_arrow
            }).ToList();
            if (ailm.Any())
            {
                ailm.First().BgResource = Resource.Drawable.cell_top_state;
                if (ailm.Count().Equals(1))
                {
                    ailm.First().BgResource = Resource.Drawable.cell_bottom_full_state;
                }
                else
                {
                    ailm.Last().BgResource = Resource.Drawable.cell_bottom_state;
                }
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