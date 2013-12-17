﻿using System.Net;
using Infrastructure.Messaging;
using ServiceStack.CacheAccess;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface.Auth;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using ServiceStack.ServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.Api.Services
{
    public class ConfigurationsService : Service
    {
        private readonly IConfigurationManager _configManager;
        private readonly ICommandBus _commandBus;
        
        public ConfigurationsService(IConfigurationManager configManager, ICommandBus commandBus)
        {
            _configManager = configManager;
            _commandBus = commandBus;
        }
        
        public object Get(ConfigurationsRequest request)
        {
            var keys = new string[0];
            bool returnAllKeys = SessionAs<AuthUserSession>().HasPermission(RoleName.SuperAdmin);
            
            if (request.AppSettingsType.Equals(AppSettingsType.Webapp))
            {
                var listKeys = _configManager.GetSetting("Admin.CompanySettings");
                if(listKeys != null) keys = listKeys.Split(',');
            }
            else //AppSettingsType.Mobile
            { 
                keys = new[] {"DefaultPhoneNumber", "DefaultPhoneNumberDisplay","GCM.SenderId", "PriceFormat", "DistanceFormat", "Direction.TarifMode", "Direction.NeedAValidTarif", "Direction.FlateRate", "Direction.RatePerKm", "Direction.MaxDistance", "NearbyPlacesService.DefaultRadius", "Map.PlacesApiKey", "AccountActivationDisabled" };
            }
            
            var allKeys = _configManager.GetSettings();

            var result = allKeys.Where(k => returnAllKeys 
                                            || keys.Contains(k.Key) 
                                            || k.Key.StartsWith("Client.") 
                                            || k.Key.StartsWith("GeoLoc."))
                                .ToDictionary(s => s.Key, s => s.Value);
            
            return result;
        }
        
        public object Post(ConfigurationsRequest request)
        {
            if (request.AppSettings.Any())
            {
                var command = new Commands.AddOrUpdateAppSettings { AppSettings = request.AppSettings,  CompanyId = AppConstants.CompanyId };
                _commandBus.Send(command);
            }
            
            return "";
        }
    }
}
