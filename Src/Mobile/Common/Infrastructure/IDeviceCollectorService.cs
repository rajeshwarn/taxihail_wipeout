using System;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
	public abstract class BaseDeviceCollectorService : IDeviceCollectorService
	{
	    private readonly IAppSettings _settings;

	    public BaseDeviceCollectorService(IAppSettings settings)
	    {
	        _settings = settings;
	    }

	    private string SandboxDeviceCollectorUrl = "https://tst.kaptcha.com/logo.htm";
        private string SandboxMerchantId = "160700";
        private string ProductionDeviceCollectorUrl = "https://tst.kaptcha.com/logo.htm";
        private string ProductionMerchantId = "160700";

        public abstract string CollectAndReturnSessionId();

		public string GenerateSessionId()
		{
			return Guid.NewGuid().ToString("N");
		}

	    protected string DeviceCollectorUrl()
	    {
	        if (_settings.Data.Kount.UseSandbox)
	        {
	            return SandboxDeviceCollectorUrl;
	        }

	        return ProductionDeviceCollectorUrl;
	    }

	    protected string MerchantId()
	    {
            if (_settings.Data.Kount.UseSandbox)
            {
                return SandboxMerchantId;
            }

            return ProductionMerchantId;
        }
	}

	public interface IDeviceCollectorService
	{
		string CollectAndReturnSessionId();
	}
}
