using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Booking.Api.Contract.Resources;
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
	[AuthorizationRequired(ApplyTo.Get, RoleName.Support)]
	[Route("/account/orderswithuserid/{userid}", "GET")]
	public class AccountOrderListRequestWithUserIdRequest : IReturn<AccountOrderListRequestWithUserIdResponse>
	{
		public Guid UserId { get; set; }
	}

	[NoCache]
	public class AccountOrderListRequestWithUserIdResponse : List<Order>
	{
	}
}