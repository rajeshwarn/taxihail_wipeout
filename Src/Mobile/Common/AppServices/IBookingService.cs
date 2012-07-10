using System;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IBookingService
	{
		
		bool IsValid( ref BookingInfoData info );
			
		
        //LocationData[] SearchAddress( string address );
		
        //LocationData[] SearchAddress( double latitude, double longitude );
	
		LocationData[] FindSimilar( string address );

		//double? GetRouteDistance ( double originLong, double originLat, double destLong, double destLat );


        bool IsCompleted(Account user, int orderId);
		
		bool IsCompleted( int statusId );




        bool CancelOrder(Account user, int orderId);

        int CreateOrder(Account user, BookingInfoData info, out string error);

        apcurium.MK.Booking.Mobile.Data.OrderStatus GetOrderStatus(Account user, int orderId);
		
		
	}
}

