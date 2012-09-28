using System;
using System.Collections.Generic;
using Xamarin.Contacts;
using MonoTouch.AddressBook;
using System.Linq;
using apcurium.MK.Booking.Mobile.Infrastructure;
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client
{
	public class AddressBookService : IAddressBookService
	{
		public AddressBookService ()
		{
		}

		public List<Contact> LoadContacts( )
		{
			NSError err;
			var ab = ABAddressBook.Create( out err);
			ab.RequestAccess( (success, e) => {
				if( !success )
				{
					Console.WriteLine("Access not granted to AddressBook" );
				}
			});

			var list = new List<Contact>();
			foreach( ABPerson p in ab )
			{
				var addr = p.GetAllAddresses();
				var addresses = addr.GetValues().Select( pa => new Address() { StreetAddress = pa.Street, City = pa.City, Region = pa.State, Country = pa.Country, PostalCode = pa.Zip });
				list.Add( new Contact() {  Addresses = addresses, DisplayName = p.FirstName + " " + p.LastName, FirstName = p.FirstName, LastName = p.LastName });
			}

			return list;
		}

	}
}

