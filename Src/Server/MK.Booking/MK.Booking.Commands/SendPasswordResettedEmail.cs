using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class SendPasswordResettedEmail : ICommand
    {
        public SendPasswordResettedEmail()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }
    }
}
