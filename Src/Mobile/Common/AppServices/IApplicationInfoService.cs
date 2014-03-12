using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IApplicationInfoService
	{
        Task<ApplicationInfo> GetAppInfoAsync();

        void ClearAppInfo();

        void CheckVersionAsync();
        
    }
}

