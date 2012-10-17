using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Api.Contract.Requests;

namespace apcurium.MK.Booking.Api.Client
{
    public class AdministrationServiceClient : BaseServiceClient
    {
        public AdministrationServiceClient(string url, string sessionId) : base(url, sessionId)
        {
        }

        public void GrantAdminAccess(GrantAdminRightRequest request)
        {
            var req = string.Format("/account/grantadmin");
            Client.Put<string>(req, request);
        }
    }
}
