using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Common.Provider
{
    public interface IServiceTypeSettingsProvider
    {
        IBSSettingContainer GetIBSSettings(ServiceType serviceType);
    }
}