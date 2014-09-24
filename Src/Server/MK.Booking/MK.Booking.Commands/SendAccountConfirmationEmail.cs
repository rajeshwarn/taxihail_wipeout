#region

using System;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class SendAccountConfirmationEmail : ICommand
    {
        public SendAccountConfirmationEmail()
        {
            Id = Guid.NewGuid();
        }

        public string ClientLanguageCode { get; set; }
        public string EmailAddress { get; set; }
        public Uri ConfirmationUrl { get; set; }
        public Uri BaseUrl { get; set; }
        public Guid Id { get; set; }
    }
}