using MK.Common.Configuration;

namespace apcurium.MK.Common.Configuration
{
    public interface IServerSettings
    {
        ServerTaxiHailSetting Data { get; }
        void Load();
    }
}