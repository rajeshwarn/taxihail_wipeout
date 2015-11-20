using System;
using System.Net;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using MK.Common.Exceptions;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class AirportInformationService : BaseService, IAirportInformationService
	{
		private readonly ILocalization _localizeService;
		private readonly IMessageService _messageService;

		private const string NotAvailable = "N/A";

		public AirportInformationService(ILocalization localizeService, IMessageService messageService)
		{
			_localizeService = localizeService;
			_messageService = messageService;
		}


		public async Task<string> GetTerminal(DateTime date, string flightNumber, string carrierCode, string carrierName, string airportId, bool isPickup)
		{

			var sanitizedCarrierCode = carrierCode.Contains(".")
				? carrierCode.Split('.')[1]
				: carrierCode;

			var flightInformationRequest = new FlightInformationRequest
			{
				AirportId = airportId,
				CarrierCode = sanitizedCarrierCode.ToLowerInvariant(),
				Date = date,
				FlightNumber = flightNumber,
				IsPickup = isPickup
			};

			try
			{
				var airportInfo = await UseServiceClientAsync<FlightInformationServiceClient, FlightInformation>(service =>
					service.GetTerminal(flightInformationRequest)
				);

				return airportInfo == null
					? null
					: airportInfo.Terminal;
			}
			catch (WebServiceException ex)
			{
				var showMessage = false;

				if (ex.StatusCode == (int)HttpStatusCode.NoContent)
				{
					return NotAvailable;
				}

				if (ex.StatusCode == (int)HttpStatusCode.BadRequest)
				{
					_messageService
						.ShowMessage(_localizeService["Error"], string.Format(_localizeService[ex.ErrorCode], carrierName, flightNumber, date.ToString("g")))
						.FireAndForget();

					return string.Empty;
				}

				if (ex.StatusCode == (int)HttpStatusCode.NotFound)
				{
					showMessage = true;
				}

				Logger.LogMessage("An error has occurred while attempting to get the airport terminal.");
				Logger.LogError(ex);

				if (!showMessage)
				{
					return string.Empty;
				}
			}

			var cancel = false;

			await _messageService.ShowMessage(
						_localizeService["BookingAirportNoFlights_Title"],
						string.Format(_localizeService["BookingAirportNoFlights_Message"], carrierName, flightNumber, date.ToString("g")),
						_localizeService["YesButton"],
						() => { },
						_localizeService["NoButton"],
						() => cancel = true);


			return cancel
				? string.Empty
				: NotAvailable;
			
		}
	}
}