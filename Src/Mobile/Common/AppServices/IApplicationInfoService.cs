using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public enum VersionCheck
	{
		CheckUpdates,
		CheckMinimumSupportedVersion
	}

	public interface IApplicationInfoService
	{
        Task<ApplicationInfo> GetAppInfoAsync();

        void ClearAppInfo();

        Task CheckVersionAsync(VersionCheck versionCheck);
    }
}