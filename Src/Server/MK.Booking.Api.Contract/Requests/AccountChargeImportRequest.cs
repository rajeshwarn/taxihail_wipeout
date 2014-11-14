using System;
using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Common.Entity;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
#if !CLIENT
    [AuthorizationRequired(ApplyTo.Post)]
#endif
    [Route("/admin/accountscharge/import", "POST")]
    public class AccountChargeImportRequest
    {
        public AccountCharge[] AccountCharges { get; set; }
    }

    public class AccountChargeImportResponse : AccountChargeImportRequest
    {
    }
}