using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CustomerPortal.Web.Entities
{
    public class Style
    {
        public Style()
        {
            MenuColor = "#f2f2f2";
            EmailFontColor = "#000";
        }

        [Obsolete]
        public string NavigationBarColor { get; set; }
        public string TitleColor { get; set; }
        public string WebAccentColor { get; set; }

        public string CompanyColor { get; set; }
        public string MenuColor { get; set; }

        public string LoginColor { get; set; }
        public string EmailFontColor { get; set; }
    }
}