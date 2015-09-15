using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using CMTPayment.Extensions;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
	public class FlightInformationServiceClient : BaseServiceClient
	{

		public Task<FlightInformation> GetTerminal(FlightInformationRequest request)
		{
			return Client.PostAsync(request);
		}
	}
}
