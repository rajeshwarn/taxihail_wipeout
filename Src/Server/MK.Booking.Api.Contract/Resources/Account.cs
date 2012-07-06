using System;

namespace apcurium.MK.Booking.Api.Contract.Resources
{    
        
    public class Account  
    {
        public Guid Id { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Phone { get; set; }

        public int IBSAccountid { get; set; }

        public BookingSettings Settings { get; set; }
    }
}
