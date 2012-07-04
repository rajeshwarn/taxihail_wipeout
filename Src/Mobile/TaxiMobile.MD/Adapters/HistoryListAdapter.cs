using System.Collections.Generic;
using Android.App;
using Android.Views;
using Android.Widget;
using Java.Lang;
using TaxiMobile.Models;

namespace TaxiMobile.Adapters
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
        public override Object GetItem(int position)
        {
            return position;
        }
        public override long GetItemId(int position)
        {
            return Items[position].Id;
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
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
