using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.IBS
{
    public interface IIBSServiceProvider
    {
        IAccountWebServiceClient Account(string companyKey = null, string market = null);
        IStaticDataWebServiceClient StaticData(string companyKey = null, string market = null);
        IBookingWebServiceClient Booking(string companyKey = null, string market = null);
        IBSSettingContainer GetSettingContainer(string companyKey, string market);
    }
}