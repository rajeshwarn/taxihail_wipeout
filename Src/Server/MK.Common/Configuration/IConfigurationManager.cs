using System.Collections.Generic;
using MK.Common.Android.Configuration.Impl;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Common.Configuration
{
    public interface IConfigurationManager
    {
        void Reset();
        string GetSetting( string key );
        
        IDictionary<string, string> GetSettings();
        void SetSettings(IDictionary<string, string> appSettings);


        ClientPaymentSettings GetPaymentSettings();

    }

    public class ClientPaymentSettings
    {
		public ClientPaymentSettings ()
		{
			CmtPaymentSettings = new CmtPaymentSettings();
		}
        public PaymentMethod PaymentMode { get; set; }
		public CmtPaymentSettings CmtPaymentSettings { get; set; }
    }
}
