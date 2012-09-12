using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Client.Models;

namespace apcurium.MK.Booking.Mobile.Client.Adapters
{
    public class LocationListAdapter : BaseAdapter<AddressItemListModel>
    {
        private readonly Activity _context;
        public List<AddressItemListModel> ListAddress { get; set; }

        public LocationListAdapter(Activity context, List<AddressItemListModel> objects)
            : base()
        {
            ListAddress = objects;
            this._context = context;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = ListAddress[position];
            
            
            TitleSubTitleListItemController controller = null;
            if (item.Address.IsHistoric)
            {
                if ((convertView == null) || new TitleSubTitleListItemController(convertView).HasSubTitle)
                {
                    controller = new TitleSubTitleListItemController(_context.LayoutInflater.Inflate(Resource.Layout.TitleListItem, null));
                }
                else
                {
                    controller = new TitleSubTitleListItemController(convertView);
                }
                controller.Title = this.ListAddress[position].Address.FullAddress;                
            }
            else
            {
                if ((convertView == null) || !( new TitleSubTitleListItemController(convertView).HasSubTitle))
                {
                    controller = new TitleSubTitleListItemController(_context.LayoutInflater.Inflate(Resource.Layout.TitleSubTitleListItem, null));
                }
                else
                {
                    controller = new TitleSubTitleListItemController(convertView);
                }                
                controller.Title = this.ListAddress[position].Address.FriendlyName;                                
                controller.SubTitle = this.ListAddress[position].Address.FullAddress;
                
            }

            controller.SetBackImage(this.ListAddress[position].BackgroundImageResource);
            controller.SetNavIcon(this.ListAddress[position].NavigationIconResource);

            return controller.View;
        }

        public override int Count
        {
            get { return ListAddress.Count; }
        }

        public override AddressItemListModel this[int position]
        {
            get { return ListAddress[position]; }
        }
    }
}