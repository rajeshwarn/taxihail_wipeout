namespace apcurium.MK.Common.Configuration.Impl
{
	public class MonerisPaymentSettings
	{
		public MonerisPaymentSettings()
		{
#if DEBUG
			StoreId = "moneris";
			ApiToken = "hurgle";
			SandboxHost = "esqa.moneris.com";
			BaseHost = "esqa.moneris.com";
			IsSandbox = true;
#endif
		}

		public bool IsSandbox { get; set; }

		private string BaseHost { get; set; }
		private string SandboxHost { get; set; }
		public string Host
		{
			get { return IsSandbox 
				? SandboxHost 
					: BaseHost; }
		}

		public string StoreId { get; set; }
		public string ApiToken { get; set; }
	}
}

