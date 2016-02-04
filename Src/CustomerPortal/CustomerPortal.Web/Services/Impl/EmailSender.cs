using System.Net;
using System.Net.Mail;
using apcurium.MK.Common.Extensions;
using AutoMapper;
using CustomerPortal.Contract.Resources;

namespace CustomerPortal.Web.Services.Impl
{
    public class EmailSender : IEmailSender
    {
        private const string DeploymentMessageTemplate = "<html><head></head><body>Job started by {1} with server {2} for version {3}. <br/><br/> <b>Build log:</b> <br/><br/>{0}</body></html>";

        private const string DeploymentSubjectTemplate = "Deployment job failed for {0}.";

        private const string DeploymentToEmail = "taxihail@apcurium.freshdesk.com";


        private const string ServiceStatusMessageTemplate = @"
<html>
    <head></head>
    <body>
        The following service currently have issues. <br/>
        <br/>
        Company: {0}<br/>
        Url: {1}<br/>
        <br/>
        Report:<br/>
        {2}
    </body>
</html>";

        private const string ServiceStatusToEmail = "dominique.savoie@apcurium.com";
        private const string ServiceStatusSubjectTemplate = "There is a problem accessing {0}'s server";
        private const string ServiceStatusFromEmail = "taxihail@apcurium.com";

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
        
        public void SendDeploymentWarningEmail(string details, string tag, string company, string userName,string userEmail, string server)
        {
            var message = new MailMessage()
            {
                From = new MailAddress(userEmail, userName),
                To = { DeploymentToEmail },
                Subject = DeploymentSubjectTemplate.InvariantCultureFormat(company),
                Body = DeploymentMessageTemplate.InvariantCultureFormat(details, userName, server, tag),
                IsBodyHtml = true
            };

            using (var client = GetClient())
            {
                client.Send(message);
            }
        }

        public void SendServiceStatusEmail(string companyName, string url, ServiceStatus status, HttpStatusCode httpStatusCode)
        {
            var report = GenerateReport(status, httpStatusCode);

            var message = new MailMessage()
            {
                From = new MailAddress(ServiceStatusFromEmail),
                To = { ServiceStatusToEmail },
                Subject = ServiceStatusSubjectTemplate.InvariantCultureFormat(companyName),
                Body = ServiceStatusMessageTemplate.InvariantCultureFormat(companyName, url, report),
                IsBodyHtml = true
            };

            using (var client = GetClient())
            {
                client.Send(message);
            }
        }

        private string GenerateReport(ServiceStatus status, HttpStatusCode httpStatusCode)
        {
            if (httpStatusCode != HttpStatusCode.OK)
            {
                return "Connection to server resulted in a problem accessing the server <br/>" + httpStatusCode;
            }

            var report = "Server is up.<br/>";

            if (!status.IsCustomerPortalAvailable)
            {
                report += "Server cannot access Customer Portal. <br/>";
            }

            if (!status.IsIbsAvailable)
            {
                report += "Server cannot access IBS at {0}. <br/>".InvariantCultureFormat(status.IbsUrl);
            }

            if (!(status.IsGeoAvailable ?? true))
            {
                report += "Server cannot access CmtGeo at {0}. <br/>".InvariantCultureFormat(status.GeoUrl);
            }

            if (!(status.IsHoneyBadgerAvailable ?? true))
            {
                report += "Server cannot access Insight(HoneyBadger) at {0}. <br/>".InvariantCultureFormat(status.HoneyBadgerUrl);
            }

            if (!(status.IsMapiAvailable ?? true))
            {
                report += "Server cannot access CMT Mobile API (MAPI) at {0}. <br/>".InvariantCultureFormat(status.MapiUrl);
            }

            if (!(status.IsPapiAvailable ?? true))
            {
                report += "Server cannot access CMT Payment API (PAPI) at {0}. <br/>".InvariantCultureFormat(status.PapiUrl);
            }

            if (!status.IsSqlAvailable)
            {
                report += "Server cannot access database. <br/>";
            }

            if (status.IsUpdaterDeadlocked)
            {
                report += "Status updater is running the same cycle for at least 10 minutes. We will force the restart of status updater after 15 mins. <br/>";
            }

            return report;
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