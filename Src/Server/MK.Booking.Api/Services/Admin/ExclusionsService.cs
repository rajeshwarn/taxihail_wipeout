using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Messaging;
using ServiceStack.CacheAccess;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.Api.Services.Admin
{
    public class ExclusionsService: RestServiceBase<ExclusionsRequest>
    {
        private readonly IConfigurationManager _configManager;
        private readonly ICommandBus _commandBus;
        private readonly ICacheClient _cacheClient;

        public ExclusionsService(IConfigurationManager configManager, ICommandBus commandBus, ICacheClient cacheClient)
        {
            _configManager = configManager;
            _commandBus = commandBus;
            _cacheClient = cacheClient;
        }

        public override object OnGet(ExclusionsRequest request)
        {
            var excludedVehicleTypeId =   _configManager.GetSetting("IBS.ExcludedVehicleTypeId").SelectOrDefault( s=> s
                .Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse).ToArray(), new int[0]);
            var excludedPaymentTypeId = _configManager.GetSetting("IBS.ExcludedPaymentTypeId").SelectOrDefault( s=> s
                .Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse).ToArray(), new int[0]);
            var excludedProviderId = _configManager.GetSetting("IBS.ExcludedProviderId").SelectOrDefault( s=> s
                .Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse).ToArray(), new int[0]);

            return new ExclusionsRequestResponse()
            {
                ExcludedVehicleTypeId = excludedVehicleTypeId,
                ExcludedPaymentTypeId = excludedPaymentTypeId,
                ExcludedProviderId = excludedProviderId
            };

        }

        public override object OnPost(ExclusionsRequest request)
        {
            var settings = new Dictionary<string,string>()
            {
                {"IBS.ExcludedVehicleTypeId",   request.ExcludedVehicleTypeId == null   ? null :    string.Join(";",request.ExcludedVehicleTypeId.Select(x=>x.ToString()))  },
                {"IBS.ExcludedPaymentTypeId",   request.ExcludedPaymentTypeId == null   ? null :    string.Join(";",request.ExcludedPaymentTypeId.Select(x=>x.ToString()))  },
                {"IBS.ExcludedProviderId",      request.ExcludedProviderId == null      ? null :    string.Join(";",request.ExcludedProviderId.Select(x=>x.ToString()))  }
            };

            var command = new Commands.AddOrUpdateAppSettings { AppSettings = settings, CompanyId = AppConstants.CompanyId };
            _commandBus.Send(command);
            _cacheClient.Remove(ReferenceDataService.CacheKey);

            return null;
        }
    }
}
