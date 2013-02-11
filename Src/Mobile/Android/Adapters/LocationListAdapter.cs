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
        public LocationListAdapter(Activity context, IList itemsSource)
            : base(context)
        {
			ItemsSource = itemsSource;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView (int position, View convertView, ViewGroup parent)
		{
			var item = (AddressViewModel)ItemsSource [position];
            
			return base.GetBindableView (convertView, new {
					DisplayLine1 = item.Address.FriendlyName,
					DisplayLine2 = item.Address.FullAddress,
					IsFirst = item.IsFirst,
					IsLast = item.IsLast,
				    ShowRightArrow = item.ShowRightArrow,
					ShowPlusSign = item.ShowPlusSign,
                    Icon = item.Icon
				}, Resource.Layout.SimpleListItem);
        }
    }
}