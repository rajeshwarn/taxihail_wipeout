using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Booking.EventHandlers
{
    public class AppSettingsGenerator :
        IEventHandler<AppSettingsAdded>,
        IEventHandler<AppSettingsUpdated>
    {
        private readonly Func<ConfigurationDbContext> _contextFactory;
        private IConfigurationManager _configurationManager;

        public AppSettingsGenerator(IConfigurationManager configurationManager, Func<ConfigurationDbContext> contextFactory )
        {
            _configurationManager = configurationManager;
            _contextFactory = contextFactory;
        }

        public void Handle(AppSettingsAdded @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var setting = context.Set<AppSetting>().FirstOrDefault(x => x.Key.Equals(@event.Key));
               
                if (setting != null)
                {
                    setting.Value = @event.Value;
                    context.Set<AppSetting>().Attach(setting);
                }
                else
                {
                    context.Set<AppSetting>().Add(new AppSetting(@event.Key, @event.Value));
                }
                context.SaveChanges();

            }
        }

        public void Handle(AppSettingsUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                
                var setting = context.Query<AppSetting>().SingleOrDefault(c => c.Key.Equals(@event.Key));
                if (setting !=null)
                {
                    setting.Key = @event.Key;
                    setting.Value = @event.Value;
                    context.SaveChanges();
                }
            }
        }
    }
}