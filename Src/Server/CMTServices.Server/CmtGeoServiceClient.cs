using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Http.Extensions;
using CMTServices.Enums;
using CMTServices.Responses;
using ServiceStack.Common;
using ServiceStack.Text;

namespace CMTServices
{
    public class CmtGeoServiceClient : BaseAvailableVehicleServiceClient
    {
        public CmtGeoServiceClient(IServerSettings serverSettings, ILogger logger)
            : base(serverSettings, logger, serverSettings.ServerData.CmtGeo.ServiceUrl)
        {
            // create another client for the geo access
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", serverSettings.ServerData.CmtGeo.AppKey);
        }

        /// <summary>
        /// Method that returns the vehicles available for a market.
        /// </summary>
        /// <param name="market">The market to search for available vehicles.</param>
        /// <param name="latitude">Search origin latitude.</param>
        /// <param name="longitude">Search origin longitude</param>
        /// <param name="searchRadius">Search radius in meters (Optional)</param>
        /// <param name="fleetIds">The ids of the fleets to search. (Optional)</param>
        /// <param name="returnAll">True to return all the available vehicles; false will return a set number defined by the admin settings. (Optional)</param>
        /// <param name="wheelchairAccessibleOnly">True to return only wheelchair accessible vehicles, false will return all. (Optional)</param>
        /// <returns>The available vehicles.</returns>
        public override IEnumerable<VehicleResponse> GetAvailableVehicles(string market, double latitude, double longitude, int? searchRadius = null, IList<int> fleetIds = null, bool returnAll = false, bool wheelchairAccessibleOnly = false)
        {
            var @params = GetAvailableVehicleParams(market, latitude, longitude, searchRadius, fleetIds, returnAll, wheelchairAccessibleOnly);
            if (@params == null)
            {
                return new List<VehicleResponse>();
            }

			@params.AddRange(new []
			{
				new KeyValuePair<string, object>("availState", ((int)AvailabilityStates.Available).ToString()),
				new KeyValuePair<string, object>("includeETA", "true")
			});

            CmtGeoResponse response = null;
            try
            {
                response = Client.Post("/availability", ToDictionary(@params))
                    .Deserialize<CmtGeoResponse>()
                    .Result;
            }
            catch (Exception ex)
            {
                Logger.LogMessage("An error occured when trying to contact Geo service");
                Logger.LogError(ex);
            }
             
            if (response != null && response.Entities != null)
            {
                var numberOfVehicles = Settings.ServerData.AvailableVehicles.Count;
                // make sure that if ETA is null they are last in the list
                var orderedVehicleList = response.Entities
                    .OrderBy(v => (v.ETASeconds != null ? 0 : 1))
                    .ThenBy(v => v.ETASeconds)
                    .ThenBy(v => v.Medallion);

                var entities = !returnAll 
                    ? orderedVehicleList.Take(numberOfVehicles) 
                    : orderedVehicleList;

                return ToVehicleResponse(entities);
            }

            return new List<VehicleResponse>();
        }

	    public VehicleResponse GetPairedVehicle(string medallion, string market, double latitude, double longitude, int? searchRadius = null, IList<int> fleetIds = null, bool returnAll = false, bool wheelchairAccessibleOnly = false)
	    {
			var @params = GetAvailableVehicleParams(market, latitude, longitude,searchRadius,fleetIds,returnAll,wheelchairAccessibleOnly, hired: true);
			if (@params == null)
			{
				return null;
			}

			@params.Add(new KeyValuePair<string, object>("eHailSate", ((int)EHailStates.PairedWithRL).ToString()));
			@params.Add(new KeyValuePair<string, object>("medallions", new[]{medallion}));

			CmtGeoResponse response = null;
			try
			{
				response = Client.Post("/availability", ToDictionary(@params))
					.Deserialize<CmtGeoResponse>()
					.Result;
			}
			catch (Exception ex)
			{
				Logger.LogMessage("An error occured when trying to contact Geo service");
				Logger.LogError(ex);

				return null;
			}

		    if (response == null || response.Entities == null)
		    {
			    return null;
		    }

		    var entity = response.Entities.FirstOrDefault(item => item.Medallion == medallion);

			return ToVehicleResponse(entity);

	    }

        protected IEnumerable<VehicleResponse> ToVehicleResponse(IEnumerable<CmtGeoContent> entities)
        {
            return entities.Select(ToVehicleResponse);
        }

        protected VehicleResponse ToVehicleResponse(CmtGeoContent entity)
        {
            var response = base.ToVehicleResponse(entity);

            response.Eta = entity.ETASeconds;

            return response;
        }

        public VehicleResponse GetEta(double latitude, double longitude, string vehicleRegistration)
        {
            var @params = new []
            {
                new KeyValuePair<string, object>("lat", latitude),
                new KeyValuePair<string, object>("lon", longitude),
				new KeyValuePair<string, object>("includeMapMatch", "true"),
                new KeyValuePair<string, object>("deviceName", vehicleRegistration)
            };

            try
            {
                var response = Client.Post("/eta", @params.ToDictionary(kv => kv.Key, kv => kv.Value))
                    .Deserialize<CmtGeoContent>()
                    .Result;

                return ToVehicleResponse(response);
            }
            catch (Exception ex)
            {
                Logger.LogMessage("An error occured when trying to contact CMT Geo service");
                Logger.LogError(ex);

                return new VehicleResponse();
            }
        }

        private Dictionary<string,object> ToDictionary(IEnumerable<KeyValuePair<string,object>> data)
        {
            if (data == null)
            {
                return null;
            }

            return data
                .Where(kv =>
                {
                    var arrayValue = kv.Value as IEnumerable<string>;

                    return arrayValue != null 
                        ? arrayValue.Any() 
                        : (kv.Value as string).HasValue();
                })
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        protected List<KeyValuePair<string, object>> GetAvailableVehicleParams(
			string market, 
			double latitude, 
			double longitude, 
			int? searchRadius = null, 
			IList<int> fleetIds = null, 
			bool returnAll = false, 
			bool wheelchairAccessibleOnly = false,
			bool hired = false)
        {
            if (fleetIds != null && !fleetIds.Any())
            {
                // No fleetId allowed for available vehicles
                return null;
            }

	        var meterState = hired
		        ? MeterStates.Hired
		        : MeterStates.ForHire;

			

            var @params = new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("meterState", ((int)meterState).ToString()),
                    new KeyValuePair<string, object>("logonState", ((int)LogonStates.LoggedOn).ToString()),
                    new KeyValuePair<string, object>("lat", latitude.ToString(CultureInfo.InvariantCulture)),
                    new KeyValuePair<string, object>("lon", longitude.ToString(CultureInfo.InvariantCulture)),
                    new KeyValuePair<string, object>("rad", (searchRadius ?? Settings.ServerData.AvailableVehicles.Radius).ToString()),

                    // the following have different defaults from badger service
                    new KeyValuePair<string, object>("includeMapMatch", "true"),

                    // required for geo service
                    new KeyValuePair<string, object>("limit", "10")
                };

            if (market.HasValue())
            {
                @params.Add(new KeyValuePair<string, object>("markets", market.Split(',')));

            }

            if (wheelchairAccessibleOnly)
            {
                @params.Add(new KeyValuePair<string, object>("wavState", "1"));
            }

            if (fleetIds != null)
            {
	            var fleetIdsArray = fleetIds
					.Select(fleet => fleet.ToString())
					.ToArray();

				@params.Add(new KeyValuePair<string, object>("fleets", fleetIdsArray));
            }

            return @params;
        }
    }
}