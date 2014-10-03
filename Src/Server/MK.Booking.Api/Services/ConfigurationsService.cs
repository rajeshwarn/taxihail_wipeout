#region

using System.Collections.Generic;
using System.Linq;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
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
        private readonly IConfigurationDao _configDao;
        private readonly IConfigurationManager _configManager;

        public ConfigurationsService(IConfigurationManager configManager, ICommandBus commandBus, IConfigurationDao configDao)
        {
            _configManager = configManager;
            _commandBus = commandBus;
            _configDao = configDao;
        }

        public object Get(ConfigurationsRequest request)
        {
            var keys = new string[0];
            var result = new Dictionary<string, string>();

            var isFromWebApp = request.AppSettingsType == AppSettingsType.Webapp;
            var settings = _configManager.ServerData.GetType().GetAllProperties();
            var returnAllKeys = SessionAs<AuthUserSession>().HasPermission(RoleName.SuperAdmin);

            if (isFromWebApp)
            {
                var listKeys = _configManager.ServerData.Admin.CompanySettings;
                if (listKeys != null) keys = listKeys.Split(',');
            }
 
            foreach (var setting in settings)
            {
                bool sendToClient = false;
                var attributes = setting.Value.GetCustomAttributes(false);

                // Check if we have to return this setting to the client
                var sendToClientAttribute = attributes.OfType<SendToClientAttribute>().FirstOrDefault();
                if (sendToClientAttribute != null)
                {
                    sendToClient = !isFromWebApp || !sendToClientAttribute.ExcludeFromWebApp;
                }

                if (returnAllKeys || sendToClient || keys.Contains(setting.Key))
                {
                    var settingValue = _configManager.ServerData.GetNestedPropertyValue(setting.Key);

                    string settingStringValue = settingValue == null ? string.Empty : settingValue.ToString();
                    if (settingStringValue.IsBool())
                    {
                        // Needed because ToString() returns False instead of false
                        settingStringValue = settingStringValue.ToLower();
                    }

                    result.Add(setting.Key, settingStringValue);
                }
            }

            // Order results alphabetically
            return result.OrderBy(s => s.Key)
                         .ToDictionary(s => s.Key, s => s.Value);
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

            return string.Empty;
        }

        public object Get(NotificationSettingsRequest request)
        {
            if (request.AccountId.HasValue)
            {
                // if account notification settings have not been created yet, send back the default company values
                var accountSettings = _configDao.GetNotificationSettings(request.AccountId.Value);
                return accountSettings ?? _configDao.GetNotificationSettings();
            }

            return _configDao.GetNotificationSettings();
        }

        public object Post(NotificationSettingsRequest request)
        {
            if (!request.AccountId.HasValue)
            {
                if (!SessionAs<AuthUserSession>().HasPermission(RoleName.Admin))
                {
                    return new HttpError(HttpStatusCode.Unauthorized, "You do not have permission to modify company settings");
                }

                _commandBus.Send(new AddOrUpdateNotificationSettings
                {
                    CompanyId = AppConstants.CompanyId,
                    NotificationSettings = request.NotificationSettings
                });
            }
            else
            {
                _commandBus.Send(new AddOrUpdateNotificationSettings
                {
                    AccountId = request.AccountId.Value,
                    CompanyId = AppConstants.CompanyId,
                    NotificationSettings = request.NotificationSettings
                });
            }

            return new HttpResult(HttpStatusCode.OK, "OK");
        }
    }
}