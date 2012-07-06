using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RestService("/account/{AccountId}/resetpassword", "POST")]
    public class ResetPassword
    {
        public Guid AccountId { get; set; } 
    }
}
