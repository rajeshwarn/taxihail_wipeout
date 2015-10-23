#region

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Resources;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;

#endregion

namespace apcurium.MK.Booking.Resources
{
    public class Resources : DynamicObject
    {
        private readonly IServerSettings _serverSettings;
        private readonly ResourceManager _resources;
        private readonly string _applicationKey;
        private const string DefaultLanguageCode = "en";

        public Resources(IServerSettings serverSettings)
        {
            _serverSettings = serverSettings;
            
            var names = GetType().Assembly.GetManifestResourceNames();

            _applicationKey = serverSettings.ServerData.TaxiHail.ApplicationKey;
            var resourceName = "apcurium.MK.Booking.Resources." + _applicationKey + ".resources";

            MissingResourceFile = names.None(n => n.ToLower() == resourceName.ToLower());
            // resource files doesn't exist for all the customers

            _resources = new ResourceManager("apcurium.MK.Booking.Resources." + _applicationKey, GetType().Assembly);
        }

        public bool MissingResourceFile { get; private set; }

        public string Get(string key, string languageCode = DefaultLanguageCode, string context = "")
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
            return GetStringForContext(key, languageCode, context)
                    ?? _resources.GetString(key, CultureInfo.GetCultureInfo(languageCode))
                   ?? Global.ResourceManager.GetString(key, CultureInfo.GetCultureInfo(languageCode));
        }

        private string GetStringForContext(string key, string languageCode, string context)
        {
            if (!string.IsNullOrWhiteSpace(context))
            {
                var names = GetType().Assembly.GetManifestResourceNames();
                var contextualResourceSetName = string.Format("{0}.{1}-{2}.resources", _applicationKey ?? "Global", languageCode.Substring(0, 2), context);

                foreach (var name in names)
                {
                    if (name.Contains(contextualResourceSetName))
                    {
                        var resourceSetStream = GetType().Assembly.GetManifestResourceStream(name);
                        var resourceSet = new ResourceSet(resourceSetStream);
                        return resourceSet.GetString(key);
                    }
                }
            }

            return null;
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
            var culture = _serverSettings.ServerData.PriceFormat;
            var currencyPriceFormat = Get("CurrencyPriceFormat", culture);
            return string.Format(new CultureInfo(culture), currencyPriceFormat, price.HasValue ? price.Value : 0);
        }

        public string FormatDistance(double? distance)
        {
            if (distance.HasValue)
            {
                var distanceFormat = _serverSettings.ServerData.DistanceFormat;
                var roundedDistance = Math.Round(distance.Value, 1);

                return string.Format(distanceFormat == DistanceFormat.Km
                    ? "{0:n1} km"
                    : "{0:n1} Miles", roundedDistance);
            }

            return string.Empty;
        }

        public string GetCurrencyCode()
        {
            return new RegionInfo(_serverSettings.ServerData.PriceFormat).ISOCurrencySymbol;
        }
    }
}