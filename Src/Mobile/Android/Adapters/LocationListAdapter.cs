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
        private Color BgColor { get; set; }
        private string ImageUri { get; set; }

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
            View view = convertView;
            if (view == null)
            {
                view = _context.LayoutInflater.Inflate(Resource.Layout.LocationListItem, null);
            }
            var layout = view.FindViewById<LinearLayout>(Resource.Id.LocationListLayout);
            var title = view.FindViewById<TextView>(Resource.Id.LocationListTitle);
            var subtitle = view.FindViewById<TextView>(Resource.Id.LocationListSubtitle);
            var image = view.FindViewById<ImageView>(Resource.Id.LocationListPicture);

            title.Text = this.ListAddress[position].Address.FriendlyName;
            //subtitle.Text = this.ListAddress[position].Address.FullAddress.Length < 40 ? this.ListAddress[position].Address.FullAddress : this.ListAddress[position].Address.FullAddress.Substring(0,37) + "...";
            subtitle.Text = this.ListAddress[position].Address.FullAddress;
            layout.SetBackgroundResource(this.ListAddress[position].BgResource);
            try
            {
                image.SetImageDrawable(this._context.Resources.GetDrawable(this.ListAddress[position].ImageResource));
            }
            catch (Exception)
            {
                throw;
            }
            return view;
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