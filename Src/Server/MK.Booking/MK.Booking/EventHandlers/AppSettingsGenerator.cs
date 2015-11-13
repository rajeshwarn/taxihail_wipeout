#region

using System;
using System.Linq;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging.Handling;
using MK.Common.Configuration;
using System.Collections.Generic;
using System.Reactive;
using apcurium.MK.Booking.Projections;
using System.Data.Entity;

#endregion

namespace apcurium.MK.Booking.EventHandlers
{
    public class AppSettingsGenerator :
        IEventHandler<AppSettingsAddedOrUpdated>
    {
        private readonly AppSettingsProjection _projection;
        private readonly IServerSettings _serverSettings;

        public AppSettingsGenerator(AppSettingsProjection projection, IServerSettings serverSettings)
        {
            _serverSettings = serverSettings;
            _projection = projection;
        }

        public void Handle(AppSettingsAddedOrUpdated @event)
        {
            var taxiHailSettings = new ServerTaxiHailSetting();
            var defaultSettings = taxiHailSettings.GetType().GetAllProperties();
            var settings = _projection.Load();

            foreach (var appSetting in @event.AppSettings)
            {
                if (!defaultSettings.ContainsKey(appSetting.Key))
                {
                        // Setting doesn't exist
                        continue;
                }

                var defaultSettingValue = taxiHailSettings.GetNestedPropertyValue(appSetting.Key);
                string defaultSettingStringValue;

                if (defaultSettings[appSetting.Key].PropertyType == typeof(bool?))
                {
                        // Support for nullabool
                        defaultSettingStringValue = defaultSettingValue == null
                            ? "null"
                            : defaultSettingValue.ToString().ToLower();
                }
                else
                {
                    defaultSettingStringValue = defaultSettingValue == null ? string.Empty : defaultSettingValue.ToString();
                }

                if (defaultSettingStringValue.IsBool())
                {
                        // Needed because ToString() returns False instead of false
                        defaultSettingStringValue = defaultSettingStringValue.ToLower();
                }

                var settingToUpdate = settings.FirstOrDefault(x => x.Key == appSetting.Key);

                if (settings.ContainsKey(appSetting.Key))
                {
                    if (appSetting.Value != defaultSettingStringValue)
                    {
                            // Value is different than default
                            settings[appSetting.Key] = appSetting.Value;
                    }
                    else
                    {
                            // Value is the same as default, remove the setting
                            settings.Remove(appSetting.Key);
                    }
                }
                else if (appSetting.Value != defaultSettingStringValue)
                {
                        // New setting with value different than default
                        settings[appSetting.Key] = appSetting.Value;
                }
            }
            _projection.Save(settings);

            // Refresh the ServerData object
            _serverSettings.Reload();
        }
    }
    

    public abstract class AppSettingsProjection : IProjection<IDictionary<string, string>>
    {
        public abstract IDictionary<string, string> Load();

        public abstract void Save(IDictionary<string, string> projection);
    }

    public class AppSettingsEntityProjection : AppSettingsProjection
    {
        private readonly Func<DbContext> _contextFactory;
        public AppSettingsEntityProjection(Func<ConfigurationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public override IDictionary<string, string> Load()
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Set<AppSetting>().ToDictionary(x => x.Key, x => x.Value);
            }
        }

        public override void Save(IDictionary<string, string> projection)
        {
            using (var context = _contextFactory.Invoke())
            {
                context.Set<AppSetting>().RemoveRange(context.Set<AppSetting>());
                context.Set<AppSetting>().AddRange(projection.Select(x => new AppSetting { Key = x.Key, Value = x.Value }));
                context.SaveChanges();
            }
        }
    }

    public class AppSettingsMemoryProjection : AppSettingsProjection
    {
        IDictionary<string, string> _dictionnary = new Dictionary<string, string>();

        public override IDictionary<string, string> Load()
        {
            return new Dictionary<string, string>(_dictionnary);
        }

        public override void Save(IDictionary<string, string> projection)
        {
            _dictionnary = projection;
        }
    }
}