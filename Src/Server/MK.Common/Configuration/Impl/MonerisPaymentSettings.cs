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

        public string BaseHost { get; set; }
        public string SandboxHost { get; set; }

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

