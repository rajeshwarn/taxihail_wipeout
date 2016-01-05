using System;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
	public abstract class BaseDeviceCollectorService : IDeviceCollectorService
	{
		protected string DeviceCollectorUrl = "https://tst.kaptcha.com/";
		protected string MerchantId = "160700";

        public abstract string CollectAndReturnSessionId();

		public string GenerateSessionId()
		{
			return Guid.NewGuid().ToString("N");
		}
	}

	public interface IDeviceCollectorService
	{
		string CollectAndReturnSessionId();
	}
}