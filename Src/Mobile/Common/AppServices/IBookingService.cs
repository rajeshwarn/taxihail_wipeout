using System;
using apcurium.MK.Booking.Mobile.Data;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IBookingService
	{
		
		bool IsValid( ref BookingInfoData info );
			
		
		LocationData[] SearchAddress( string address );
		
		LocationData[] SearchAddress( double latitude, double longitude );
	
		LocationData[] FindSimilar( string address );

		double? GetRouteDistance ( double originLong, double originLat, double destLong, double destLat );
		
		
		bool IsCompleted(AccountData user, int orderId );
		
		bool IsCompleted( int statusId );
		
		
		
		
		bool CancelOrder (AccountData user, int orderId );
		
		int CreateOrder( AccountData user , BookingInfoData info  , out string error);
		
		OrderStatus GetOrderStatus (AccountData user, int orderId);
		
		
	}
}

