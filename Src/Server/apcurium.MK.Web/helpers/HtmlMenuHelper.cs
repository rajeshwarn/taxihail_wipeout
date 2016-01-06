﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
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

    public static class HiddenExtensions
    {
        public static MvcHtmlString HiddenForEnumerable<TModel, TProperty>(this HtmlHelper<TModel> helper, Expression<Func<TModel, IEnumerable<TProperty>>> expression)
        {
            var sb = new StringBuilder();

            var membername = expression.GetMemberName();
            var model = helper.ViewData.Model;
            var list = expression.Compile()(model);

            if (list != null)
            {
                for (var i = 0; i < list.Count(); i++)
                {
                    var item = list.ElementAt(i);

                    var properties = new Dictionary<string, object>();
                    IterateProps(item, string.Format("{0}[{1}]", membername, i), ref properties);

                    foreach (var prop in properties)
                    {
                        sb.Append(helper.Hidden(prop.Key, prop.Value));
                    }
                }
            }

            return new MvcHtmlString(sb.ToString());
        }

        private static void IterateProps(object obj, string baseProperty, ref Dictionary<string, object> properties)
        {
            if (obj != null)
            {
                var baseType = obj.GetType();
                var props = baseType.GetProperties();
                foreach (var property in props)
                {
                    var name = property.Name;
                    var propType = property.PropertyType;
                    if (propType.IsClass && propType.Name != "String")
                    {
                        IterateProps(property.GetValue(obj, null), baseProperty + "." + property.Name, ref properties);
                    }
                    else
                    {
                        properties.Add(baseProperty + "." + name, property.GetValue(obj, null));
                    }
                }
            }
        }

        private static string GetMemberName<TModel, T>(this Expression<Func<TModel, T>> input)
        {
            if (input == null)
            {
                return null;
            }

            if (input.Body.NodeType != ExpressionType.MemberAccess)
            {
                return null;
            }

            var memberExp = input.Body as MemberExpression;
            return memberExp != null ? memberExp.Member.Name : null;
        }
    }
}