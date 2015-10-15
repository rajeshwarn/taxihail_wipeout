using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Security;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
	[Route("/account/findaccount/{accountid}", "GET")]
	[AuthorizationRequired(ApplyTo.Get, RoleName.Support)]
	public class FindAccountRequest
	{
		public Guid AccountId { get; set; }
	}
}