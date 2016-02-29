using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Contract.Resources.FlightStats;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Http.Extensions;
using apcurium.MK.Web.Security;

namespace apcurium.MK.Web.Controllers.Api
{
    public class FlightInformationController : BaseApiController
    {
        private readonly IServerSettings _serverSettings;
        private readonly HttpClient _client;

        // {carrier}/{flight}/{direction}/{year}/{month}/{day} 
        private const string StatusEndPoint = "status/{0}/{1}/{2}/{3}/{4}/{5}?appId={6}&appKey={7}&utc=false&airport={8}";

        public FlightInformationController(IServerSettings serverSettings)
        {
            _serverSettings = serverSettings;
            _client = new HttpClient()
            {
                BaseAddress = new Uri(_serverSettings.ServerData.FlightStats.ApiUrl)
            };
        }

        [HttpPost, Auth]
        [Route("flightInfo")]
        public object GetFlightInformation(FlightInformationRequest request)
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
                throw GetException(HttpStatusCode.NotFound, "No flight found.");
            }

            var error = flightStatsResponse.Error;

            if (error != null)
            {
                throw GetException(HttpStatusCode.BadRequest, "OrderAirportView_" + error.ErrorCode);
            }

            if (flightStatsResponse.FlightStatuses == null || flightStatsResponse.FlightStatuses.None())
            {
                throw GetException(HttpStatusCode.NotFound, "No flight found.");
            }

            var terminal = GetTerminal(flightStatsResponse.FlightStatuses, request.IsPickup, request.AirportId);

            if (!terminal.HasValue())
            {
                throw GetException(HttpStatusCode.NoContent, "No terminal found.");
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
