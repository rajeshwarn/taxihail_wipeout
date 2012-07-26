using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class SendAccountConfirmationEmail : ICommand
    {
        public SendAccountConfirmationEmail()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public string EmailAddress { get; set; }
        public Uri ConfirmationUrl { get; set; }
    }
}
