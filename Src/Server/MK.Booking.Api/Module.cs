using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using ServiceStack.CacheAccess;
using ServiceStack.CacheAccess.Providers;

namespace apcurium.MK.Booking.Api
{
    public class Module
    {
        public void Init(IUnityContainer container)
        {
            container.RegisterInstance<ICacheClient>(new MemoryCacheClient { FlushOnDispose = false });

        }
    }
}
