#region

using System;
using System.Linq;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common.Configuration.Impl;
using Infrastructure.Messaging.Handling;

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

                foreach (var appSetting in @event.AppSettings)
                {
                    var setting = settings.FirstOrDefault(x => x.Key.Equals(appSetting.Key));

                    if (setting != null)
                    {
                        setting.Value = appSetting.Value;
                    }
                    else
                    {
                        context.Set<AppSetting>().Add(new AppSetting(appSetting.Key, appSetting.Value));
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