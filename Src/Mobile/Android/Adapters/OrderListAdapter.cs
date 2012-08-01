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
using apcurium.MK.Booking.Mobile.Client.Models;

namespace apcurium.MK.Booking.Mobile.Client.Adapters
{
    public class OrderListAdapter : BaseAdapter<OrderItemListModel>
    {
        private readonly Activity _context;
        public List<OrderItemListModel> ListOrder { get; set; }

        public OrderListAdapter(Activity context, List<OrderItemListModel> objects)
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
            var layout = view.FindViewById<LinearLayout>(Resource.Id.OrderListLayout);
            var title = view.FindViewById<TextView>(Resource.Id.OrderListTitle);
            var subtitle = view.FindViewById<TextView>(Resource.Id.OrderListSubtitle);
            var image = view.FindViewById<ImageView>(Resource.Id.OrderListPicture);

            title.Text = this.ListOrder[position].Order.IBSOrderId.ToString();
            subtitle.Text = this.ListOrder[position].Order.PickupAddress.FullAddress;

            layout.SetBackgroundResource(this.ListOrder[position].BgResource);
            try
            {
                image.SetImageDrawable(this._context.Resources.GetDrawable(this.ListOrder[position].ImageResource));
            }
            catch (Exception)
            {
                throw;
            }
            return view;
        }

        public override int Count
        {
            get { return ListOrder.Count; }
        }

        public override OrderItemListModel this[int position]
        {
            get { return ListOrder[position]; }
        }
    }
}