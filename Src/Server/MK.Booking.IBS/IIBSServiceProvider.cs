using apcurium.MK.Common.Configuration;
using apcurium.MK.Booking.IBS.ChargeAccounts;

namespace apcurium.MK.Booking.IBS
{
    public interface IIBSServiceProvider
    {
        IAccountWebServiceClient Account(string companyKey = null, string market = null);
        IStaticDataWebServiceClient StaticData(string companyKey = null, string market = null);
        IBookingWebServiceClient Booking(string companyKey = null, string market = null);
        IBSSettingContainer GetSettingContainer(string companyKey = null, string market = null);
        IChargeAccountWebServiceClient ChargeAccount(string companyKey = null, string market = null);
    }
}