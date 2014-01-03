#region

using System.ComponentModel.DataAnnotations;

#endregion

namespace apcurium.MK.Common.Configuration.Impl
{
    public class AppSetting
    {
        public AppSetting()
        {
        }

        public AppSetting(string key, string value)
        {
            Key = key;
            Value = value;
        }

        [Key]
        public string Key { get; set; }

        public string Value { get; set; }
    }
}