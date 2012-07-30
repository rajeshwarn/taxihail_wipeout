using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RestService("/account/confirm/{EmailAddress}/{ConfirmationToken}", "GET")]
    public class ConfirmAccountRequest
    {
        public string EmailAddress { get; set; }
        public string ConfirmationToken { get; set; }
    }
}
