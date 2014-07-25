using System;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using ServiceStack.Text;
using Twilio;

namespace apcurium.MK.Booking.SMS.Impl
{
    public class TwilioService : ISmsService
    {
        private readonly ILogger _logger;
        private readonly IConfigurationManager _configurationManager;

        public TwilioService(ILogger logger, IConfigurationManager configurationManager)
        {
            _logger = logger;
            _configurationManager = configurationManager;
        }

        public void Send(string toNumber, string message)
        {
            var accountSid = _configurationManager.GetSetting("Client.SMSAccountSid");
            var authToken = _configurationManager.GetSetting("Client.SMSAuthToken");
            var fromNumber = _configurationManager.GetSetting("Client.SMSFromNumber");
            var twilio = new TwilioRestClient(accountSid, authToken);
            var response = twilio.SendSmsMessage("+" + fromNumber, "+" + toNumber, message, "");

            if (response.RestException != null)
            {
                _logger.LogMessage("Error sending sms  : " + response.RestException.ToJson());
                throw new InvalidOperationException(response.RestException.Message);
            }
        }
    }
}
