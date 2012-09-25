using System;
using System.Threading.Tasks;
using Xamarin.Contacts;
using apcurium.MK.Booking.Api.Contract.Resources;
using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Requests;
using Address = apcurium.MK.Common.Entity.Address;

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

        List<Address> GetAddressFromAddressBook(Predicate<Contact> criteria);

	    Task LoadContacts();

	}
}

