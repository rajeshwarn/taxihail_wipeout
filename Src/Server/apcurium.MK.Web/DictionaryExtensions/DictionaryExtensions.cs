using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace apcurium.MK.Web.Extensions
{
    public static class DictionaryExtensions
    {
        public static Dictionary<string, string> ToDictionary(this NameValueCollection nameValues)
        {
            return nameValues.AllKeys
                .Where(key => key != null)
                .Select(key => new KeyValuePair<string, string>(key, nameValues.Get(key)))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}