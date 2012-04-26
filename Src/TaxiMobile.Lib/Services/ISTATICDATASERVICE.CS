using TaxiMobile.Lib.Data;

namespace TaxiMobile.Lib.Services
{
    public interface IStaticDataService
    {
        ListItem[] GetCompaniesList();
        ListItem[] GetVehiclesList();
        ListItem[] GetPaymentsList();
    }
}