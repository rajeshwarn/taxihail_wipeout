using System;
using apcurium.MK.Booking.Api.Contract.Requests.Payment.Braintree;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
	public interface IDropInViewService
	{
		Task<string> ShowDropInView(string clientToken);
	}
}

