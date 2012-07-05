using System.Linq;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.IBS.Impl
{
    public class StaticDataWebServiceClient : BaseService<StaticDataservice>, IStaticDataWebServiceClient
    {
        protected override string GetUrl()
        {
            return base.GetUrl() + "IStaticData";
        }

        public StaticDataWebServiceClient(IConfigurationManager configManager, ILogger logger) : base(configManager, logger)
        {
        }

        public ListItem[] GetCompaniesList()
        {
            var items = new ListItem[] {};
            UseService(service =>
            {
                var companies = service.GetProviders(_userNameApp, _passwordApp);
                items= companies.Select(x => new ListItem { Display = x.ProviderName, Id = x.ProviderNum }).ToArray();
            });
            return items;
        }

        public ListItem[] GetVehiclesList(int compagnieId = 0)
        {
            var items = new ListItem[] { };
            UseService(service =>
            {
                var vehicules = service.GetVehicleTypes(_userNameApp, _passwordApp, compagnieId);
                items = vehicules.Select(x => new ListItem { Display = x.Name, Id = x.ID }).ToArray();
            });
            return items;
        }

        public ListItem[] GetPaymentsList(int compagnieId = 0)
        {
            var items = new ListItem[] { };
            UseService(service =>
            {
                var payments = service.GetChargeTypes(_userNameApp, _passwordApp, compagnieId);
                items = payments.Select(x => new ListItem { Display = x.ChargeTypeName, Id = x.ChargeTypeID }).ToArray();
            });
            return items;
        }
    }
}