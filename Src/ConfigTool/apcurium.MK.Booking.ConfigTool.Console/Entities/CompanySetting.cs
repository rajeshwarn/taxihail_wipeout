using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CustomerPortal.Web.Entities
{
    public class CompanySetting
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public bool IsClientSetting { get; set; }
    }
}