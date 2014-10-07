#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

#endregion

namespace CustomerPortal.Web.HtmlHelpers
{
    public enum Position
    {
        Horizontal,
        Vertical,
    }

    public static class HtmlExtensions
    {
        public static IEnumerable<SelectListItem> ToSelectList(this Enum enumValue)
        {
            return from Enum e in Enum.GetValues(enumValue.GetType())
                select new SelectListItem
                {
                    Selected = e.Equals(enumValue),
                    Text = e.ToDescription(),
                    Value = e.ToString()
                };
        }

        public static string ToDescription(this Enum value)
        {
            var attributes = (DescriptionAttribute[]) value.GetType().GetField(
                value.ToString()).GetCustomAttributes(typeof (DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }

        public static MvcHtmlString RadioButtonForSelectList<TModel, TProperty>(
            this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> expression,
            IEnumerable<SelectListItem> listOfValues, Position position = Position.Horizontal)
        {
            var metaData = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            string fullName = ExpressionHelper.GetExpressionText(expression);
            var sb = new StringBuilder();

            if (listOfValues != null)
            {
                // Create a radio button for each item in the list 
                foreach (SelectListItem item in listOfValues)
                {
                    // Generate an id to be given to the radio button field 
                    var id = string.Format("rb_{0}_{1}",
                        fullName.Replace("[", "").Replace(
                            "]", "").Replace(".", "_"), item.Value);

                    // Create and populate a radio button using the existing html helpers 
                    var label = htmlHelper.Label(id, HttpUtility.HtmlEncode(item.Text));
                    //var radio = htmlHelper.RadioButtonFor(expression, item.Value, new { id = id }).ToHtmlString();
                    var radio = htmlHelper.RadioButton(fullName, item.Value, item.Selected, new {id}).ToHtmlString();

                    // Create the html string that will be returned to the client 
                    // e.g. <input data-val="true" data-val-required=
                    //   "You must select an option" id="TestRadio_1" 
                    //    name="TestRadio" type="radio"
                    //   value="1" /><label for="TestRadio_1">Line1</label> 
                    sb.AppendFormat("<{2} class=\"RadioButton\">{0}{1}</{2}>",
                        radio, label, (position == Position.Horizontal ? "span" : "div"));
                }
            }

            return MvcHtmlString.Create(sb.ToString());
        }
    }
}