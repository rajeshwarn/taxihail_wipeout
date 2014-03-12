using apcurium.MK.Common.Entity;
using TinyMessenger;

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