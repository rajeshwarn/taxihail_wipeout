using System;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Api.Contract.Resources
{
	public class AccountPaymentQuestion : BaseDto
	{
		public string Question { get; set; }

		public string Answer { get; set; }

		public bool IsEnabled { get { return Question.HasValue(); } }
	}
}