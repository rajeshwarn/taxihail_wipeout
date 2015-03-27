#region

using System;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class RegisterFacebookAccount : ICommand
    {
        public RegisterFacebookAccount()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        public Guid AccountId { get; set; }

        public string FacebookId { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string Language { get; set; }

        public string PayBack { get; set; }
    }
}