﻿using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using System;

namespace apcurium.MK.Booking.Api.Contract.Http
{
#if CLIENT
    public partial class NoCacheAttribute: Attribute {}
#else 
    public partial class NoCacheAttribute : ResponseFilterAttribute
    {
        public NoCacheAttribute(ApplyTo applyTo)
            :base(applyTo)
        {
            
        }

        public NoCacheAttribute()
        {
            
        }

        public override void Execute(IHttpRequest req, IHttpResponse res, object requestDto)
        {
            res.AddHeader("Cache-Control", "no-cache");
        }
    }
#endif
}
