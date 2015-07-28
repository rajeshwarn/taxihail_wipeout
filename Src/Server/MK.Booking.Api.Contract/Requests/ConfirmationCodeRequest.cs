using ServiceStack.ServiceHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/account/getconfirmationcode/{Email}", "GET")]
    public class ConfirmationCodeRequest
    {
        public string Email { get; set; }
    }
}