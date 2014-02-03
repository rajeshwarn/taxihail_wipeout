using MK.Common.Configuration;

namespace apcurium.MK.Common.Configuration
{
	public interface IAppSettings
	{
		TaxiHailSetting Data { get; }
		void Load();
	}
}

