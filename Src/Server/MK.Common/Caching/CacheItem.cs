#region

using System;
using System.ComponentModel.DataAnnotations;

#endregion

namespace apcurium.MK.Common.Caching
{
    public class CacheItem
    {
        public CacheItem()
        {
        }

        public CacheItem(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public CacheItem(string key, string value, DateTime expiresAt)
        {
            Key = key;
            Value = value;
            ExpiresAt = expiresAt;
        }

        [Key]
        public string Key { get; set; }

        public string Value { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}