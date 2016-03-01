using Android.Content;
using Android.Runtime;
using Android.Util;
using Cirrious.MvvmCross.Binding.Droid.Views;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	[Register("apcurium.MK.Booking.Mobile.Client.Controls.Widgets.AppBarBookingStatus")]
	public class AppBarBookingStatus : MvxFrameControl
	{
		public AppBarBookingStatus(Context context, IAttributeSet attrs) : base(Resource.Layout.SubView_Appbar_BookingStatus,context, attrs)
		{
		}
	}
}