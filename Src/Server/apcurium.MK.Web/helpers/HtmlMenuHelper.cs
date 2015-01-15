using System;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace apcurium.MK.Web.helpers
{
    public static class HtmlMenuHelper
    {
        /// <summary>
        /// Extension method that applies a "selected" style to the current menu item.
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="text"></param>
        /// <param name="action"></param>
        /// <param name="controller"></param>
        /// <returns></returns>
        public static MvcHtmlString MenuLink(this HtmlHelper helper, string text, string action, string controller)
        {
            var routeData = helper.ViewContext.RouteData.Values;
            var currentController = routeData["controller"];
            var currentAction = routeData["action"];

            if (string.Equals(action, currentAction as string, StringComparison.OrdinalIgnoreCase)
                && string.Equals(controller, currentController as string, StringComparison.OrdinalIgnoreCase))
            {
                return helper.ActionLink( text, action, controller, null, new { @class = "active" });
            }
            return helper.ActionLink(text, action, controller);
        }
    }
}