using MK.Common.Configuration;

namespace apcurium.MK.Common.Configuration
{
	public interface IAppSettings
	{
		TaxiHailSetting Data { get; }
        ServerTaxiHailSetting ServerData { get; }
		void Load();
        void ChangeServerUrl(string serverUrl);
	}
}

