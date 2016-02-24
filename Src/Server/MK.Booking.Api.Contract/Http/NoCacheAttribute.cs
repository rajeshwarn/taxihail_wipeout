#region

#if CLIENT
using System;
#else
using System;
using System.Web.Mvc;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
#endif
#endregion



namespace apcurium.MK.Booking.Api.Contract.Http
{
#if CLIENT
    public partial class NoCacheAttribute: Attribute {}
#else
    public class NoCacheAttribute : FilterAttribute, IResultFilter
    {
        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
            
        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
            filterContext.HttpContext.Response.Headers.Add("Cache-Control", "no-cache");
        }
    }
#endif
}