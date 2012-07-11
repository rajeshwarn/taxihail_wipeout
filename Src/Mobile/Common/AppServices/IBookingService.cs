using System;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Api.Contract.Resources;
using System.Collections.Generic;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IBookingService
	{
		
		bool IsValid( ref BookingInfoData info );
				
		bool IsCompleted(Account user, int orderId);
		
		bool IsCompleted( int statusId );
        
        bool CancelOrder(Account user, int orderId);

        int CreateOrder(Account user, BookingInfoData info, out string error);

        apcurium.MK.Booking.Mobile.Data.OrderStatus GetOrderStatus(Account user, int orderId);
		
		
	}
}

