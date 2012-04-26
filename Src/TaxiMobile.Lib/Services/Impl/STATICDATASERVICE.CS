using System;
using System.Linq;
using TaxiMobile.Lib.Data;

namespace TaxiMobile.Lib.Services.Impl
{
    public class StaticDataServiceImpl : BaseService<StaticDataservice>, IStaticDataService
    {
        protected override string GetUrl()
        {
            return base.GetUrl() + "IStaticData";
        }
        
        public ListItem[] GetCompaniesList()
        {
            ListItem[] listeVehicles = null;
            UseService(service =>
            {
                var companies = service.GetProviders(userNameApp, passwordApp);
                listeVehicles = companies.Select(x => new ListItem { Display = x.ProviderName, Id = x.ProviderNum }).ToArray();
            });
            return listeVehicles;
        }

        public ListItem[] GetVehiclesList()
        {
            ListItem[] listeVehicles = null;
            UseService(service =>
                           {
                               var companies = service.GetVehicleTypes(userNameApp, passwordApp, 0);
                               listeVehicles = companies.Select(x => new ListItem {Display = x.Name, Id = x.ID}).ToArray();
                           });
            return listeVehicles;
        }

        public ListItem[] GetPaymentsList()
        {
            ListItem[] listePaymentList = null;
            UseService(service =>
            {
                var items = service.GetChargeTypes(userNameApp, passwordApp, 0);
                listePaymentList = items.Select(x => new ListItem { Display = x.ChargeTypeName, Id = x.ChargeTypeID }).ToArray();
            });
            return listePaymentList;
        }
    }
}