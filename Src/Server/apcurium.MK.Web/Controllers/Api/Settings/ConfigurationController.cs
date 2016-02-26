using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Extensions;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common;
using apcurium.MK.Common.Caching;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Cryptography;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Web.Security;
using Infrastructure.Messaging;
using MK.Common.Configuration;

namespace apcurium.MK.Web.Controllers.Api.Settings
{
    public class ConfigurationController : BaseApiController
    {
        private readonly ICacheClient _cacheClient;
        private readonly IServerSettings _serverSettings;
        private readonly ICommandBus _commandBus;
        private readonly IConfigurationDao _configDao;

        public ConfigurationController(ICacheClient cacheClient, IServerSettings serverSettings, ICommandBus commandBus, IConfigurationDao configDao)
        {
            _cacheClient = cacheClient;
            _serverSettings = serverSettings;
            _commandBus = commandBus;
            _configDao = configDao;
        }

        [HttpGet, Route("settings/reset")]
        public bool ResetConfiguration()
        {
            _cacheClient.RemoveByPattern(string.Format("{0}*", ReferenceDataService.CacheKey));
            _serverSettings.Reload();
            return true;
        }

        [HttpGet, Route("settings")]
        public Dictionary<string, string> GetAppSettings(ConfigurationsRequest request)
        {
            return GetConfigurationsRequestInternal(request.AppSettingsType, _serverSettings.ServerData.GetType().GetAllProperties());
        }

        [HttpGet, Route("encryptedsettings")]
        public Dictionary<string, string> GetEncryptedSettings(EncryptedConfigurationsRequest request)
        {
            var data = GetConfigurationsRequestInternal(request.AppSettingsType, _serverSettings.ServerData.GetType().GetAllProperties());

            SettingsEncryptor.SwitchEncryptionStringsDictionary(_serverSettings.ServerData.GetType(), null, data, true);

            return data;
        }

        [HttpPost, Auth(Roles = new []{ Roles.Admin }), Route("settings")]
        public HttpResponseMessage UpdateSettings(ConfigurationsRequest request)
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

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpGet, Auth, Route("settings/notifications/{accountId:Guid?}")]
        public NotificationSettings GetNotificationSettings(Guid? accountId)
        {
            if (accountId.HasValue)
            {
                // if account notification settings have not been created yet, send back the default company values
                var accountSettings = _configDao.GetNotificationSettings(accountId);
                return accountSettings ?? _configDao.GetNotificationSettings();
            }

            return _configDao.GetNotificationSettings();
        }

        [HttpPost, Auth, Route("settings/notifications/{accountId:Guid?}")]
        public HttpResponseMessage UpdateNotificationSettings(Guid? accountId, NotificationSettingsRequest request)
        {
            if (accountId.HasValue)
            {
                _commandBus.Send(new AddOrUpdateNotificationSettings
                {
                    AccountId = request.AccountId,
                    CompanyId = AppConstants.CompanyId,
                    NotificationSettings = request.NotificationSettings
                });
            }
            else
            {
                if (!GetSession().HasPermission(RoleName.Admin))
                {
                    throw new HttpException((int)HttpStatusCode.Unauthorized, "You do not have permission to modify company settings");
                }

                _commandBus.Send(new AddOrUpdateNotificationSettings
                {
                    CompanyId = AppConstants.CompanyId,
                    NotificationSettings = request.NotificationSettings
                });
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpGet, Auth, Route("settings/taxihailnetwork/{accountId:Guid?}")]
        public UserTaxiHailNetworkSettings GetUserTaxiHailNetworkSettings(Guid? accountId, UserTaxiHailNetworkSettingsRequest request)
        {
            var userId = accountId ?? request.AccountId ?? GetSession().UserId;

            var networkSettings = _configDao.GetUserTaxiHailNetworkSettings(id)
                ?? new UserTaxiHailNetworkSettings { IsEnabled = true, DisabledFleets = new string[] { } };

            return new UserTaxiHailNetworkSettings
            {
                Id = networkSettings.Id,
                IsEnabled = networkSettings.IsEnabled,
                DisabledFleets = networkSettings.DisabledFleets
            };
        }

        [HttpPost, Auth, Route("settings/taxihailnetwork/{accountId:Guid?}")]
        public HttpResponseMessage UpdateUserTaxiHailNetworkSettings(Guid? accountId, UserTaxiHailNetworkSettingsRequest request)
        {
            var userId = accountId ?? request.AccountId ?? GetSession().UserId;

            _commandBus.Send(new AddOrUpdateUserTaxiHailNetworkSettings
            {
                AccountId = userId,
                IsEnabled = request.UserTaxiHailNetworkSettings.IsEnabled,
                DisabledFleets = request.UserTaxiHailNetworkSettings.DisabledFleets
            });

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        public Dictionary<string, string> GetConfigurationsRequestInternal(AppSettingsType appSettingsType, IDictionary<string, PropertyInfo> settings)
        {
            var result = new Dictionary<string, string>();

            var isFromAdminPortal = appSettingsType == AppSettingsType.Webapp;
            var returnAllKeys = GetSession().HasPermission(RoleName.SuperAdmin);
            var isTaxiHailPro = _serverSettings.ServerData.IsTaxiHailPro;

            foreach (var setting in settings)
            {
                var sendToClient = false;
                var customizableByCompany = false;
                var attributes = setting.Value.GetCustomAttributes(false);

                // Check if we have to return this setting to the mobile client
                var sendToClientAttribute = attributes.OfType<SendToClientAttribute>().FirstOrDefault();
                if (sendToClientAttribute != null)
                {
                    sendToClient = !isFromAdminPortal;
                }

                // Check if we have to return this setting to the company settings of admin section
                var customizableByCompanyAttribute = attributes.OfType<CustomizableByCompanyAttribute>().FirstOrDefault();
                if (customizableByCompanyAttribute != null)
                {
                    if (isTaxiHailPro)
                    {
                        // company is taxihail pro, no need to check for taxihail pro attribute on setting, we know we return it
                        customizableByCompany = isFromAdminPortal;
                    }
                    else
                    {
                        var requiresTaxiHailProAttribute = attributes.OfType<RequiresTaxiHailPro>().FirstOrDefault();
                        if (requiresTaxiHailProAttribute == null)
                        {
                            customizableByCompany = isFromAdminPortal;
                        }
                    }
                }

                if (returnAllKeys                       // in the case of superadmin
                    || sendToClient                     // send to mobile client
                    || customizableByCompany)           // company settings in admin section
                {
                    var settingValue = _serverSettings.ServerData.GetNestedPropertyValue(setting.Key);

                    var settingStringValue = settingValue == null ? string.Empty : settingValue.ToString();
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



    }
}
