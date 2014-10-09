#region

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc.Html;

#endregion

namespace System.Web.Mvc
{
    public static class HtmlExtensions
    {
        private static readonly SelectListItem[] SingleEmptyItem = {new SelectListItem {Text = "", Value = ""}};

        public static MvcHtmlString HelpLabelFor<TModel, TValue>(this HtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression)
        {
            return LabelHelper(html,
                ModelMetadata.FromLambdaExpression(expression, html.ViewData),
                ExpressionHelper.GetExpressionText(expression));
        }

        public static MvcHtmlString DisplayDescriptionFor<TModel, TValue>(this HtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression)
        {
            var metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            return new MvcHtmlString(HttpUtility.HtmlEncode(metadata.Description));
        }

        public static MvcHtmlString EnumDropDownListFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TEnum>> expression)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            Type enumType = GetNonNullableModelType(metadata);
            IEnumerable<TEnum> values = Enum.GetValues(enumType).Cast<TEnum>();

            IEnumerable<SelectListItem> items =
                values.Select(value => new SelectListItem
                {
                    Text = value.ToString(),
                    Value = value.ToString(),
                    Selected = value.Equals(metadata.Model)
                });

            if (metadata.IsNullableValueType)
            {
                items = SingleEmptyItem.Concat(items);
            }

            return htmlHelper.DropDownListFor(
                expression,
                items
                );
        }

        public static MvcHtmlString ValueNotSet<TModel>(this HtmlHelper<TModel> htmlHelper, string text = "not set")
        {
            return new MvcHtmlString("<i class='muted'>&lt;" + text + "&gt;</i>");
        }

        internal static MvcHtmlString LabelHelper(HtmlHelper html, ModelMetadata metadata, string htmlFieldName)
        {
            string resolvedLabelText = metadata.DisplayName ?? metadata.PropertyName ?? htmlFieldName.Split('.').Last();
            if (String.IsNullOrEmpty(resolvedLabelText))
            {
                return MvcHtmlString.Empty;
            }

            var tag = new TagBuilder("div");
            tag.Attributes.Add("for",
                TagBuilder.CreateSanitizedId(html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(htmlFieldName)));
            var tooltip = HttpUtility.HtmlEncode(metadata.Description);
            if (!string.IsNullOrEmpty(tooltip))
            {
                tag.InnerHtml = "<label>" + HttpUtility.HtmlEncode(resolvedLabelText)
                                + "</label><p class=help-block><small>" + tooltip + "</small></p>";
            }
            else
            {
                tag.InnerHtml = "<label>" + HttpUtility.HtmlEncode(resolvedLabelText) + "</label>";
            }
            return new MvcHtmlString(tag.ToString(TagRenderMode.Normal));
        }

        private static Type GetNonNullableModelType(ModelMetadata modelMetadata)
        {
            Type realModelType = modelMetadata.ModelType;

            Type underlyingType = Nullable.GetUnderlyingType(realModelType);
            if (underlyingType != null)
            {
                realModelType = underlyingType;
            }
            return realModelType;
        }
    }
}