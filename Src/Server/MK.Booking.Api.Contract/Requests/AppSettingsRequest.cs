using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RestService("/appsettings/{SettingKey}", "GET")]    
    public class AppSettingsRequest
    {
        public string SettingKey { get; set; }
    }
}
