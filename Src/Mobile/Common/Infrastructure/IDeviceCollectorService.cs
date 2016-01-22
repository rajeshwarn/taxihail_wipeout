using System;
using apcurium.MK.Booking.Mobile.AppServices;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
	public abstract class BaseDeviceCollectorService : IDeviceCollectorService
	{
		protected readonly IPaymentService PaymentService;

		protected string SessionId = null;

		protected BaseDeviceCollectorService(IPaymentService paymentService)
		{
			PaymentService = paymentService;
		}

		private const string SandboxDeviceCollectorUrl = "https://tst.kaptcha.com/logo.htm";
		private const string ProductionDeviceCollectorUrl = "https://ssl.kaptcha.com/logo.htm";
		protected const string MerchantId = "160700";

		public abstract Task GenerateNewSessionIdAndCollect();

		public string GetSessionId()
		{
			return SessionId;
		}

		protected void GenerateSessionId()
		{
			SessionId = Guid.NewGuid().ToString("N");
		}

		protected string DeviceCollectorUrl(bool isSandbox)
		{
			return isSandbox
				? SandboxDeviceCollectorUrl
				: ProductionDeviceCollectorUrl;
		}
	}

	public interface IDeviceCollectorService
	{
		/// <summary>
		/// Generates a new Kount session id for the possible next call in the future
		/// Kount can take up to 15sec to collect the data, so we need to call it way before we need it.
		/// NOTE: Should only be used with CMT Payment since collecting is only one step of the Kount process, 
		/// there needs to be a server-side call (done by CMT) to verify the data
		/// </summary>
		Task GenerateNewSessionIdAndCollect();

		/// <summary>
		/// Gets the last generated Kount session id
		/// </summary>
		/// <returns>The session identifier.</returns>
		string GetSessionId();
	}
}
