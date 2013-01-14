using System;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using apcurium.MK.Booking.Mobile.AppServices;
using Cirrious.MvvmCross.ExtensionMethods;
using System.Collections.Generic;
using ServiceStack.Text;

namespace apcurium.MK.Booking.Mobile
{
	public class NotificationViewModel: BaseViewModel,
		IMvxServiceConsumer<IAccountService>,
		IMvxServiceConsumer<IBookingService>
	{
		public NotificationViewModel (string orderId)
		{
			this.OrderId = Guid.Parse(orderId);
		}

		public override void Start (bool firstStart)
		{
			base.Start (firstStart);

			var accountService = this.GetService<IAccountService> ();
			var bookingService = this.GetService<IBookingService> ();
			
			var orderStatus = bookingService.GetOrderStatus (OrderId);
			var order = accountService.GetHistoryOrder (OrderId);
			
			if (order != null && orderStatus != null) {

				RequestNavigate<BookingStatusViewModel>(new Dictionary<string, string> {
					{"order", order.ToJson()},
					{"orderStatus", orderStatus.ToJson()},
				},clearTop: true);
				
			}

		}

		public override void Stop ()
		{
			base.Stop ();

			Close ();
		}

		public Guid OrderId {
			get;
			private set;
		}
	}
}

