using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using TinyMessenger;

namespace apcurium.MK.Booking.Mobile.Messages
{
    public class CreditCardRemoved: GenericTinyMessage<Guid>
    {

        public CreditCardRemoved(object sender, Guid creditCardId)
            : base(sender, creditCardId)
        {
        }
    }
}