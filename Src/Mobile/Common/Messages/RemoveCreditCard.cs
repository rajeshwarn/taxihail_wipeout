using System;
using TinyMessenger;

namespace apcurium.MK.Booking.Mobile.Messages
{
    public class RemoveCreditCard: GenericTinyMessage<Guid>
    {

        public RemoveCreditCard(object sender, Guid creditCardId)
            : base(sender, creditCardId)
        {
        }
    }
}