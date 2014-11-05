#region

using System.Web.Mvc;
using System.Web.Routing;

#endregion

namespace CustomerPortal.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("Manifest", "Distribution/Manifest/{id}/{number}/{package}/manifest.plist",
                new {controller = "Distribution", action = "Manifest"}, new[] {"CustomerPortal.Web.Controllers"}
                );

            routes.MapRoute("Package", "Distribution/Package/{id}/{number}/{package}",
                new {controller = "Distribution", action = "Package"}, new[] {"CustomerPortal.Web.Controllers"}
                );

            routes.MapRoute("Distribution", "Distribution/Index/{id}",
              new { controller = "Distribution", action = "Index" }, new[] { "CustomerPortal.Web.Controllers" }
              );

            routes.MapRoute("Default", "{controller}/{action}/{id}",
                new {controller = "Home", action = "Index", id = UrlParameter.Optional},
                new[] {"CustomerPortal.Web.Controllers"}
                );
        }
    }
}