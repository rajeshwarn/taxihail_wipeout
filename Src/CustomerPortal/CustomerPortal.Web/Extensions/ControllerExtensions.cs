#region

using System;
using System.IO;
using System.Web.Mvc;

#endregion

namespace CustomerPortal.Web.Extensions
{
    public static class ControllerExtensions
    {
        public static string RenderPartialViewToString(this Controller controller, string viewName, object model)
        {
            controller.ViewData.Model = model;
            try
            {
                using (var sw = new StringWriter())
                {
                    ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext,
                        viewName);
                    var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData,
                        controller.TempData, sw);
                    viewResult.View.Render(viewContext, sw);

                    return sw.GetStringBuilder().ToString();
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
    }
}