#region

using System;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class RegisterAccount : ICommand
    {
        public RegisterAccount()
        {
            Id = Guid.NewGuid();
        }

        public Guid AccountId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        public int IbsAccountId { get; set; }
        public string ConfimationToken { get; set; }
        public string Language { get; set; }
        public bool IsAdmin { get; set; }
        public bool AccountActivationDisabled { get; set; }
        public Guid Id { get; set; }
    }
}