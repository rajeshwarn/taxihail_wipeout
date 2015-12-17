using System;
using apcurium.MK.Booking.Api.Contract.Requests.Payment.Braintree;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
	public interface IBraintreeDropinViewService
	{
		Task<string> ShowDropinView(string clientToken);
	}
}

