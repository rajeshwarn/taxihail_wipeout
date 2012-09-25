using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class AddressList : BaseDTO
    {
        public AddressList()
        {
            Addresses = new Address[0];
        }
        public Address[] Addresses { get; set; }
    }
}
