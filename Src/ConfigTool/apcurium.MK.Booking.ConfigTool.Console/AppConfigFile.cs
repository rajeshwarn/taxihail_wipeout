using System;

namespace apcurium.MK.Booking.ConfigTool
{
    public class AppConfigFile
    {
        public string FacebookAppId { get; set; }
        public string AppName { get; set; }
        public string Package { get; set; }
        public string GoogleMapKey { get; set; }
        public string AndroidSigningKeyAlias { get; set; }
        public string AndroidSigningKeyPassStorePass { get; set; }
    }
}

