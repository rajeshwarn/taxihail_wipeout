using System;
using Infrastructure.Messaging;
using apcurium.MK.Common;

namespace apcurium.MK.Booking.Commands
{
	public class SendPasswordResetSMS : ICommand
	{
		public SendPasswordResetSMS()
		{
			Id = Guid.NewGuid();
		}		

		public string ClientLanguageCode { get; set; }

		public CountryISOCode CountryCode { get; set; }

		public string PhoneNumber { get; set; }

		public string Password { get; set; }

		public Guid Id { get; set; }

	}
}