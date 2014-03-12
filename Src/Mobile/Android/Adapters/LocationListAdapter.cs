using System.Collections;
using Android.App;
using Android.Views;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.Droid.Views;

namespace apcurium.MK.Booking.Mobile.Client.Adapters
{
    public class LocationListAdapter : MvxAdapter
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

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
			var item = (AddressViewModel) GetRawItem(position);

            return base.GetBindableView(convertView, new
            {
                DisplayLine1 = item.Address.FriendlyName,
                DisplayLine2 = item.Address.FullAddress,
                item.IsFirst,
                item.IsLast,
                item.ShowRightArrow,
                item.ShowPlusSign,
                item.Icon
            }, Resource.Layout.SimpleListItem);
        }
    }
}