using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Contract.Resources.FlightStats;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;
using CustomerPortal.Client.Http.Extensions;

namespace apcurium.MK.Booking.Api.Services
{
	public class FlightInformationService : BaseApiService
	{
		private readonly IServerSettings _serverSettings;
		private readonly HttpClient _client;

		// {carrier}/{flight}/{direction}/{year}/{month}/{day} 
		private const string StatusEndPoint = "status/{0}/{1}/{2}/{3}/{4}/{5}?appId={6}&appKey={7}&utc=false&airport={8}";


		public FlightInformationService(IServerSettings serverSettings)
		{
			_serverSettings = serverSettings;
			_client = new HttpClient()
			{
				BaseAddress = new Uri(_serverSettings.ServerData.FlightStats.ApiUrl)
			};
		}

		public object Post(FlightInformationRequest request)
		{
			var direction = request.IsPickup
				? "arr"
				: "dep";

			var endpoint = string.Format(
				StatusEndPoint, 
				request.CarrierCode, 
				request.FlightNumber, 
				direction, 
				request.Date.Year,
				request.Date.Month, 
				request.Date.Day,
				_serverSettings.ServerData.FlightStats.AppId,
				_serverSettings.ServerData.FlightStats.ApplicationKeys,
				request.AirportId
			);

			var flightStatsResponse = _client.GetAsync(endpoint)
				.Deserialize<FlightStatsResponse>()
				.Result;

			if (flightStatsResponse == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "No flight found.");
			}

			var error = flightStatsResponse.Error;

			if (error != null)
			{
				throw new HttpException((int)HttpStatusCode.BadRequest, "OrderAirportView_" + error.ErrorCode);
			}

			if (flightStatsResponse.FlightStatuses == null ||  flightStatsResponse.FlightStatuses.None())
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "No flight found.");
			}

			var terminal = GetTerminal(flightStatsResponse.FlightStatuses, request.IsPickup, request.AirportId);

			if (!terminal.HasValue())
			{
				throw new HttpException((int)HttpStatusCode.NoContent,"No terminal found.");
			}

			return new FlightInformation
			{
				Terminal = terminal
			};
		}

		private string GetTerminal(IEnumerable<FlightStatus> flightStatuses, bool isPickup, string airportId)
		{
			if (isPickup)
			{
				return flightStatuses
					.Where(fs => fs.ArrivalAirportFsCode.SoftEqual(airportId))
					.Select(fs => fs.AirportResources.ArrivalTerminal)
					.FirstOrDefault();
			}

			return flightStatuses
				.Where(fs => fs.DepartureAirportFsCode.SoftEqual(airportId))
				.Select(fs => fs.AirportResources.DepartureTerminal)
				.FirstOrDefault();
		}
	}
}
