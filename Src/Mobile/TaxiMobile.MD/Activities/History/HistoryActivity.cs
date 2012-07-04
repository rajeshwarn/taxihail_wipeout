using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using TaxiMobile.Adapters;
using TaxiMobile.Lib.Framework.Extensions;
using TaxiMobile.Models;

namespace TaxiMobile.Activities.History
{
    [Activity(Label = "History", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation=ScreenOrientation.Portrait)]
    public class HistoryActivity : Activity
    {
        public CustomListAdapter Adapter { get; set; }
        public static string ITEM_TITLE = "TITLE";
        public static string ITEM_ID = "ID";
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
			SetAdapter();
			UpdateUI();
        }
		
		private void SetAdapter()
		{
            var historyView = GetHistory();
            Adapter = new CustomListAdapter(this);
            Adapter.AddSection(Resources.GetString(Resource.String.HistoryInfo), new SimpleAdapter(this, historyView, Resource.Layout.ListItem, new string[] { ITEM_TITLE }, new int[] { Resource.Id.ListComplexTitle }));			
		}

        private void UpdateUI()
        {
            ListView listView = new ListView(this);
            listView.Adapter = Adapter;
            listView.CacheColorHint = Color.Transparent; 
            this.SetContentView(listView);
            listView.ItemClick += new EventHandler<ItemEventArgs>(listView_ItemClick);
        }

        private IDictionary<string, object> CreateItem(string title, HistoryModel model)
        {
            IDictionary<string, object> item = new Dictionary<string, object>();
            item.Add(ITEM_TITLE, model.Display);
            item.Add(ITEM_ID, model.Id);
            return item;
        }

        void listView_ItemClick(object sender, ItemEventArgs e)
        {
             var item = Adapter.GetItem(e.Position);
             if ((item != null) && (item is IDictionary<string, object>))
             {
                 IDictionary<string, object> model = (IDictionary<string, object>)item;
                 int id = (int)model.GetValueOrDefault<string, object>(ITEM_ID);
                 Intent i = new Intent(this, typeof(HistoryDetailActivity));
                 i.PutExtra(NavigationStrings.HistorySelectedId.ToString(), id);
				 StartActivityForResult(i, (int)ActivityEnum.History );
                 //StartActivity(i);
             }

        }
		
		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);
			if( data != null )
			{
				switch( requestCode )
				{
				case (int)ActivityEnum.History:
					var rebookTripId = data.GetIntExtra("Rebook", 0);
					if( rebookTripId != 0 )
					{
						//AppContext.Current.TripIdToRebook = rebookTripId;
						var parent = (MainActivity)Parent;
                        parent.RebookTrip(rebookTripId);
						parent.MainTabHost.CurrentTab = 0;
					    
					}

                    var bookId = data.GetIntExtra("Book", 0);
                    if (bookId != 0)
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
             var historic = AppContext.Current.LoggedUser.BookingHistory.Where (b => !b.Hide).OrderByDescending( b=>b.Id ).ToArray();         

                historic.ForEach(his =>
                {
                    var history = new HistoryModel()
                    {
                        Display = "#" + his.Id + " - " + his.PickupLocation.Address,
                        Id = his.Id,
                    };
                    result.Add(CreateItem(ITEM_TITLE, history));
                });
            return result;
        }
		
		protected override void OnResume ()
		{
			base.OnResume ();
			SetAdapter();
			UpdateUI();
		}


    }
}