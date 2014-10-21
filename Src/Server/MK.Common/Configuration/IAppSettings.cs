using MK.Common.Configuration;
using System.Threading.Tasks;

namespace apcurium.MK.Common.Configuration
{
	public interface IAppSettings
	{
		TaxiHailSetting Data { get; }
        
		Task Load();
        Task ChangeServerUrl(string serverUrl);
	}
}

