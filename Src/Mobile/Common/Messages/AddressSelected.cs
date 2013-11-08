using TinyMessenger;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.Messages
{
    public class AddressSelected : GenericTinyMessage<Address>
    {

        public AddressSelected(object sender, Address address, string ownerId, bool shouldRecenter)
            : base(sender, address)
        {
            OwnerId = ownerId;
            ShouldRecenter = shouldRecenter;
        }

        public string OwnerId { get; private set; }

        public bool ShouldRecenter
        {
            get;
            set;
        }


    }
}