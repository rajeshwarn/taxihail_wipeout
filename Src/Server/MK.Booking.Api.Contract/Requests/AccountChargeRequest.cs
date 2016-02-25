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
    [Route("/admin/accountscharge", "GET, POST, PUT")]
    [Route("/admin/accountscharge/{AccountNumber}", "GET,DELETE")]
    [Route("/admin/accountscharge/{AccountNumber}/{CustomerNumber}/{HideAnswers}", "GET")]
    public class AccountChargeRequest
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string AccountNumber { get; set; }

        public string CustomerNumber { get; set; }

        public bool HideAnswers { get; set; }

        public bool UseCardOnFileForPayment { get; set; }

        public virtual AccountChargeQuestion[] Questions { get; set; }
    }
}