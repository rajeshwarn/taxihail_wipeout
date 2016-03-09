using System;
using apcurium.MK.Common.Entity;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
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