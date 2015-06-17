using System;
using System.Net;
using System.Net.Mail;
using apcurium.MK.Common.Extensions;
using AutoMapper;

namespace CustomerPortal.Web.Services.Impl
{
    public class EmailSender : IEmailSender
    {
        private const string MessageTemplate = "<html><head></head><body>Job started by {1} with server {2} for version {3}. <br/><br/> <b>Build log:</b> <br/><br/>{0}</body></html>";

#if DEBUG
        private const string SubjectTemplate = "[TEST]Deployment job failed for {0}.";
#else
        private const string SubjectTemplate = "Deployment job failed for {0}.";
#endif
        private const string ToEmail = "taxihail@apcurium.freshdesk.com";
        private SmtpClient GetClient()
        {

            var config = new SmtpConfiguration
            {
                Port = 2525,
                Host = "smtpcorp.com",
                Username = "TaxiHail",
                Password = "Password01",
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            var client = new SmtpClient();
            Mapper.Map(config, client);

            return client;
        }


        public void SendEmail(string details, string tag, string company, string userName,string userEmail, string server)
        {
            var message = new MailMessage()
            {
                From = new MailAddress(userEmail, userName),
                To = { ToEmail },
                Subject = SubjectTemplate.InvariantCultureFormat(company),
                Body = MessageTemplate.InvariantCultureFormat(details, userName, server, tag),
                IsBodyHtml = true
            };

            using (var client = GetClient())
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