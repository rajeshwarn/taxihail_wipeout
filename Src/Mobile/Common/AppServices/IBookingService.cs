using System;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Api.Contract.Resources;
using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Requests;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IBookingService
	{

        bool IsValid(ref CreateOrder info);
				
		bool IsCompleted(Guid orderId);
		
		bool IsStatusCompleted( string statusId );
        
        bool CancelOrder(Guid orderId);

        OrderStatusDetail CreateOrder(CreateOrder info);

        OrderStatusDetail GetOrderStatus(Guid orderId);

	    void RemoveFromHistory(Guid orderId);


	}
}

