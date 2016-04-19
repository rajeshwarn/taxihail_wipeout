using System;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Http;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/admin/accountscharge", "GET, POST, PUT")]
    [RouteDescription("/admin/accountscharge/{AccountNumber}", "GET,DELETE")]
    [RouteDescription("/admin/accountscharge/{AccountNumber}/{CustomerNumber}/{HideAnswers}", "GET")]
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