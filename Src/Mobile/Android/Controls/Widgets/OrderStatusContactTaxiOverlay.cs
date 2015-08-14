using Android.Content;
using Android.Runtime;
using Android.Util;
using Cirrious.MvvmCross.Binding.Droid.Views;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	[Register("apcurium.MK.Booking.Mobile.Client.Controls.Widgets.OrderStatusContactTaxiOverlay")]
	public class OrderStatusContactTaxiOverlay : MvxFrameControl
	{
		public OrderStatusContactTaxiOverlay(Context context, IAttributeSet attrs) : base(Resource.Layout.SubView_ContactTaxiOverlay,context, attrs)
		{
		}
	}
}