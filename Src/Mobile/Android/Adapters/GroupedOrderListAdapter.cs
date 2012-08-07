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

namespace apcurium.MK.Booking.Mobile.Client.Adapters
{
    public class GroupedOrderListAdapter : BaseAdapter
    {
        private Activity _context;

        public IDictionary<string, OrderListAdapter> Sections { get; set; }
        public ArrayAdapter<string> Headers { get; set; }
        public static int TYPE_SECTION_HEADER = 0;

        public static string ITEM_TITLE = "TITLE";
        public static string ITEM_SUBTITLE = "SUBTITLE";
        public static string ITEM_ID = "ID";

        public GroupedOrderListAdapter(Activity context)
            : base()
        {
            _context = context;
            Headers = new ArrayAdapter<string>(context, Resource.Layout.ListHeader);
            Sections = new Dictionary<string, OrderListAdapter>();
        }
        public void AddSection(string section, OrderListAdapter adapter)
        {
            Headers.Add(section);
            Sections.Add(new KeyValuePair<string, OrderListAdapter>(section, adapter));
        }

        public override int Count
        {
            get
            {
                int total = 0;
                foreach (var adapter in this.Sections.Values)
                {
                    total += adapter.Count + 1;
                }
                return total;
            }
        }
        public override int ViewTypeCount
        {
            get
            {
                int total = 1;
                foreach (var adapter in this.Sections.Values)
                {
                    total += adapter.ViewTypeCount;
                }


                return total;
            }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            foreach (var section in this.Sections.Keys)
            {
                OrderListAdapter adapter = Sections[section];
                int size = adapter.Count + 1;
                if (position == 0)
                {
                    return section;
                }
                if (position < size)
                {
                    return adapter.GetItem(position - 1);
                }
                position -= size;

            }

            return null;
        }

        public override int GetItemViewType(int position)
        {
            int type = 1;
            foreach (var section in this.Sections.Keys)
            {
                OrderListAdapter adapter = this.Sections[section];
                int size = adapter.Count + 1;
                if (position == 0)
                {
                    return TYPE_SECTION_HEADER;
                }
                if (position < size)
                {
                    return type + adapter.GetItemViewType(position - 1);
                }
                position -= size;
                type += adapter.ViewTypeCount;
            }
            return -1;
        }
        public override bool AreAllItemsEnabled()
        {
            return false;
        }

        public override long GetItemId(int position)
        {
            return position;
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            int sectionNum = 0;
            foreach (var section in this.Sections.Keys)
            {
                OrderListAdapter adapter = this.Sections[section];
                int size = adapter.Count + 1;

                // check if position inside this section
                if (position == 0)
                {
                    var header = Headers.GetView(sectionNum, convertView, parent);
                    if (header is TextView)
                    {
                        ((TextView)header).Typeface = AppFonts.Bold;
                    }
                    return header;
                }
                if (position < size)
                {                    
                    return adapter.GetView(position - 1, convertView, parent);                 
                }

                // otherwise jump into next section
                position -= size;
                sectionNum++;
            }
            return null;
        }
    }
}
