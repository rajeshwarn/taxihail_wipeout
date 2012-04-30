using System;
using System.Linq;
using TaxiMobile.Lib.Data;

namespace TaxiMobile.Lib.Services.Impl
{
    public class StaticDataServiceImpl : BaseService<StaticDataservice>, IStaticDataService
    {
        private ListItem[] _listeCompanies;
        private ListItem[] _listeVehicles;
        private ListItem[] _listePaymentList;

        protected override string GetUrl()
        {
            return base.GetUrl() + "IStaticData";
        }

        public void Load()
        {
            GetCompaniesList();
            GetVehiclesList();
            GetPaymentsList();
        }

        public ListItem[] GetCompaniesList()
        {
            if(_listeCompanies == null)
            {
                UseService(service =>
                {
                    var companies = service.GetProviders(userNameApp, passwordApp);
                    _listeCompanies = companies.Select(x => new ListItem { Display = x.ProviderName, Id = x.ProviderNum }).ToArray();
                });
            }
            return _listeCompanies;
        }

        public ListItem[] GetVehiclesList()
        {
            if (_listeVehicles == null)
            {
                UseService(service =>
                               {
                                   var vehicules = service.GetVehicleTypes(userNameApp, passwordApp, 0);
                                   _listeVehicles = vehicules.Select(x => new ListItem { Display = x.Name, Id = x.ID }).ToArray();
                               });
            }
            return _listeVehicles;
        }

        public ListItem[] GetPaymentsList()
        {
            if (_listePaymentList == null)
            {
                UseService(service =>
                {
                    var items = service.GetChargeTypes(userNameApp, passwordApp, 0);
                    _listePaymentList = items.Select(x => new ListItem { Display = x.ChargeTypeName, Id = x.ChargeTypeID }).ToArray();
                });
            }
            return _listePaymentList;
        }
    }
}