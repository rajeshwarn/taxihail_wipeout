using System;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
	public abstract class BaseDeviceCollectorService : IDeviceCollectorService
	{
	    private readonly IAppSettings _settings;

		protected string SessionId = null;

	    protected BaseDeviceCollectorService(IAppSettings settings)
	    {
	        _settings = settings;
	    }

	    private const string SandboxDeviceCollectorUrl = "https://tst.kaptcha.com/logo.htm";
		private const string SandboxMerchantId = "160700";
		private const string ProductionDeviceCollectorUrl = "https://tst.kaptcha.com/logo.htm";
		private const string ProductionMerchantId = "160700";

		public abstract void GenerateNewSessionIdAndCollect();

		public string GetSessionId()
		{
			return SessionId;
		}

		protected void GenerateSessionId()
		{
			SessionId = Guid.NewGuid().ToString("N");
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
		/// <summary>
		/// Generates a new Kount session id for the possible next call in the future
		/// Kount can take up to 15sec to collect the data, so we need to call it way before we need it
		/// </summary>
		void GenerateNewSessionIdAndCollect();

		/// <summary>
		/// Gets the last generated Kount session id
		/// </summary>
		/// <returns>The session identifier.</returns>
		string GetSessionId();
	}
}
