using System;
using System.Collections.Generic;
using System.Text;
using Android.Widget;
using Android.App;
using apcurium.MK.Booking.Mobile.Client.Models;

namespace apcurium.MK.Booking.Mobile.Client.Adapters
{
    public class HistoryListAdapter : BaseAdapter
    {
        private Activity _context;
        public List<HistoryModel> Items { get; set; }
        public HistoryListAdapter(Activity context,List<HistoryModel> items)
            : base()
        {
            _context = context;
            this.Items = items;
        }


        public override int Count
        {
            get { return Items.Count; }
        }
        public override Java.Lang.Object GetItem(int position)
        {
            return position;
        }
        
        public override long GetItemId(int position)
        {
            return -1;
            //return Items[position].Id;
        }
        public override Android.Views.View GetView(int position, Android.Views.View convertView, Android.Views.ViewGroup parent)
        {
            var item = Items[position];
            var view = (convertView ??_context.LayoutInflater.Inflate(Resource.Layout.HistoryListItem,parent,false)) as LinearLayout;
            var display = view.FindViewById(Resource.Id.HistoryItemTitle) as TextView;
            display.SetText(item.Display, TextView.BufferType.Normal);
            return view;

        }

        public HistoryModel GetItemAtPosition(int position)
        {
            return Items[position];
        }
    }
}
