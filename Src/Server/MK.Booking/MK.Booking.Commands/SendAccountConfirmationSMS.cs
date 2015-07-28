using System;
using Infrastructure.Messaging;
using apcurium.MK.Common;

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
        
        public CountryISOCode CountryCode { get; set; }

        public string PhoneNumber { get; set; }
        
        public Guid Id { get; set; }
    }
}