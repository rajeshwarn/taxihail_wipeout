using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace apcurium.MK.Booking.Mobile.Settings
{
    public class AppSettingsData
    {
        public string ApplicationName { get; set; }
        public bool ErrorLogEnabled { get; set; }
        public string ErrorLog { get; set; }        

        public bool CanChangeServiceUrl{ get; set; }
        public string ServiceUrl { get; set; }

        public bool TwitterEnabled{ get; set; }
        public string TwitterConsumerKey { get; set; }
        public string TwitterCallback { get; set; }
        public string TwitterConsumerSecret { get; set; }
        public string TwitterRequestTokenUrl { get; set; }
        public string TwitterAccessTokenUrl { get; set; }
        public string TwitterAuthorizeUrl { get; set; }
           
        public bool FacebookEnabled { get; set; }
        public string FacebookAppId{ get; set; }

    }
}
