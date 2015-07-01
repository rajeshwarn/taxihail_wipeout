using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GridMvc.Columns;

namespace System.Web.Mvc
{
    public static class GridExtensions
    {
        public static IGridColumn<T> IsCustomHtmlColumn<T>(this IGridColumn<T> column, bool isCustom)
        {
            return column
                .Sanitized(!isCustom)
                .Encoded(!isCustom);
        }
    }
}