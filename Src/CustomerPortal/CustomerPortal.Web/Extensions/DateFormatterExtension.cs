using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CustomerPortal.Web.Extensions
{
    public static class DateFormatterExtension
    {
        public static string FormatDate(this DateTime? date)
        {
            if (!date.HasValue)
            {
                return "n/a";
            }
            else
            {
                return date.Value.ToShortDateString();
            }
        }
    }
}