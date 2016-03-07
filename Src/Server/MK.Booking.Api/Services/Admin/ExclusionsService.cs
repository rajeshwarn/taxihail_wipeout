#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Common;
using apcurium.MK.Common.Caching;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Api.Services.Admin
{
    public class ExclusionsService : BaseApiService
    {
        private readonly ICacheClient _cacheClient;
        private readonly ICommandBus _commandBus;
        private readonly IServerSettings _serverSettings;

        public ExclusionsService(IServerSettings serverSettings, ICommandBus commandBus, ICacheClient cacheClient)
        {
            _serverSettings = serverSettings;
            _commandBus = commandBus;
            _cacheClient = cacheClient;
        }

        public ExclusionsRequestResponse Get()
        {
            var excludedVehicleTypeId = _serverSettings.ServerData.IBS.ExcludedVehicleTypeId.SelectOrDefault(s => s
                .Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse).ToArray(), new int[0]);
            var excludedProviderId = _serverSettings.ServerData.IBS.ExcludedProviderId.SelectOrDefault(s => s
                .Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse).ToArray(), new int[0]);

            return new ExclusionsRequestResponse
            {
                ExcludedVehicleTypeId = excludedVehicleTypeId,
                ExcludedProviderId = excludedProviderId
            };
        }

        public void Post(ExclusionsRequest request)
        {
            var settings = new Dictionary<string, string>
            {
                {
                    "IBS.ExcludedVehicleTypeId",
                    request.ExcludedVehicleTypeId == null
                        ? null
                        : string.Join(";", request.ExcludedVehicleTypeId.Select(x => x.ToString(CultureInfo.InvariantCulture)))
                },
                {
                    "IBS.ExcludedProviderId",
                    request.ExcludedProviderId == null
                        ? null
                        : string.Join(";", request.ExcludedProviderId.Select(x => x.ToString(CultureInfo.InvariantCulture)))
                }
            };

            var command = new AddOrUpdateAppSettings {AppSettings = settings, CompanyId = AppConstants.CompanyId};
            _commandBus.Send(command);
            _cacheClient.RemoveByPattern(string.Format("{0}*", ReferenceDataService.CACHE_KEY));
        }
    }
}