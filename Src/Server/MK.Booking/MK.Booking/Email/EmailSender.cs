using System;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.Email
{
    public class EmailSender : IEmailSender
    {
        private readonly SmtpConfiguration _configuration;

        public EmailSender(IConfigurationManager configurationManager)
        {
            _configuration = new SmtpConfiguration
            {
                Host = configurationManager.GetSetting("Smtp.Host"),
                Port = System.Convert.ToInt32(configurationManager.GetSetting("Smtp.Port"), CultureInfo.InvariantCulture),
                EnableSsl = System.Convert.ToBoolean(configurationManager.GetSetting("Smtp.EnableSsl"), CultureInfo.InvariantCulture),
                DeliveryMethod = (SmtpDeliveryMethod)Enum.Parse(typeof(SmtpDeliveryMethod), configurationManager.GetSetting("Smtp.DeliveryMethod")),
                UseDefaultCredentials = Convert.ToBoolean(configurationManager.GetSetting("Smtp.UseDefaultCredentials"), CultureInfo.InvariantCulture),
                Username = configurationManager.GetSetting("Smtp.Credentials.Username"),
                Password = configurationManager.GetSetting("Smtp.Credentials.Password"),
            };
        }

        public void Send(MailMessage message)
        {
            var client = new System.Net.Mail.SmtpClient();
            AutoMapper.Mapper.Map(_configuration, client);

            using(client)
            {
                client.Send(message);
            }
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
