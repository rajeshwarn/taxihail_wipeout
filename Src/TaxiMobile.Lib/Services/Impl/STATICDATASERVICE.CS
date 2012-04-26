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
            throw new System.NotImplementedException();
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
            throw new System.NotImplementedException();
        }
    }
}