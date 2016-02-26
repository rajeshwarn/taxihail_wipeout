using apcurium.MK.Common.Services;

namespace apcurium.MK.Common.Configuration.Impl
{
	public class MonerisPaymentSettings
	{
		public MonerisPaymentSettings()
		{
#if DEBUG
            StoreId = "store3";
            ApiToken = "yesguy";
            SandboxHost = "esqa.moneris.com";
            BaseHost = "www3.moneris.com";
			IsSandbox = true;
#endif
		}

		public bool IsSandbox { get; set; }

		[PropertyEncrypt]
        public string BaseHost { get; set; }

		[PropertyEncrypt]
        public string SandboxHost { get; set; }

		[PropertyEncrypt]
		public string Host
		{
			get { return IsSandbox 
				? SandboxHost 
					: BaseHost; }
		}

		[PropertyEncrypt]
		public string StoreId { get; set; }

		[PropertyEncrypt]
		public string ApiToken { get; set; }
	}
}

