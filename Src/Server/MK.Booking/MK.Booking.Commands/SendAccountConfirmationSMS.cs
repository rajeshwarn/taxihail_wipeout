using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class SendAccountConfirmationSMS : ICommand
    {
        public SendAccountConfirmationSMS()
        {
            Id = Guid.NewGuid();
        }

        public string ClientLanguageCode { get; set; }
        public string Code { get; set; }
        public string PhoneNumber { get; set; }
        
        public Guid Id { get; set; }
    }
}