using System;
using apcurium.MK.Booking.Api.Contract.Requests;
using TinyMessenger;

namespace apcurium.MK.Booking.Mobile
{
	public class AccountCreated : GenericTinyMessage<RegisterAccount>
	{
		public AccountCreated(object sender, RegisterAccount data)
			: base(sender, data)
		{

		}
	}
}

