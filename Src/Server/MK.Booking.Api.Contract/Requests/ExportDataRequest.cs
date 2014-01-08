using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Security;
using System;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [AuthorizationRequired(ApplyTo.Get, RoleName.Admin)]
    [Route("/admin/export/{Target}", "POST")]
    public class ExportDataRequest
    {
        public DataType Target { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public enum DataType
    {
        Orders,
        Accounts
    }
}