using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Data;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IApplicationInfoService
	{
        Task<ApplicationInfo> GetAppInfoAsync();

        void ClearAppInfo();

        Task CheckVersionAsync(VersionCheckTypes versionCheckType);
    }
}