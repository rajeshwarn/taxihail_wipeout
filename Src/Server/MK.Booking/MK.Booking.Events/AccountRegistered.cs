using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class AccountRegistered : VersionedEvent
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public byte[] Password { get; set; }
        public int IbsAcccountId { get; set; }
    }
    
}
