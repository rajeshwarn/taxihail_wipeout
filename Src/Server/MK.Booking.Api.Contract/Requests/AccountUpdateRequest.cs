using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
	[AuthorizationRequired(ApplyTo.Put, RoleName.Support)]
	[Route("/account/update", "PUT")]
	public class AccountUpdateRequest
	{
		public Guid AccountID { get; set; }

		public BookingSettingsRequest BookingSettingsRequest { get; set; }
	}
}