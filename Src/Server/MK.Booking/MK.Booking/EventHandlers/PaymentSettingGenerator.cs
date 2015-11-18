#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging.Handling;
using System.Data.Entity;
using System.Linq;
using apcurium.MK.Booking.Projections;

#endregion

namespace apcurium.MK.Booking.EventHandlers
{
    public class PaymentSettingGenerator
        : IEventHandler<PaymentSettingUpdated>
    {
        private readonly Func<ConfigurationDbContext> _contextFactory;
        private readonly IProjectionSet<ServerPaymentSettings, string> _networkCompanyPaymentSettingsProjections;
        private readonly IProjection<ServerPaymentSettings> _companyPaymentSettingsProjection;

        public PaymentSettingGenerator(IProjectionSet<ServerPaymentSettings, string> networkCompanyPaymentSettingsProjections, IProjection<ServerPaymentSettings> companyPaymentSettingsProjection)
        {
            _networkCompanyPaymentSettingsProjections = networkCompanyPaymentSettingsProjections;
            _companyPaymentSettingsProjection = companyPaymentSettingsProjection;
        }

        public void Handle(PaymentSettingUpdated @event)
        {
            // migration for old events, set the default value
            if (!@event.ServerPaymentSettings.PreAuthAmount.HasValue)
            {
                @event.ServerPaymentSettings.PreAuthAmount = new ServerPaymentSettings().PreAuthAmount;
            }

            if (@event.ServerPaymentSettings.CompanyKey.HasValue())
            {
                _networkCompanyPaymentSettingsProjections.AddOrReplace(@event.ServerPaymentSettings);
            }
            else
            {
                _companyPaymentSettingsProjection.Save(@event.ServerPaymentSettings);
            }

        }
    }

    public class NetworkCompanyPaymentSettingsEntityProjections : IProjectionSet<ServerPaymentSettings, string>
    {
        Func<DbContext> _contextFactory;
        public NetworkCompanyPaymentSettingsEntityProjections(Func<ConfigurationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Add(ServerPaymentSettings projection)
        {
            throw new NotImplementedException();
        }

        public void AddRange(IEnumerable<ServerPaymentSettings> projections)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<ServerPaymentSettings> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void AddOrReplace(ServerPaymentSettings projection)
        {
            using (var context = _contextFactory.Invoke())
            {
                context
                    .Set<ServerPaymentSettings>()
                    .RemoveRange(context.Set<ServerPaymentSettings>()
                        .Where(x => x.CompanyKey == projection.CompanyKey));

                context.Set<ServerPaymentSettings>().Add(projection);
                context.SaveChanges();
            }
        }

        public void Update(Func<ServerPaymentSettings, bool> predicate, Action<ServerPaymentSettings> action)
        {
            throw new NotImplementedException();
        }

        public void Update(string identifier, Action<ServerPaymentSettings> action)
        {
            throw new NotImplementedException();
        }

        public bool Exists(string sourceId)
        {
            throw new NotImplementedException();
        }

        public IProjection<ServerPaymentSettings> GetProjection(string identifier)
        {
            throw new NotImplementedException();
        }
    }
}