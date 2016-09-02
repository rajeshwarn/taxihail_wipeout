using System.Threading.Tasks;
using MK.Common.Configuration;

namespace apcurium.MK.Common.Configuration
{
	public interface IAppSettings
	{
		TaxiHailSetting Data { get; }
        
		Task Load();
        Task ChangeServerUrl(string serverUrl, bool skipAppRestart = false);

		string GetServiceUrl();

		void SetAppleTestAccountMode(bool isAppleTestAccountUsed);
	}
}

