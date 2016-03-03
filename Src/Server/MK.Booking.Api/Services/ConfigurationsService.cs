#region

using System;
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
using System.Reflection;
using System.Web;
using System.Web.Http;
using apcurium.MK.Booking.Api.Extensions;
using apcurium.MK.Common.Cryptography;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class ConfigurationsService : BaseApiService
    {
        private readonly ICommandBus _commandBus;
        private readonly IConfigurationDao _configDao;
        private readonly IServerSettings _serverSettings;

        public ConfigurationsService(IServerSettings serverSettings, ICommandBus commandBus, IConfigurationDao configDao)
        {
            _serverSettings = serverSettings;
            _commandBus = commandBus;
            _configDao = configDao;
        }

        public IDictionary<string, string> Get(ConfigurationsRequest request)
        {
			return GetConfigurationsRequestInternal(request.AppSettingsType, _serverSettings.ServerData.GetType().GetAllProperties());
        }

		public IDictionary<string, string> Get(EncryptedConfigurationsRequest request)
		{
			var data = GetConfigurationsRequestInternal(request.AppSettingsType, _serverSettings.ServerData.GetType().GetAllProperties());

			SettingsEncryptor.SwitchEncryptionStringsDictionary(_serverSettings.ServerData.GetType(), null, data, true);

			return data;
		}

		public IDictionary<string, string> GetConfigurationsRequestInternal(AppSettingsType appSettingsType, IDictionary<string, PropertyInfo> settings)
		{
			var result = new Dictionary<string, string>();

			var isFromAdminPortal = appSettingsType == AppSettingsType.Webapp;
			var returnAllKeys = Session.HasPermission(RoleName.SuperAdmin);
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


        public void Post(ConfigurationsRequest request)
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
        }

        public NotificationSettings Get(NotificationSettingsRequest request)
        {
            if (request.AccountId.HasValue)
            {
                // if account notification settings have not been created yet, send back the default company values
                var accountSettings = _configDao.GetNotificationSettings(request.AccountId.Value);
                return accountSettings ?? _configDao.GetNotificationSettings();
            }

            return _configDao.GetNotificationSettings();
        }

        public void Post(NotificationSettingsRequest request)
        {
            if (!request.AccountId.HasValue)
            {
                if (!Session.HasPermission(RoleName.Admin))
                {
                    throw new HttpException((int)HttpStatusCode.Unauthorized, "You do not have permission to modify company settings");
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
        }

        public UserTaxiHailNetworkSettings Get(UserTaxiHailNetworkSettingsRequest request)
        {
            if (request.AccountId == null)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, "Account Id cannot be null");
            }

            var networkSettings = _configDao.GetUserTaxiHailNetworkSettings(request.AccountId.Value) 
                ?? new UserTaxiHailNetworkSettings { IsEnabled = true, DisabledFleets = new string[]{} };

            return new UserTaxiHailNetworkSettings
            {
                Id = networkSettings.Id,
                IsEnabled = networkSettings.IsEnabled,
                DisabledFleets = networkSettings.DisabledFleets
            };
        }

        public void Post(UserTaxiHailNetworkSettingsRequest request)
        {
            if (request.AccountId == null)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, "Account Id cannot be null");
            }

            _commandBus.Send(new AddOrUpdateUserTaxiHailNetworkSettings
            {
                AccountId = request.AccountId.Value,
                IsEnabled = request.UserTaxiHailNetworkSettings.IsEnabled,
                DisabledFleets = request.UserTaxiHailNetworkSettings.DisabledFleets
            });
        }
    }
}