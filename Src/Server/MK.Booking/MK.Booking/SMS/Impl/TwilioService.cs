using System;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using RestSharp.Extensions;
using ServiceStack.Text;
using Twilio;

namespace apcurium.MK.Booking.SMS.Impl
{
    public class TwilioService : ISmsService
    {
        private readonly ILogger _logger;
        private readonly IServerSettings _serverSettings;

        public TwilioService(ILogger logger, IServerSettings serverSettings)
        {
            _logger = logger;
            _serverSettings = serverSettings;
        }

        public void Send(libphonenumber.PhoneNumber toNumber, string message)
        {
            if (!toNumber.IsValidNumber)
            {
                _logger.LogMessage("Cannot send SMS for number {0}, phone number is {1})", toNumber.Format(libphonenumber.PhoneNumberUtil.PhoneNumberFormat.E164), toNumber.IsPossibleNumberWithReason.ToString());
                return;
            }

            var accountSid = _serverSettings.ServerData.SMSAccountSid;
            var authToken = _serverSettings.ServerData.SMSAuthToken;
            var fromNumber = _serverSettings.ServerData.SMSFromNumber;

            if (!accountSid.HasValue() || !authToken.HasValue() || !fromNumber.HasValue())
            {
                _logger.LogMessage("Cannot send SMS. Either one of those values are not set: SMSAccountSid, SMSAuthToken, SMSFromNumber");
                return;
            }

            var twilio = new TwilioRestClient(accountSid, authToken);

            var response = twilio.SendSmsMessage("+" + fromNumber, toNumber.Format(libphonenumber.PhoneNumberUtil.PhoneNumberFormat.E164), message, string.Empty);

            if (response.RestException != null)
            {
                _logger.LogMessage("Error sending sms  : " + response.RestException.ToJson());
                throw new InvalidOperationException(response.RestException.Message);
            }
        }
    }
}
