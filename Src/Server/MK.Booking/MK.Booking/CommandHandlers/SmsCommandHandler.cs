using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.SMS;
using apcurium.MK.Common.Configuration;
using Infrastructure.Messaging.Handling;

namespace apcurium.MK.Booking.CommandHandlers
{
    public class SmsCommandHandler : ICommandHandler<SendAccountConfirmationSMS>
    {
        private readonly ISmsService _smsService;
        private readonly IConfigurationManager _configurationManager;
        private readonly Resources.Resources _resources;

        public SmsCommandHandler(ISmsService smsService, IConfigurationManager configurationManager)
        {
            _smsService = smsService;
            _configurationManager = configurationManager;
            var applicationKey = _configurationManager.GetSetting("TaxiHail.ApplicationKey");
            _resources = new Resources.Resources(applicationKey);
        }

        public void Handle(SendAccountConfirmationSMS command)
        {
            var template = _resources.Get("AccountConfirmationSmsBody", command.ClientLanguageCode);
            var message = string.Format(template, command.Code, _configurationManager.GetSetting("TaxiHail.ApplicationName"));
            _smsService.Send(command.PhoneNumber,message);
        }
    }
}