using apcurium.MK.Booking.Api.Contract.Http;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
	[Authenticate]
	[Route("/account/ordernumbertoallowrating", "GET")]
	public class AccountOrderNumberToAllowRatingRequest:IReturn<int>
	{
	}
}