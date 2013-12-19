using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Entity;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.Api.Contract.Requests
{    
    [Authenticate]
#if !CLIENT
    [AuthorizationRequired(ApplyTo.Post, RoleName.Admin)]
#endif
    [Route("/admin/pushnotifications/{EmailAddress}", "POST")]    
    public class PushNotificationAdministrationRequest
    {
        public string EmailAddress { get; set; }
        public string Message { get; set; }        
    }
}
