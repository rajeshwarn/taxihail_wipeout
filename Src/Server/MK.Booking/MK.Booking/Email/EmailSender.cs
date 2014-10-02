#region

using System;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;
using AutoMapper;

#endregion

namespace apcurium.MK.Booking.Email
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfigurationManager _configurationManager;
        private readonly string[] _debugEmails = {"john@taxihail.com"};
        private SmtpConfiguration _configuration;

        public EmailSender(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
        }

        public void Send(MailMessage message)
        {
            if (message.To.Any(to => _debugEmails.Any(d => to.Address.ToLower() == d.ToLower())))
            {
                return;
            }

            Task.Factory.StartNew(() =>
            {
                if (_configuration == null)
                {
                    _configuration = new SmtpConfiguration
                    {
                        Host = _configurationManager.ServerData.Smtp.Host,
                        Port = _configurationManager.ServerData.Smtp.Port,
                        EnableSsl = _configurationManager.ServerData.Smtp.EnableSsl,
                        DeliveryMethod = _configurationManager.ServerData.Smtp.DeliveryMethod,
                        UseDefaultCredentials = _configurationManager.ServerData.Smtp.UseDefaultCredentials,
                        Username = _configurationManager.ServerData.Smtp.Credentials.Username,
                        Password = _configurationManager.ServerData.Smtp.Credentials.Password,
                    };
                }

                Action sendAction = () =>
                {
                    var client = new SmtpClient();
                    Mapper.Map(_configuration, client);
                    using (client)
                    {
                        client.Send(message);
                    }
                };

                sendAction.Retry(new TimeSpan(0, 0, 0, 15), 10);
            });
        }

        internal class SmtpConfiguration
        {
            public string Host { get; set; }
            public int Port { get; set; }
            public bool EnableSsl { get; set; }
            public SmtpDeliveryMethod DeliveryMethod { get; set; }
            public bool UseDefaultCredentials { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }
    }
}