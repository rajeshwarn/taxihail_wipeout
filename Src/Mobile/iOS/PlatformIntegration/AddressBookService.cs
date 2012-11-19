using System;
using System.Collections.Generic;
using Xamarin.Contacts;
using MonoTouch.AddressBook;
using System.Linq;
using apcurium.MK.Booking.Mobile.Infrastructure;
using MonoTouch.Foundation;
using apcurium.MK.Common;
using apcurium.MK.Common.Extensions;

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
			ABAddressBook ab = default(ABAddressBook);
			try
			{
				ab = ABAddressBook.Create( out err);
				ab.RequestAccess( (success, e) => {
					if( !success )
					{
						Console.WriteLine("Access not granted to AddressBook" );
					}
				});
			}
			catch(System.EntryPointNotFoundException e) {
				// iOS5 or lower
				ab = new ABAddressBook();
			}

			var list = new List<Contact>();
            var persons = ab.GetPeople().ToArray();
            foreach( ABPerson p in persons )
			{
                var addr = p.GetAllAddresses();
                if ( addr.Count > 0 )
                {				    
				    var addresses = addr.GetValues().Select( pa => new Address() { StreetAddress = pa.Street, City = pa.City, Region = pa.State, Country = pa.Country, PostalCode = pa.Zip });
                    var displayName = Params.Get(p.FirstName , p.LastName).Where( s=>s.HasValue() ).JoinBy( " " );
                    list.Add( new Contact() {  Addresses = addresses, DisplayName = displayName, FirstName = p.FirstName, LastName = p.LastName });
                }
			}

			return list;
		}

	}
}

