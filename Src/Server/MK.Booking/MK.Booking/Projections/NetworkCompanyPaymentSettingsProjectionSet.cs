using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Booking.Projections
{
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

        public bool Exists(Func<ServerPaymentSettings, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public void Remove(string identifier)
        {
            throw new NotImplementedException();
        }

        public void Remove(Func<ServerPaymentSettings, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public IProjection<ServerPaymentSettings> GetProjection(string identifier)
        {
            throw new NotImplementedException();
        }

        public IProjection<ServerPaymentSettings> GetProjection(Func<ServerPaymentSettings, bool> predicate)
        {
            throw new NotImplementedException();
        }
    }
}