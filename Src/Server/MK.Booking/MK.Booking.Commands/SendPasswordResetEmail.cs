#region

using System;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class SendPasswordResetEmail : ICommand
    {
        public SendPasswordResetEmail()
        {
            Id = Guid.NewGuid();
        }

        public string ClientLanguageCode { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public Guid Id { get; set; }
    }
}