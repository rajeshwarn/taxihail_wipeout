#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Resources;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;

#endregion

namespace apcurium.MK.Booking.Resources
{
    public class Resources : DynamicObject
    {
        private readonly IConfigurationManager _configManager;
        private readonly ResourceManager _resources;
        private const string DefaultLanguageCode = "en";

        public Resources(IConfigurationManager configManager)
        {
            _configManager = configManager;
            
            var names = GetType().Assembly.GetManifestResourceNames();

            var applicationKey = configManager.ServerData.TaxiHail.ApplicationKey;
            var resourceName = "apcurium.MK.Booking.Resources." + applicationKey + ".resources";

            MissingResourceFile = names.None(n => n.ToLower() == resourceName.ToLower());
            // resource files doesn't exist for all the customers

            _resources = new ResourceManager("apcurium.MK.Booking.Resources." + applicationKey, GetType().Assembly);
        }

        public bool MissingResourceFile { get; private set; }

        public string Get(string key, string languageCode = DefaultLanguageCode)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key");
            }

            if (string.IsNullOrWhiteSpace(languageCode))
            {
                languageCode = DefaultLanguageCode;
            }

            if (MissingResourceFile)
            {
                return Global.ResourceManager.GetString(key, CultureInfo.GetCultureInfo(languageCode));
            }
            return _resources.GetString(key, CultureInfo.GetCultureInfo(languageCode))
                   ?? Global.ResourceManager.GetString(key, CultureInfo.GetCultureInfo(languageCode));
        }

        public Dictionary<string, string> GetLocalizedDictionary(string languageCode)
        {
            // we take the ResourceSet from the Main file because it contains all the keys
            var resourceSet = Global.ResourceManager.GetResourceSet(CultureInfo.InvariantCulture, true, true);

            var enumerator = resourceSet.GetEnumerator();
            var result = new Dictionary<string, string>();
            while (enumerator.MoveNext())
            {
                result[(string)enumerator.Key] = Get((string)enumerator.Key, languageCode);
            }

            return result;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            // we pass null since we only want to see if it exists
            result = Get(binder.Name);
            return true;
        }

        public string FormatPrice(double? price)
        {
            var culture = _configManager.ServerData.PriceFormat;
            var currencyPriceFormat = Get("CurrencyPriceFormat", culture);
            return string.Format(new CultureInfo(culture), currencyPriceFormat, price.HasValue ? price.Value : 0);
        }
    }
}