using System;
using Xamarin.Contacts;
using System.Collections.Generic;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
	public interface IAddressBookService
	{
		List<Contact> LoadContacts(  );
	}
}

