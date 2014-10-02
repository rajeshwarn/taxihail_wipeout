#region

using System;
using System.Linq;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common.Configuration.Impl;
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

        public AppSettingsGenerator(Func<ConfigurationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Handle(AppSettingsAddedOrUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var settings = context.Query<AppSetting>().ToList();
                var taxiHailSettings = new TaxiHailSetting();
                var defaultSettings = taxiHailSettings.GetType().GetAllProperties();

                foreach (var appSetting in @event.AppSettings)
                {
                    var settingToUpdate = settings.FirstOrDefault(x => x.Key == appSetting.Key);

                    if (!defaultSettings.ContainsKey(appSetting.Key))
                    {
                        // Setting doesn't exist
                        continue;
                    }

                    var defaultSettingValue = taxiHailSettings.GetNestedPropertyValue(appSetting.Key);
                    string defaultSettingStringValue = defaultSettingValue == null ? string.Empty : defaultSettingValue.ToString();

                    // For boolean values, string comparison will ignore case
                    bool isValueBoolean;
                    bool.TryParse(defaultSettingStringValue, out isValueBoolean);

                    if (settingToUpdate != null)
                    {
                        if (AreSettingsEqual(appSetting.Value, defaultSettingStringValue))
                        {
                            // Value is it's different than default
                            settingToUpdate.Value = appSetting.Value;
                        }
                        else
                        {
                            // Value is the same as the default, remove the setting
                            context.Set<AppSetting>().Remove(settingToUpdate);
                        }
                    }
                    else
                    {
                        if (AreSettingsEqual(appSetting.Value, defaultSettingStringValue))
                        {
                            // New setting with value different than default
                            context.Set<AppSetting>().Add(new AppSetting(appSetting.Key, appSetting.Value));
                        }
                    }
                }

                context.SaveChanges();
            }
        }

        public void Handle(AppSettingsDeleted @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var settings = context.Query<AppSetting>().ToList();

                foreach (var appSetting in @event.AppSettings)
                {
                    var setting = settings.FirstOrDefault(x => x.Key.EndsWith(appSetting));
                    context.Set<AppSetting>().Remove(setting);
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
                        context.Set<AppSetting>().Add(new AppSetting
                        {
                            Key = appSetting.Key.Split(new []{"Client."}, StringSplitOptions.RemoveEmptyEntries).First(),
                            Value = appSetting.Value
                        });
                    }
                }

                context.SaveChanges();
            }
        }

        private bool AreSettingsEqual(string setting1, string setting2)
        {
            // For boolean values, string comparison will ignore case
            bool isValueBoolean;
            bool.TryParse(setting1, out isValueBoolean);

            return isValueBoolean
                ? setting1.Equals(setting2, StringComparison.InvariantCultureIgnoreCase)
                : setting1 == setting2;
        }
    }
}