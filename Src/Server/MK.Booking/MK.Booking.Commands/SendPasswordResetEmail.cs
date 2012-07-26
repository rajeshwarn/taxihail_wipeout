using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class SendPasswordResetEmail : ICommand
    {
        public SendPasswordResetEmail()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }
    }
}
