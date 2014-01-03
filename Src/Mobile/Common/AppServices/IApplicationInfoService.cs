using apcurium.MK.Booking.Api.Contract.Resources;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IApplicationInfoService
	{
        Task<ApplicationInfo> GetAppInfoAsync();

        void ClearAppInfo();

        void CheckVersionAsync();
        
    }
}

