#region

using System;
using Infrastructure.Messaging;
using apcurium.MK.Common;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class RegisterAccount : ICommand
    {
        public RegisterAccount()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        public Guid AccountId { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public CountryISOCode Country { get; set; }

        public string Phone { get; set; }

        public string Password { get; set; }

        public string ConfimationToken { get; set; }

        public string Language { get; set; }

        public bool IsAdmin { get; set; }

        public bool AccountActivationDisabled { get; set; }

        public string PayBack { get; set; }
    }
}