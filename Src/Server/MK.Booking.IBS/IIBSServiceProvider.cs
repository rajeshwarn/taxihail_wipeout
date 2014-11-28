using apcurium.MK.Booking.IBS.ChargeAccounts;

namespace apcurium.MK.Booking.IBS
{
    public interface IIBSServiceProvider
    {
        IAccountWebServiceClient Account(string companyKey = null);
        IStaticDataWebServiceClient StaticData(string companyKey = null);
        IBookingWebServiceClient Booking(string companyKey = null);
        IChargeAccountWebServiceClient ChargeAccount(string companyKey = null);
    }
}