﻿using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Security;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [AuthorizationRequired(ApplyTo.Get, RoleName.Admin)]
    [RestService("/admin/export/{Target}", "GET")]
    public class ExportDataRequest
    {
        public DataType Target { get; set; }
    }

    public enum DataType
    {
        Orders,
        Accounts
    }
}