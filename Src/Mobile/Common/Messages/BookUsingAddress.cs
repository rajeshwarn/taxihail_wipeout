using TinyMessenger;
using apcurium.MK.Common.Entity;

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