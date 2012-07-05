using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class AddressList
    {
        public AddressList()
        {
            Addresses = new Address[0];
        }
        public Address[] Addresses { get; set; }
    }
}
