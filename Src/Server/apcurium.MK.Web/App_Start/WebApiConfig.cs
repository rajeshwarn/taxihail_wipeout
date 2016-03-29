using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Controllers;
using apcurium.MK.Booking.Api.Controllers;

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