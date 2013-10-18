using System;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RestService("/account/register", "POST")]
    public class RegisterAccount : BaseDTO
    {
        public Guid AccountId { get; set; }

        public string Email { get; set; }

        public string Name { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; } 

        public string Password { get; set; }

        public string Phone { get; set; }

        public string FacebookId { get; set; }

        public string TwitterId { get; set; }

        public string Language { get; set; }

        public bool IsAdmin { get; set; }

        public bool AccountActivationDisabled { get; set; }

    }
}
