using System;
using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Entity;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
#if !CLIENT
    [AuthorizationRequired(ApplyTo.Post | ApplyTo.Put | ApplyTo.Delete, RoleName.Admin)]
#endif
    [Route("/admin/accountscharge", "GET, POST")]
    [Route("/admin/accountscharge/{Number}", "PUT, DELETE")]
    public class AccountChargeRequest
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Number { get; set; }

        public virtual AccountChargeQuestion[] Questions { get; set; }
    }
}