using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Common.Provider
{
    public interface IServiceTypeSettingsProvider
    {
        string GetIBSWebServicesUrl(ServiceType serviceType);
    }
}