#region

using System;
using System.Linq;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common.Configuration.Impl;
using Infrastructure.Messaging.Handling;
using MK.Common.Configuration;

#endregion

namespace apcurium.MK.Booking.EventHandlers
{
    public class AppSettingsGenerator :
        IEventHandler<AppSettingsAddedOrUpdated>,
        IEventHandler<AppSettingsDeleted>
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
                var defaultSettings = taxiHailSettings.GetType().GetProperties();

                foreach (var appSetting in @event.AppSettings)
                {
                    var setting = settings.FirstOrDefault(x => x.Key.Equals(appSetting.Key));
                    var keySubstring = appSetting.Key.Split('.').Last();

                    if (setting != null)
                    {
                        setting.Value = appSetting.Value;
                    }
                    else
                    {
                        // TODO: check if setting is different before saving it
                        var defaultSetting = defaultSettings.FirstOrDefault(x => x.Name == keySubstring);
                        var defaultSettingValue = defaultSetting.GetValue(taxiHailSettings, null);
                        string defaultSettingStringValue = defaultSettingValue == null ? string.Empty : defaultSettingValue.ToString();

                        // For boolean values, string comparison will ignore case
                        bool isValueBoolean;
                        bool.TryParse(defaultSettingStringValue, out isValueBoolean);

                        if (isValueBoolean
                            ? appSetting.Value.Equals(defaultSettingStringValue, StringComparison.InvariantCultureIgnoreCase)
                            : appSetting.Value == defaultSettingStringValue)
                        {
                            // Only save setting in DB if different than default value
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
    }
}