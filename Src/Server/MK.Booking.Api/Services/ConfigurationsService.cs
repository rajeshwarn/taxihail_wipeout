#region

using System.Linq;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Enumeration;
using Infrastructure.Messaging;
using MK.Common.Configuration;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class ConfigurationsService : Service
    {
        private readonly ICommandBus _commandBus;
        private readonly IConfigurationManager _configManager;

        public ConfigurationsService(IConfigurationManager configManager, ICommandBus commandBus)
        {
            _configManager = configManager;
            _commandBus = commandBus;
        }

        public object Get(ConfigurationsRequest request)
        {
            var keys = new string[0];
            var returnAllKeys = SessionAs<AuthUserSession>().HasPermission(RoleName.SuperAdmin);

            if (request.AppSettingsType.Equals(AppSettingsType.Webapp))
            {
                var listKeys = _configManager.GetSetting("Admin.CompanySettings");
                if (listKeys != null) keys = listKeys.Split(',');
            }
            else //AppSettingsType.Mobile
            {
                keys = new[]
                {
                    "DefaultPhoneNumber", "DefaultPhoneNumberDisplay", "GCM.SenderId", "PriceFormat", "DistanceFormat",
                    "Direction.TarifMode", "Direction.NeedAValidTarif", "Direction.FlateRate", "Direction.RatePerKm",
                    "Direction.MaxDistance", "NearbyPlacesService.DefaultRadius", "Map.PlacesApiKey",
                    "AccountActivationDisabled"
                };
            }

            

            var allKeys = _configManager.GetSettings();

            var result = allKeys.Where(k => returnAllKeys
                                            || keys.Contains(k.Key)
                                            || k.Key.StartsWith("Client.")
                                            || k.Key.StartsWith("GeoLoc.")
                                            || k.Key.StartsWith("AvailableVehicles."))
                .ToDictionary(s => s.Key, s => s.Value);

            return result;
        }

        public object Post(ConfigurationsRequest request)
        {
            if (request.AppSettings.Any())
            {
                var command = new AddOrUpdateAppSettings
                {
                    AppSettings = request.AppSettings,
                    CompanyId = AppConstants.CompanyId
                };
                _commandBus.Send(command);
            }

            return "";
        }

        public object Get(NotificationSettingsRequest request)
        {
            if (!request.AccountId.HasValue)
            {
                return new NotificationSettings();
            }

            return new NotificationSettings();
        }

        public object Post(NotificationSettingsRequest request)
        {
            if (!request.AccountId.HasValue)
            {
                if (!SessionAs<AuthUserSession>().HasPermission(RoleName.Admin))
                {
                    return new HttpError(HttpStatusCode.Unauthorized, "");
                }

                return new HttpResult(HttpStatusCode.OK, "OK");
            }

            return new HttpResult(HttpStatusCode.OK, "OK");
        }
    }
}