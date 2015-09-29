
namespace apcurium.MK.Booking.Api.Contract.Resources.FlightStats
{
	public class FlightStatus
	{
		public AirportResource AirportResources { get; set; }
		public string ArrivalAirportFsCode { get; set; }
		public string CarrierFsCode { get; set; }
		public string DepartureAirportFsCode { get; set; }
		public string FlightId { get; set; }
		public string FlightNumber { get; set; }
	}
}
