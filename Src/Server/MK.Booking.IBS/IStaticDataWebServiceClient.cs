using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.IBS
{
    public interface IStaticDataWebServiceClient
    {
        ListItem[] GetCompaniesList();
        ListItem[] GetVehiclesList(int compagnieId = 0);
        ListItem[] GetPaymentsList(int compagnieId = 0);
    }
}