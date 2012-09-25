using TinyMessenger;
using apcurium.MK.Common.Entity;

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