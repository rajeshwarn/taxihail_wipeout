using System.Linq;
using System.Web.Http;
using Swashbuckle.Application;

namespace apcurium.MK.Web
{
    public class SwaggerConfig
    {
        public static void Register()
        {
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
