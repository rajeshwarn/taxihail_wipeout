﻿using System;
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
        IEventHandler<AppSettingsAddedOrUpdated>
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
    }
}