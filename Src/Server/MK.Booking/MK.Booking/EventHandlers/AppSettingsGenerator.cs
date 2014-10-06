#region

using System;
using System.Linq;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging.Handling;
using MK.Common.Configuration;

#endregion

namespace apcurium.MK.Booking.EventHandlers
{
    public class AppSettingsGenerator :
        IEventHandler<AppSettingsAddedOrUpdated>,
        IEventHandler<AppSettingsDeleted>,
        IEventHandler<AppSettingNamesMigrated>
    {
        private readonly Func<ConfigurationDbContext> _contextFactory;
        private readonly IConfigurationManager _configManager;

        public AppSettingsGenerator(Func<ConfigurationDbContext> contextFactory, IConfigurationManager configManager)
        {
            _contextFactory = contextFactory;
            _configManager = configManager;
        }

        public void Handle(AppSettingsAddedOrUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var settings = context.Query<AppSetting>().ToList();
                var taxiHailSettings = new ServerTaxiHailSetting();
                var defaultSettings = taxiHailSettings.GetType().GetAllProperties();

                foreach (var appSetting in @event.AppSettings)
                {
                    if (!defaultSettings.ContainsKey(appSetting.Key))
                    {
                        // Setting doesn't exist
                        continue;
                    }

                    var defaultSettingValue = taxiHailSettings.GetNestedPropertyValue(appSetting.Key);
                    string defaultSettingStringValue = defaultSettingValue == null ? string.Empty : defaultSettingValue.ToString();
                    if (defaultSettingStringValue.IsBool())
                    {
                        // Needed because ToString() returns False instead of false
                        defaultSettingStringValue = defaultSettingStringValue.ToLower();
                    }

                    var settingToUpdate = settings.FirstOrDefault(x => x.Key == appSetting.Key);
                    if (settingToUpdate != null)
                    {
                        if (appSetting.Value != defaultSettingStringValue)
                        {
                            // Value is different than default
                            settingToUpdate.Value = appSetting.Value;
                        }
                        else
                        {
                            // Value is the same as default, remove the setting
                            context.Set<AppSetting>().Remove(settingToUpdate);
                        }
                    }
                    else
                    {
                        if (appSetting.Value != defaultSettingStringValue)
                        {
                            // New setting with value different than default
                            context.Set<AppSetting>().Add(new AppSetting(appSetting.Key, appSetting.Value));
                        }
                    }
                }

                context.SaveChanges();
            }

            // Refresh the ServerData object
            _configManager.Reload();
        }

        public void Handle(AppSettingsDeleted @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var settings = context.Query<AppSetting>().ToList();

                foreach (var appSetting in @event.AppSettings)
                {
                    var settingToDelete = settings.FirstOrDefault(x => x.Key == appSetting);
                    if (settingToDelete != null)
                    {
                        context.Set<AppSetting>().Remove(settingToDelete);
                    }  
                }

                context.SaveChanges();
            }
        }

        public void Handle(AppSettingNamesMigrated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var settings = context.Query<AppSetting>().ToList();

                foreach (var appSetting in settings)
                {
                    if (appSetting.Key.StartsWith("Client.", StringComparison.InvariantCultureIgnoreCase))
                    {
                        context.Set<AppSetting>().Remove(appSetting);

                        var newKey = appSetting.Key.Split(new[] {"Client."}, StringSplitOptions.RemoveEmptyEntries).First();
                        var existingEntry = context.Set<AppSetting>().FirstOrDefault(x => x.Key == newKey);
                        
                        if (existingEntry == null)
                        {
                            context.Set<AppSetting>().Add(new AppSetting
                            {
                                Key = newKey,
                                Value = appSetting.Value
                            });
                        }
                    }

                    if (appSetting.Key == "DistanceFormat"
                        && (appSetting.Value == "KM" || appSetting.Value == "km"))
                    {
                        appSetting.Value = DistanceFormat.Km.ToString();
                    }
                }

                context.SaveChanges();
            }

            // Refresh the ServerData object
            _configManager.Reload();
        }
    }
}