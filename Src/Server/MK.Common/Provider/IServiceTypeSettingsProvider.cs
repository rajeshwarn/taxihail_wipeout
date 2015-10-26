using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Enumeration;
using MK.Common.Configuration;

namespace apcurium.MK.Common.Provider
{
    public interface IServiceTypeSettingsProvider
    {
        IBSSettingContainer GetIBSSettings(ServiceType serviceType);
        ServiceTypeSettings GetSettings(ServiceType serviceType);
    }
}