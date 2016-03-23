using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using apcurium.MK.Booking.Api.Contract.Controllers;
using apcurium.MK.Booking.Api.Controllers;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.IoC;

namespace apcurium.MK.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration builder)
        {
            builder.MapHttpAttributeRoutes();

            builder.MessageHandlers.Add(new TaxihailApiHttpHandler());

            builder.Filters.Add(new ValidationFilterAttribute());

            builder.EnableSystemDiagnosticsTracing();
        }
        
    }
}