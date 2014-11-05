#region

using System.Collections.Generic;
using CustomerPortal.Web.Entities;
using System;

#endregion

namespace CustomerPortal.Web.Areas.Admin.Models
{
    public class InitSettingsModel
    {
        public string CityInfo { get; set; }
        public Company Company { get; set; }
        public Dictionary<string, Value> Settings { get; set; }
    }

    public class Value
    {
        public Value()
        {

        }

        public Value(string s, bool b)
        {
            StringValue = s;
            BoolValue = b;
        }

        public string StringValue { get; set; }
        public bool BoolValue { get; set; }

    }
}