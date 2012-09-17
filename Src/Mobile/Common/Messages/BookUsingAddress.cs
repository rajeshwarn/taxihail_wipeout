using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TinyMessenger;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.Messages
{
    public class BookUsingAddress : GenericTinyMessage<Address>
    {
        public BookUsingAddress(object sender, Address address)
            : base(sender, address)
        {            
        }
    }
}