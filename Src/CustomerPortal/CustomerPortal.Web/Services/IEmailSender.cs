using System.Net;
using CustomerPortal.Contract.Resources;

namespace CustomerPortal.Web.Services
{
    public interface IEmailSender
    {
        void SendDeploymentWarningEmail(string details, string tag, string company, string userName, string userEmail, string server);

        void SendServiceStatusEmail(string companyName, string url, ServiceStatus status, HttpStatusCode httpStatusCode);
    }
}
