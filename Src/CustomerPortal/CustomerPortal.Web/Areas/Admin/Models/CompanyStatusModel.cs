using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using CustomerPortal.Contract.Resources;

namespace CustomerPortal.Web.Areas.Admin.Models
{
    public class CompanyStatusModel 
    {
        public ServiceStatus ServiceStatus { get; set; }

        public bool IsServerAvailable { get; set; }

        public bool IsApiAvailable { get; set; }

        public string CompanyName { get; set; }

        public string CompanyKey { get; set; }

        public bool HasAuthenticationError { get; set; }

        public ServiceStatusType ServiceState
        {
            get
            {
                if (!IsServerAvailable)
                {
                    return ServiceStatusType.NoConnection;
                }

                if (!IsApiAvailable)
                {
                    return ServiceStatusType.ApiUnavailable;
                }

                return ServiceStatus.SelectOrDefault(status => status.IsServerHealthy())
                    ? ServiceStatusType.Healthy 
                    : ServiceStatusType.IssuesFound;
            }
        }
    }
}