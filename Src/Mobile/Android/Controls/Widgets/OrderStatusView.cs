using apcurium.MK.Booking.Mobile.ViewModels;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.Views;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	[Register("apcurium.MK.Booking.Mobile.Client.Controls.Widgets.OrderStatusView")]
	public class OrderStatusView : MvxFrameControl
	{
		private OrderStatusContactTaxiOverlay _contactTaxiOverlay;

		public OrderStatusView(Context context, IAttributeSet attrs) : base(Resource.Layout.SubView_BookingStatus, context, attrs)
		{
			this.DelayBind(() =>
			{
				_contactTaxiOverlay = FindViewById<OrderStatusContactTaxiOverlay>(Resource.Id.ContactTaxiOverlay);
				
				var set = this.CreateBindingSet<OrderStatusView, BookingStatusViewModel>();

				set.Bind(_contactTaxiOverlay)
					.For("DataContext")
					.To(vm => vm);

				set.Bind(_contactTaxiOverlay)
					.For(v => v.Visibility)
					.To(vm => vm.IsContactTaxiVisible)
					.WithConversion("Visibility");

				set.Apply();
			});
		}
	}
}