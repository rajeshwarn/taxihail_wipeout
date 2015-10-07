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
	[AuthorizationRequired(ApplyTo.Delete, new string[] { RoleName.Admin, RoleName.SuperAdmin, RoleName.Support } )]
	[Route("/admin/deleteAllCreditCards/{AccountID}", "DELETE")]
	public class DeleteCreditCardsWithAccountRequest
	{
		public Guid AccountID { get; set; }
	}
}