using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TinyMessenger;

namespace apcurium.MK.Booking.Mobile.Messages
{
    public class CreditCardRemoved: GenericTinyMessage<Guid>
    {

        public CreditCardRemoved(object sender, Guid creditCardId, string ownerId)
            : base(sender, creditCardId)
        {
            OwnerId = ownerId;
        }

        public string OwnerId { get; private set; }
    }
}