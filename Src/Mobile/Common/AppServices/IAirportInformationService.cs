using System;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IAirportInformationService
	{
		Task<string> GetTerminal(DateTime date, string flightNumber, string carrierCode, string airportId, bool isPickup);
	}
}