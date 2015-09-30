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
	[Route("/account/findaccounts/{searchCriteria}", "GET")]
	[AuthorizationRequired(ApplyTo.Get, RoleName.Admin)]
	public class FindAccountsRequest
	{
		public string SearchCriteria { get; set; }
	}
}