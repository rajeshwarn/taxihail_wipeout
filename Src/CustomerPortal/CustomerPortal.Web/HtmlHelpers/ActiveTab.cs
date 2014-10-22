#region

using System;
using System.Web.Mvc;
using System.Web.Mvc.Html;

#endregion

namespace CustomerPortal.Web.HtmlHelpers
{
    public static class ActiveTab
    {
        public static MvcHtmlString MenuItem(this HtmlHelper helper, string linkText, string actionName,
            string controllerName, string id)
        {
            var currentControllerName = (string) helper.ViewContext.RouteData.Values["controller"];
            var currentActionName = (string) helper.ViewContext.RouteData.Values["action"];

            var builder = new TagBuilder("li");
            if (currentControllerName.Equals(controllerName, StringComparison.CurrentCultureIgnoreCase) &&
                currentActionName.Equals(actionName, StringComparison.InvariantCultureIgnoreCase))
                builder.AddCssClass("active");
            builder.InnerHtml = helper.ActionLink(linkText, actionName, controllerName, new {id}, null).ToHtmlString();
            return MvcHtmlString.Create(builder.ToString(TagRenderMode.Normal));
        }
    }
}