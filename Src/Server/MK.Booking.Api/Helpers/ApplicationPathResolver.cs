#region

using System;
using System.Web;
using System.Web.Hosting;
using System.Web.Http.Controllers;
using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Helpers
{
    public static class ApplicationPathResolver
    {
        public static string GetApplicationPath(HttpRequestContext httpRequest)
        {   
            var root = httpRequest.Url.Request.RequestUri.GetLeftPart(UriPartial.Authority);
            var aspNetRequest = httpRequest.Url.Request;
            if (aspNetRequest != null)
            {
                //We are in IIS
                //The ApplicationVirtualPath property always returns "/" as the first character of the returned value.
                //If the application is located in the root of the Web site, the return value is just "/".
                if (HostingEnvironment.ApplicationVirtualPath != null
                    && HostingEnvironment.ApplicationVirtualPath.Length > 1)
                {
                    root += HostingEnvironment.ApplicationVirtualPath;
                }
            }
            //else
            // We are probably in a test environment, using HttpListener
            // We Assume there is no virtual path
            return root;
        }
    }
}