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

        public ListItem[] GetPickupCity(ListItem company)
        {
            var items = new ListItem[] { };
            UseService(service =>
            {
                var cities = service.GetPickupCityList(_userNameApp, _passwordApp, company.Id);
                items = cities.Select(x => new ListItem { Display = x.Name, Id = x.CityID, Parent = company }).ToArray();
            });
            return items;
        }
        public ListItem[] GetDropoffCity(ListItem company)
        {
            var items = new ListItem[] { };
            UseService(service =>
            {
                var cities = service.GetDropoffCityList(_userNameApp, _passwordApp, company.Id);
                items = cities.Select(x => new ListItem { Display = x.Name, Id = x.CityID, Parent = company }).ToArray();
            });
            return items;
        }



        public ListItem[] GetVehiclesList(ListItem company)
        {
            var items = new ListItem[] { };
            UseService(service =>
            {
                var vehicules = service.GetVehicleTypes(_userNameApp, _passwordApp, company.Id);
                items = vehicules.Select(x => new ListItem { Display = x.Name, Id = x.ID, Parent = company }).ToArray();
            });
            return items;
        }

        public ListItem[] GetPaymentsList(ListItem company)
        {
            var items = new ListItem[] { };
            UseService(service =>
            {
                var payments = service.GetChargeTypes(_userNameApp, _passwordApp, company.Id);
                items = payments.Select(x => new ListItem { Display = x.ChargeTypeName, Id = x.ChargeTypeID, Parent = company }).ToArray();
            });
            return items;
        }
    }
}