using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.Client.Adapters
{
    public class OrderListAdapter : BaseAdapter<Order>
    {
        private readonly Activity _context;
        public List<Order> ListOrder { get; set; }

        public OrderListAdapter(Activity context, List<Order> objects)
            : base()
        {
            ListOrder = objects;
            this._context = context;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = ListOrder[position];
            View view = convertView;
            if (view == null)
            {
                view = _context.LayoutInflater.Inflate(Resource.Layout.OrderListItem, null);
            }
            var title = view.FindViewById<TextView>(Resource.Id.LocationListTitle);
            var subtitle = view.FindViewById<TextView>(Resource.Id.LocationListSubtitle);
            if (title != null)
            {
                title.Text = this.ListOrder[position].IBSOrderId.ToString();
            }
            if (subtitle != null)
            {
                subtitle.Text = this.ListOrder[position].PickupAddress.FullAddress;
            }
            return view;
        }

        public override int Count
        {
            get { return ListOrder.Count; }
        }

        public override Order this[int position]
        {
            get { return ListOrder[position]; }
        }
    }
}