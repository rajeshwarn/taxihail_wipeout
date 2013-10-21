using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Email
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfigurationManager _configurationManager;
        private SmtpConfiguration _configuration;
        private string[] _debugEmails = new[] { "john@taxihail.com" };

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
                        Host = _configurationManager.GetSetting("Smtp.Host"),
                        Port = System.Convert.ToInt32(_configurationManager.GetSetting("Smtp.Port"), CultureInfo.InvariantCulture),
                        EnableSsl = System.Convert.ToBoolean(_configurationManager.GetSetting("Smtp.EnableSsl"), CultureInfo.InvariantCulture),
                        DeliveryMethod = (SmtpDeliveryMethod)Enum.Parse(typeof(SmtpDeliveryMethod), _configurationManager.GetSetting("Smtp.DeliveryMethod")),
                        UseDefaultCredentials = Convert.ToBoolean(_configurationManager.GetSetting("Smtp.UseDefaultCredentials"), CultureInfo.InvariantCulture),
                        Username = _configurationManager.GetSetting("Smtp.Credentials.Username"),
                        Password = _configurationManager.GetSetting("Smtp.Credentials.Password"),
                    };
                }

                Action sendAction = () =>
                {

                    var client = new System.Net.Mail.SmtpClient();
                    AutoMapper.Mapper.Map(_configuration, client);
                    using (client)
                    {                        
                        client.Send(message);
                    }
                };


                sendAction.Retry( new TimeSpan(0,0,0,15), 10 );

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
