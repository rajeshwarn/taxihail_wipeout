using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Geography;
using CMTServices.Enums;
using CMTServices.Responses;

namespace CMTServices
{
    public abstract class BaseAvailableVehicleServiceClient
    {
        protected BaseAvailableVehicleServiceClient(IServerSettings serverSettings,ILogger logger ,string serviceUrl)
        {
            Settings = serverSettings;
            Logger = logger;

            Client = new HttpClient
            {
                BaseAddress = new Uri(serviceUrl)
            };
        }

        protected IServerSettings Settings { get; private set; }

        protected ILogger Logger { get; private set; }

        protected HttpClient Client { get; private set; }


        public abstract IEnumerable<VehicleResponse> GetAvailableVehicles(string market, double latitude, double longitude, int? searchRadius = null, IList<int> fleetIds = null, bool returnAll = false, bool wheelchairAccessibleOnly = false);

		protected static string BuildQueryString(IEnumerable<KeyValuePair<string, string>> @params, string paramsFromSetting = null)
		{
			var requestPrefix = paramsFromSetting != null ? "&" : string.Empty;

			return "?" + paramsFromSetting + requestPrefix + string.Join("&", @params.Select(x => string.Join("=", x.Key, x.Value)));
		}

        protected IEnumerable<VehicleResponse> ToVehicleResponse(IEnumerable<BaseAvailableVehicleContent> entities)
        {
            return entities.Select(ToVehicleResponse);
        }

        protected VehicleResponse ToVehicleResponse(BaseAvailableVehicleContent entity)
        {
            return new VehicleResponse
            {
                Timestamp = entity.TimeStamp,
                Latitude = entity.Latitude,
                Longitude = entity.Longitude,
                Medallion = entity.Medallion,
                FleetId = entity.FleetId,
				VehicleType = entity.VehicleType,
            };
        }
    }
}
