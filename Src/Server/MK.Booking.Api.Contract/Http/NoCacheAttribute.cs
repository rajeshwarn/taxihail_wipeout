using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Http
{
#if CLIENT
    public partial class NoCacheAttribute: Attribute {}
#else 
    public partial class NoCacheAttribute : ResponseFilterAttribute
    {
        public override void Execute(IHttpRequest req, IHttpResponse res, object requestDto)
        {
            res.AddHeader("Cache-Control", "no-cache");
        }
    }
#endif
}
