using apcurium.MK.Common.Configuration;
using apcurium.MK.Booking.IBS.ChargeAccounts;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.IBS
{
    public interface IIBSServiceProvider
    {
        IAccountWebServiceClient Account(string companyKey = null);

        IStaticDataWebServiceClient StaticData(string companyKey = null, ServiceType? serviceType = null);

        IBookingWebServiceClient Booking(string companyKey = null);

        IBSSettingContainer GetSettingContainer(string companyKey = null, ServiceType? serviceType = null);

        IChargeAccountWebServiceClient ChargeAccount(string companyKey = null);
    }
}