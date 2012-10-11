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


            TitleSubTitleListItemController controller = null;
            if (convertView == null)
            {
                controller = new TitleSubTitleListItemController(_context.LayoutInflater.Inflate(Resource.Layout.TitleSubTitleListItem, null));
            }
            else
            {
                controller = new TitleSubTitleListItemController(convertView);
            }
            var title = string.Format(_context.GetString(Resource.String.OrderHistoryListTitle), this.ListOrder[position].Order.IBSOrderId.ToString(), this.ListOrder[position].Order.CreatedDate );
            controller.Title = title;
            controller.SubTitle = this.ListOrder[position].Order.PickupAddress.FullAddress;
            controller.SetBackImage( this.ListOrder[position].BackgroundImageResource );
            controller.SetNavIcon( this.ListOrder[position].NavigationIconResource);

            return controller.View;
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