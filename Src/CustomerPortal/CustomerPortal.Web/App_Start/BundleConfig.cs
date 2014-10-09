#region

using System.Web.Optimization;

#endregion

namespace CustomerPortal.Web
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-{version}.js",
                "~/assets/js/jquery.highlight-curly-brackets.js",
                "~/assets/js/jquery.fetch-version.js",
                "~/assets/js/tipsy.js",                
                "~/assets/js/jquery.fetch-status.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                "~/Scripts/jquery.unobtrusive*",
                "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/jquery-fileupload").Include(
                "~/assets/jquery-fileupload/js/vendor/jquery.ui.widget.js",
                "~/assets/jquery-fileupload/js/jquery.iframe-transport.js",
                "~/assets/jquery-fileupload/js/jquery.fileupload.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap/js").Include(
                "~/assets/bootstrap/js/bootstrap-tab.js",
                "~/assets/bootstrap/js/bootstrap-tooltip-custom.js",
                "~/assets/bootstrap/js/bootstrap-datepicker.js"));

            var lessBundle = new Bundle("~/bundles/bootstrap/less").Include(
                "~/assets/less/portal.less",
                "~/assets/css/tipsy.css",
                "~/assets/jquery-fileupload/css/jquery.fileupload-ui.css");


            lessBundle.Transforms.Add(new LessTransform());
            lessBundle.Transforms.Add(new CssMinify());


            bundles.Add(lessBundle);
        }
    }
}