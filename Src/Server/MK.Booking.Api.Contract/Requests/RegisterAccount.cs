#region

using System;
using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/account/register", "POST")]
    public class RegisterAccount : BaseDto
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

        public bool AccountActivationDisabled { get; set; }

		public bool IsConfirmed { get; set; }

        /// <summary>
        /// if null we will use the company settings to know if we need to send an email or a sms
        /// </summary>
        public ActivationMethod? ActivationMethod { get; set; }
    }
}