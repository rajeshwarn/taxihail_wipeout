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
using Cirrious.MvvmCross.Binding.Android.Views;
using System.Collections;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Adapters
{
	public class LocationListAdapter : MvxBindableListAdapter
    {
        private readonly Activity _context;

        public LocationListAdapter(Activity context, IList itemsSource)
            : base(context)
        {
            this._context = context;
			ItemsSource = itemsSource;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = (AddressViewModel)ItemsSource[position];
            
            
            TitleSubTitleListItemController controller = null;
            if (item.Address.IsHistoric)
            {
                if ((convertView == null) || new TitleSubTitleListItemController(convertView).HasSubTitle)
                {
					var source = ItemsSource[position];
					var view = base.GetBindableView(convertView, source, Resource.Layout.TitleListItem);
                    controller = new TitleSubTitleListItemController(view);
                }
                else
                {
                    controller = new TitleSubTitleListItemController(convertView);
                }
                controller.Title = item.Address.FullAddress;                
            }
            else
            {
                if ((convertView == null) || !( new TitleSubTitleListItemController(convertView).HasSubTitle))
                {
					var source = ItemsSource[position];
					var view = base.GetBindableView(convertView, source, Resource.Layout.TitleSubTitleListItem);
                    controller = new TitleSubTitleListItemController(view);
                }
                else
                {
                    controller = new TitleSubTitleListItemController(convertView);
                }                
				controller.Title = item.Address.FriendlyName;                                
				controller.SubTitle = item.Address.FullAddress;
                
            }

			//controller.SetBackImage(item.BackgroundImageResource);
			//controller.SetNavIcon(item.NavigationIconResource);

            return controller.View;
        }
    }
}