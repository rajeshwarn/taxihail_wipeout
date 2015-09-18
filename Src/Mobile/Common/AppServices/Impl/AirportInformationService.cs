using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class AirportInformationService : BaseService, IAirportInformationService
	{



		public async Task<string> GetTerminal(DateTime date, string flightNumber, string carrierCode, string airportId, bool isPickup)
		{
			var flightInformationRequest = new FlightInformationRequest
			{
				AirportId = airportId,
				CarrierCode = carrierCode,
				Date = date,
				FlightNumber = flightNumber,
				IsPickup = isPickup
			};

			var airportInfo = await UseServiceClientAsync<FlightInformationServiceClient, FlightInformation>(service => 
				service.GetTerminal(flightInformationRequest)
			);

			return airportInfo == null 
				? null 
				: airportInfo.Terminal;
		}
	}
}