﻿#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Resources;
using apcurium.MK.Common.Extensions;

#endregion

namespace apcurium.MK.Booking.Resources
{
    public class DynamicResources : DynamicObject
    {
        private readonly ResourceManager _resources;
        private const string DefaultLanguageCode = "en";

        public DynamicResources(string applicationKey)
        {
            applicationKey = "Thriev";
            var names = GetType().Assembly.GetManifestResourceNames();
            var resourceName = "apcurium.MK.Booking.Resources." + applicationKey + ".resources";

            MissingResourceFile = names.None(n => n.ToLower() == resourceName.ToLower());
            // resource files doesn't exist for all the customers

            _resources = new ResourceManager("apcurium.MK.Booking.Resources." + applicationKey, GetType().Assembly);
        }

        public bool MissingResourceFile { get; private set; }

        public string GetString(string key, string languageCode = DefaultLanguageCode)
        {
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
                result[(string)enumerator.Key] = GetString((string)enumerator.Key, languageCode);
            }

            return result;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            // we pass null since we only want to see if it exists
            result = GetString(binder.Name);
            return true;
        }
    }
}