using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [Route("/account/creditcard/changelabel", "POST")]
    public class UpdateCreditCardLabelRequest
    {
        public Guid CreditCardId { get; set; }
        public string Label { get; set; }
    }
}
