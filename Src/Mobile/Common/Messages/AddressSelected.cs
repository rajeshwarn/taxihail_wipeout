using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TinyMessenger;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.Messages
{
    public class AddressSelected : GenericTinyMessage<Address>
    {

        public AddressSelected(object sender, Address address, string ownerId)
            : base(sender, address)
        {
            OwnerId = ownerId;
        }

        public string OwnerId { get; private set; }



    }
}