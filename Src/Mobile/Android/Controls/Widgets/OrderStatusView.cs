using Android.Content;
using Android.Runtime;
using Android.Util;
using Cirrious.MvvmCross.Binding.Droid.Views;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	[Register("apcurium.MK.Booking.Mobile.Client.Controls.Widgets.OrderStatusView")]
	public class OrderStatusView : MvxFrameControl
	{
		public OrderStatusView(Context context, IAttributeSet attrs) : base(Resource.Layout.SubView_BookingStatus, context, attrs)
		{

		}
	}
}