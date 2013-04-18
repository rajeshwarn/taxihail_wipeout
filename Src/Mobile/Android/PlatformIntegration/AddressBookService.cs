//using System;
//using System.Collections.Generic;
//using Xamarin.Contacts;
//using System.Linq;
//using apcurium.MK.Booking.Mobile.Infrastructure;
//using TinyIoC;


//namespace apcurium.MK.Booking.Mobile.Client
//{
//    public class AddressBookService : IAddressBookService
//    {
//        public AddressBookService ()
//        {
//        }

//        public List<Contact> LoadContacts( )
//        {
//            var book = TinyIoCContainer.Current.Resolve<AddressBook>();
//            book.PreferContactAggregation = true;

//            return book.ToList();
//        }

//    }
//}

