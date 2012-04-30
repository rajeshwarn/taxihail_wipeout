using TaxiMobile.Lib.Data;

namespace TaxiMobile.Lib.Services
{
    public interface IStaticDataService
    {
        void Load();
        ListItem[] GetCompaniesList();
        ListItem[] GetVehiclesList();
        ListItem[] GetPaymentsList();
    }
}