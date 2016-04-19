using System.Linq;
using System.Web.Http;
using WebActivatorEx;
using apcurium.MK.Web;
using Swashbuckle.Application;

//[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace apcurium.MK.Web
{
    public class SwaggerConfig
    {
        public static void Register()
        {
            //var thisAssembly = typeof(SwaggerConfig).Assembly;

            GlobalConfiguration.Configuration
                .EnableSwagger(c =>
                {
                    c.SingleApiVersion("v2", "Taxihail");
                    c.Schemes(new[] { "http", "https" });
                    c.DescribeAllEnumsAsStrings();
                    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                })
                .EnableSwaggerUi(c =>
                {
                    c.DisableValidator();
                    c.DocExpansion(DocExpansion.List);
                });
        }
    }
}
